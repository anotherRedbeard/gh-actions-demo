# Production Deployment Guide

This guide provides comprehensive instructions for deploying the Budget Tracker application to production on Azure using GitHub Actions.

## Table of Contents

- [Production Architecture](#production-architecture)
- [Prerequisites](#prerequisites)
- [Azure Infrastructure Setup](#azure-infrastructure-setup)
- [GitHub Configuration](#github-configuration)
- [Deployment Process](#deployment-process)
- [Runtime Configuration](#runtime-configuration)
- [Monitoring and Observability](#monitoring-and-observability)
- [Rollback Procedures](#rollback-procedures)
- [Go-Live Checklist](#go-live-checklist)
- [Troubleshooting](#troubleshooting)

---

## Production Architecture

The Budget Tracker application consists of two main components deployed to Azure:

### Frontend - Web App
- **Service**: Azure Web App
- **App Name**: `red-scus-budget`
- **Framework**: ASP.NET Core MVC (.NET 9)
- **Region**: South Central US
- **Plan**: Production App Service Plan
- **Deployment Slot**: Production

### Backend - Function App
- **Service**: Azure Function App (Flex Consumption)
- **App Name**: `red-scus-budgetbackend-demo`
- **Framework**: Azure Functions V4 (.NET Isolated Process Model)
- **Region**: South Central US
- **Resource Group**: `red-scus-ghactions-demo-rg`
- **Plan**: Flex Consumption (FC1)
- **Deployment Slot**: Production

### Communication Flow
```
Internet → Azure Web App (red-scus-budget)
              ↓ (API calls via API_BASE_URL)
          Azure Function App (red-scus-budgetbackend-demo)
```

---

## Prerequisites

### Azure Resources

Before deploying, ensure the following Azure resources are created:

1. **Resource Group**: `red-scus-ghactions-demo-rg` (South Central US)

2. **Azure Web App** (`red-scus-budget`):
   - Operating System: Windows or Linux
   - Runtime: .NET 9
   - App Service Plan: Standard or Premium tier recommended

3. **Azure Function App** (`red-scus-budgetbackend-demo`):
   - Operating System: Linux (recommended for Flex Consumption)
   - Runtime: .NET 9 Isolated
   - Hosting Plan: Flex Consumption (FC1)
   - Storage Account: Required for Function App deployment

4. **Application Insights** (recommended):
   - For monitoring and diagnostics
   - Should be linked to both Web App and Function App

### GitHub Environment

Create a GitHub environment named **`prod`** in your repository:

1. Navigate to: **Settings** → **Environments** → **New environment**
2. Name: `prod`
3. (Optional) Configure protection rules:
   - Required reviewers for production deployments
   - Deployment branches: `main` only

---

## Azure Infrastructure Setup

### 1. Create Azure AD Application and Service Principal

Create a service principal for GitHub Actions authentication:

```bash
# Set variables
SUBSCRIPTION_ID="your-subscription-id"
RESOURCE_GROUP="red-scus-ghactions-demo-rg"
APP_NAME="gh-actions-budget-demo"

# Create Azure AD app registration and service principal
az ad sp create-for-rbac \
  --name "$APP_NAME" \
  --role contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP
```

**Important**: Save the output - you'll need:
- `appId` (CLIENT_ID)
- `tenant` (TENANT_ID)
- The subscription ID

### 2. Configure Federated Credentials (OIDC)

Set up OpenID Connect (OIDC) authentication for GitHub Actions (recommended over secrets):

```bash
# Get the Application (Client) ID from previous step
APPLICATION_ID="<appId from previous command>"

# Create federated credential for production environment
az ad app federated-credential create \
  --id $APPLICATION_ID \
  --parameters '{
    "name": "github-federated-credential-prod",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:anotherRedbeard/gh-actions-demo:environment:prod",
    "audiences": ["api://AzureADTokenExchange"],
    "description": "GitHub Actions federated credential for production deployments"
  }'
```

**Subject Format**: The subject **must** match `repo:<OWNER>/<REPO>:environment:<ENVIRONMENT_NAME>`
- For this repository: `repo:anotherRedbeard/gh-actions-demo:environment:prod`

### 3. Verify RBAC Permissions

Ensure the service principal has appropriate permissions:

```bash
# Verify contributor role assignment
az role assignment list \
  --assignee $APPLICATION_ID \
  --scope /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP \
  --output table
```

---

## GitHub Configuration

### Required GitHub Secrets

Add the following secrets to your GitHub repository:

**Path**: Repository **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

| Secret Name | Description | Where to Find |
|------------|-------------|---------------|
| `AZURE_CLIENT_ID` | Application (Client) ID | Output from `az ad sp create-for-rbac` (`appId`) |
| `ENTRA_TENANT_ID` | Azure AD Tenant ID | Output from `az ad sp create-for-rbac` (`tenant`) |
| `AZURE_SUBSCRIPTION_ID` | Azure Subscription ID | `az account show --query id -o tsv` |

**Security Note**: With OIDC federated credentials, you do NOT need to store client secrets in GitHub.

### Workflow Files

The repository includes two GitHub Actions workflows:

#### 1. Frontend Deployment Workflow
- **File**: `.github/workflows/main_red-scus-budget.yml`
- **Deploys to**: Azure Web App (`red-scus-budget`)
- **Authentication**: OIDC via Azure/login@v1
- **Uses secrets**: `AZURE_CLIENT_ID`, `ENTRA_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`
- **Environment**: `prod`

#### 2. Backend Deployment Workflow
- **File**: `.github/workflows/deploy-function-app.yml`
- **Deploys to**: Azure Function App (`red-scus-budgetbackend-demo`)
- **Authentication**: OIDC via Azure/login@v1
- **Uses secrets**: `AZURE_CLIENT_ID`, `ENTRA_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`
- **Environment**: `prod`

---

## Deployment Process

### Automated Deployments (CI/CD)

#### Frontend Deployment Trigger
Automatically triggered when:
- Code is pushed to `main` branch
- Changes are detected in: `src/frontend/**` or `.github/workflows/main_red-scus-budget.yml`

**Process**:
1. Checkout code
2. Setup .NET 9 SDK
3. Build application
4. Publish application
5. Upload build artifact
6. Authenticate to Azure (OIDC)
7. Deploy to Azure Web App

#### Backend Deployment Trigger
Automatically triggered when:
- Code is pushed to `main` branch
- Changes are detected in: `src/backend/**` or `.github/workflows/deploy-function-app.yml`

**Process**:
1. Checkout code
2. Setup .NET 9 SDK
3. Restore dependencies
4. Build application
5. Publish Function App
6. Create `.azurefunctions` metadata (required for Flex Consumption)
7. Upload build artifact
8. Authenticate to Azure (OIDC)
9. Deploy to Azure Function App

### Manual Deployment

Both workflows can be triggered manually via workflow dispatch:

1. Navigate to: **Actions** tab in GitHub
2. Select the workflow:
   - **Build and deploy Frontend Budget App** (for frontend)
   - **Build and Deploy Backend Function App** (for backend)
3. Click **Run workflow**
4. Select branch: `main`
5. Click **Run workflow**

**Note**: Manual deployments still require approval if environment protection rules are configured.

### Deployment Order

When deploying both components, follow this order:

1. **Deploy Backend First**: Ensures API endpoints are available
2. **Deploy Frontend Second**: Frontend can connect to updated backend immediately

---

## Runtime Configuration

### Web App Configuration (Frontend)

The frontend Web App requires the following application setting:

#### API_BASE_URL

**Purpose**: Specifies the base URL for backend API calls

**Value**: `https://red-scus-budgetbackend-demo.azurewebsites.net/api/`

**Configure via Azure Portal**:
1. Navigate to: **Azure Portal** → **App Services** → `red-scus-budget`
2. Select: **Settings** → **Configuration** → **Application settings**
3. Click: **+ New application setting**
   - **Name**: `API_BASE_URL`
   - **Value**: `https://red-scus-budgetbackend-demo.azurewebsites.net/api/`
4. Click: **OK** → **Save**
5. Restart the Web App

**Configure via Azure CLI**:
```bash
az webapp config appsettings set \
  --name red-scus-budget \
  --resource-group red-scus-ghactions-demo-rg \
  --settings API_BASE_URL="https://red-scus-budgetbackend-demo.azurewebsites.net/api/"
```

**Verification**:
After deployment, check that the frontend can communicate with the backend:
1. Open: `https://red-scus-budget.azurewebsites.net`
2. Navigate to the Dashboard
3. Verify that budgets, transactions, and savings goals load correctly

### Function App Configuration (Backend)

The backend Function App uses default configuration but may require:

#### CORS Settings (if frontend is on different domain)

**Configure via Azure Portal**:
1. Navigate to: **Azure Portal** → **Function App** → `red-scus-budgetbackend-demo`
2. Select: **API** → **CORS**
3. Add allowed origins:
   - `https://red-scus-budget.azurewebsites.net`
   - (Optional) `http://localhost:5001` for local development
4. Click: **Save**

#### Function-Level Authentication (Optional)

For production security, consider enabling function-level authentication:
1. Navigate to: **Azure Portal** → **Function App** → `red-scus-budgetbackend-demo`
2. Select: **Settings** → **Configuration** → **Function runtime settings**
3. Set **Function app edit mode** to **Read/Write** or **Read only** based on requirements

---

## Monitoring and Observability

### Application Insights

Both the Web App and Function App should be configured with Application Insights for comprehensive monitoring.

#### Enable Application Insights

**For Web App**:
```bash
az monitor app-insights component create \
  --app budget-tracker-insights \
  --location southcentralus \
  --resource-group red-scus-ghactions-demo-rg \
  --application-type web

# Link to Web App
az webapp config appsettings set \
  --name red-scus-budget \
  --resource-group red-scus-ghactions-demo-rg \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="<connection-string>"
```

**For Function App**:
```bash
# Link to Function App
az functionapp config appsettings set \
  --name red-scus-budgetbackend-demo \
  --resource-group red-scus-ghactions-demo-rg \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="<connection-string>"
```

#### Key Metrics to Monitor

**Web App (Frontend)**:
- HTTP response times
- Failed requests (4xx, 5xx errors)
- CPU and memory usage
- User sessions and page views

**Function App (Backend)**:
- Function execution count
- Function execution duration
- Function failures
- HTTP trigger response times
- Flex Consumption scale metrics

#### Recommended Alerts

Set up alerts for:
1. **High Error Rate**: When error rate exceeds 5%
2. **Slow Response Times**: When P95 response time exceeds 2 seconds
3. **High CPU Usage**: When CPU exceeds 80% for 5 minutes
4. **Function Failures**: When function failure rate exceeds threshold

**Create Alert via Azure Portal**:
1. Navigate to: **Application Insights** → **Alerts** → **+ Create** → **Alert rule**
2. Define conditions, action groups, and notification methods

#### Log Analytics Queries

**View recent errors**:
```kusto
requests
| where success == false
| where timestamp > ago(1h)
| order by timestamp desc
| take 50
```

**Function execution statistics**:
```kusto
requests
| where timestamp > ago(24h)
| summarize 
    TotalCalls = count(),
    AvgDuration = avg(duration),
    FailureCount = countif(success == false)
  by name
| order by TotalCalls desc
```

### Health Checks

Implement health check endpoints to validate application status:

**Web App**: Typically available at `/health` or default App Service health check
**Function App**: Create a dedicated health check function if needed

---

## Rollback Procedures

If a deployment causes issues, follow these rollback procedures:

### Option 1: Redeploy Previous Version (Recommended)

**Via GitHub Actions**:
1. Navigate to: **Actions** tab
2. Find the last successful workflow run
3. Click **Re-run all jobs**
4. Wait for deployment to complete

**Via Git**:
1. Identify the last working commit: `git log --oneline`
2. Create a revert commit: `git revert <commit-hash>`
3. Push to `main` branch: `git push origin main`
4. Workflows will automatically deploy the reverted code

### Option 2: Deployment Slot Swap (if configured)

If using deployment slots:
1. Navigate to: **Azure Portal** → **App Service** or **Function App**
2. Select: **Deployment** → **Deployment slots**
3. Click: **Swap**
4. Select source and target slots
5. Click: **Swap**

### Option 3: Manual Rollback via Azure Portal

**For Web App**:
1. Navigate to: **App Services** → `red-scus-budget`
2. Select: **Deployment** → **Deployment Center**
3. Find previous successful deployment
4. Click: **Redeploy**

**For Function App**:
1. Navigate to: **Function App** → `red-scus-budgetbackend-demo`
2. Select: **Deployment** → **Deployment Center**
3. Find previous successful deployment
4. Click: **Redeploy**

### Emergency Rollback Checklist

When issues occur:
1. ☐ Identify the issue (check Application Insights, logs)
2. ☐ Assess severity (user-facing, data corruption, security)
3. ☐ Decide: Roll back or hotfix forward
4. ☐ If rolling back:
   - ☐ Trigger rollback using one of the options above
   - ☐ Verify rollback successful (health checks, smoke tests)
   - ☐ Monitor for 15-30 minutes
5. ☐ Document incident and root cause
6. ☐ Create fix and test thoroughly before redeploying

---

## Go-Live Checklist

Use this checklist before deploying to production:

### Pre-Deployment

#### Infrastructure
- [ ] Azure resources created and configured
  - [ ] Resource Group: `red-scus-ghactions-demo-rg`
  - [ ] Web App: `red-scus-budget`
  - [ ] Function App: `red-scus-budgetbackend-demo`
  - [ ] Storage Account (for Function App)
  - [ ] Application Insights enabled

#### Azure AD & Authentication
- [ ] Service Principal created with correct permissions
- [ ] Federated credentials configured with correct subject
  - [ ] Subject: `repo:anotherRedbeard/gh-actions-demo:environment:prod`
- [ ] RBAC permissions verified (Contributor on Resource Group)

#### GitHub Configuration
- [ ] Production environment (`prod`) created
- [ ] GitHub secrets configured:
  - [ ] `AZURE_CLIENT_ID`
  - [ ] `ENTRA_TENANT_ID`
  - [ ] `AZURE_SUBSCRIPTION_ID`
- [ ] (Optional) Environment protection rules configured
- [ ] Workflows validated and tested

#### Application Configuration
- [ ] Web App `API_BASE_URL` configured
- [ ] CORS settings configured on Function App
- [ ] Connection strings and app settings verified
- [ ] SSL/TLS certificates configured
- [ ] Custom domain configured (if applicable)

### Deployment

#### Backend Deployment
- [ ] Deploy Function App first
- [ ] Verify deployment success in Azure Portal
- [ ] Test API endpoints (curl/Postman)
- [ ] Check Application Insights for startup logs
- [ ] Verify no errors in Function App logs

#### Frontend Deployment
- [ ] Deploy Web App second
- [ ] Verify deployment success in Azure Portal
- [ ] Navigate to application URL
- [ ] Verify frontend loads correctly
- [ ] Test navigation and all major features

### Post-Deployment

#### Functional Testing
- [ ] Dashboard loads and displays data
- [ ] Budgets can be viewed and created
- [ ] Transactions can be viewed and created
- [ ] Savings goals display correctly
- [ ] API calls complete successfully (check browser DevTools)

#### Monitoring Setup
- [ ] Application Insights configured and receiving data
- [ ] Alerts configured for critical metrics
- [ ] Log Analytics workspace accessible
- [ ] Dashboard created for key metrics

#### Security & Performance
- [ ] HTTPS enforced on both Web App and Function App
- [ ] No secrets or credentials exposed in logs
- [ ] Response times acceptable (< 2s for most requests)
- [ ] No CORS errors in browser console
- [ ] Authentication working (if implemented)

#### Documentation
- [ ] Deployment documented
- [ ] Runbook created for common issues
- [ ] Contact information for support team updated
- [ ] Rollback procedures documented and tested

### Go-Live
- [ ] All items above completed
- [ ] Stakeholders notified of go-live
- [ ] Monitoring dashboard open and ready
- [ ] Support team available for 1-2 hours post-launch
- [ ] Rollback plan ready if needed

### Post Go-Live (First 24 Hours)
- [ ] Monitor Application Insights for errors
- [ ] Check resource utilization (CPU, memory, requests)
- [ ] Verify no performance degradation
- [ ] Review user feedback (if applicable)
- [ ] Document any issues encountered and resolutions

---

## Troubleshooting

### Common Issues and Solutions

#### Issue: Frontend deployment fails

**Symptoms**: Workflow fails during `Deploy to Azure Web App` step

**Possible Causes**:
- Invalid Azure credentials
- Incorrect app name
- Web App not accessible

**Solutions**:
1. Verify GitHub secrets are correct
2. Check federated credential subject matches: `repo:anotherRedbeard/gh-actions-demo:environment:prod`
3. Ensure Web App exists and is running
4. Check RBAC permissions on service principal

#### Issue: Backend deployment fails

**Symptoms**: Workflow fails during `Deploy to Azure Function App` step

**Possible Causes**:
- Missing `.azurefunctions` directory
- Incorrect app name
- Storage account issues
- Flex Consumption plan not configured

**Solutions**:
1. Verify `.azurefunctions/metadata.json` is created in build step
2. Check `include-hidden-files: true` in artifact upload
3. Ensure Function App exists with Flex Consumption plan
4. Verify storage account is properly linked

#### Issue: Frontend can't connect to backend

**Symptoms**: Frontend loads but no data displays, API errors in browser console

**Possible Causes**:
- `API_BASE_URL` not configured or incorrect
- CORS issues
- Function App not running

**Solutions**:
1. Verify `API_BASE_URL` app setting in Web App: `https://red-scus-budgetbackend-demo.azurewebsites.net/api/`
2. Check CORS settings on Function App allow Web App origin
3. Test Function App endpoint directly: `curl https://red-scus-budgetbackend-demo.azurewebsites.net/api/budgets`
4. Check Function App logs for errors

#### Issue: OIDC authentication fails

**Symptoms**: `Login to Azure` step fails with "OIDC token exchange failed"

**Possible Causes**:
- Federated credential not configured
- Subject mismatch
- Incorrect audience

**Solutions**:
1. Verify federated credential exists: `az ad app federated-credential list --id <APP_ID>`
2. Check subject is exactly: `repo:anotherRedbeard/gh-actions-demo:environment:prod`
3. Verify audience is: `["api://AzureADTokenExchange"]`
4. Ensure workflow uses `environment: prod`

#### Issue: Slow performance

**Symptoms**: Pages load slowly, API requests take > 5 seconds

**Possible Causes**:
- Cold start (Flex Consumption)
- Insufficient resources
- Network latency

**Solutions**:
1. Review Application Insights performance metrics
2. Consider warming up Function App or using Premium plan
3. Check if Web App and Function App are in the same region
4. Optimize database queries (when database is integrated)

### Getting Help

If you encounter issues not covered here:

1. **Check Workflow Logs**: GitHub Actions → Select failed workflow → View logs
2. **Check Azure Logs**: Azure Portal → App Service/Function App → Log stream
3. **Review Application Insights**: Check for exceptions and failed requests
4. **Consult Documentation**:
   - [Azure Web Apps Deploy Action](https://github.com/Azure/webapps-deploy)
   - [Azure Functions Action](https://github.com/Azure/functions-action)
   - [Azure Login Action](https://github.com/Azure/login)
5. **Open GitHub Issue**: Describe the problem with workflow logs and error messages

---

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure Web Apps Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
- [Azure Flex Consumption Plan](https://learn.microsoft.com/en-us/azure/azure-functions/flex-consumption-plan)
- [OIDC with GitHub Actions](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-azure)
- [Application Insights](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview)

---

*Last Updated: February 2026*
