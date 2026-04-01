# Production Deployment Setup

Essential setup instructions for deploying the Budget Tracker application to Azure using GitHub Actions.

## Architecture Overview

The application deploys to two Azure services:

- **Frontend**: Azure Web App `<your-web-app-name>` (ASP.NET Core MVC)
- **Backend**: Azure Function App `<your-function-app-name>` (Flex Consumption)
- **Resource Group**: `<your-resource-group>`

The workflows in `.github/workflows/` handle the deployment process automatically when code is pushed to `main`.

---


## 1. Azure AD Setup for OIDC Authentication

### Create Service Principal

Create an Azure AD application and service principal for GitHub Actions:

```bash
SUBSCRIPTION_ID="your-subscription-id"
RESOURCE_GROUP="<your-resource-group>"

az ad sp create-for-rbac \
  --name "gh-actions-budget-demo" \
  --role contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP
```

Save the output - you'll need the `appId` and `tenant` values.

### Configure Federated Credentials

Set up OIDC federated credentials (replaces client secret authentication):

```bash
APPLICATION_ID="<appId from above>"

az ad app federated-credential create \
  --id $APPLICATION_ID \
  --parameters '{
    "name": "github-prod-federated-credential",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:<your-github-org>/<your-repo-name>:environment:prod",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

**Important**: The `subject` must exactly match `repo:<your-github-org>/<your-repo-name>:environment:prod`

---

## 2. GitHub Configuration

### Create Production Environment

In GitHub repository settings:
1. Go to **Settings** → **Environments** → **New environment**
2. Name it `prod`
3. (Optional) Add protection rules

### Add Repository Secrets

Add these secrets in **Settings** → **Secrets and variables** → **Actions**:

| Secret Name | Value | Source |
|------------|-------|--------|
| `AZURE_CLIENT_ID` | Application (Client) ID | `appId` from service principal creation |
| `ENTRA_TENANT_ID` | Azure AD Tenant ID | `tenant` from service principal creation |
| `AZURE_SUBSCRIPTION_ID` | Azure Subscription ID | Run `az account show --query id -o tsv` |

---

## 3. Runtime Configuration

### Web App: Configure API_BASE_URL

The frontend needs to know where the backend API is located:

**Via Azure CLI**:
```bash
az webapp config appsettings set \
  --name <your-web-app-name> \
  --resource-group <your-resource-group> \
  --settings API_BASE_URL="https://<your-function-app-name>.azurewebsites.net/api/"
```

**Via Azure Portal**:
1. Navigate to Web App `<your-web-app-name>`
2. **Settings** → **Configuration** → **Application settings**
3. Add new setting:
   - **Name**: `API_BASE_URL`
   - **Value**: `https://<your-function-app-name>.azurewebsites.net/api/`
4. Click **Save** and restart the app

### Function App: Configure CORS

Allow the frontend to call the backend API:

**Via Azure Portal**:
1. Navigate to Function App `<your-function-app-name>`
2. **API** → **CORS**
3. Add allowed origin: `https://<your-web-app-name>.azurewebsites.net`
4. Click **Save**

---

## 4. Deployment

### Automatic Deployment

Workflows automatically deploy when you push to `main`:

- **Frontend**: Triggers on changes to `src/frontend/**`
- **Backend**: Triggers on changes to `src/backend/**`

### Manual Deployment

To deploy manually:
1. Go to **Actions** tab in GitHub
2. Select workflow: "Build and deploy Frontend Budget App" or "Build and Deploy Backend Function App"
3. Click **Run workflow** → Select `main` branch → **Run workflow**

### Deployment Order

When deploying both components, deploy **backend first**, then frontend.

---

## Verification

After deployment:

1. **Check backend**: Visit `https://<your-function-app-name>.azurewebsites.net/api/budgets`
2. **Check frontend**: Visit `https://<your-web-app-name>.azurewebsites.net`
3. **Verify connectivity**: Ensure data loads on the dashboard (indicates frontend→backend communication works)

---

## Common Issues

**OIDC authentication fails**: Verify the federated credential subject is exactly `repo:<your-github-org>/<your-repo-name>:environment:prod`

**Frontend can't reach backend**: Check that `API_BASE_URL` is configured in Web App settings

**CORS errors**: Ensure Function App CORS includes the Web App URL

**Smoke test fails with HTTP 000 or exit code 6**: DNS resolution failure. Verify the Web App name is correct and the app is provisioned. The smoke test uses `curl` which will fail with exit code 6 if the hostname can't be resolved.

**Smoke test fails with HTTP 503**: The backend Function App may not be reachable from the frontend. Check that:

1. **Public network access** is enabled on the Function App's storage account (required for Flex Consumption apps)
2. **Account key access** is enabled for the Function App deployment storage
3. The Function App has fully started — Flex Consumption apps can take longer on first request

**Frontend returns 500 after deployment**: The `DashboardController` calls the backend API on page load. If the backend is unavailable (503), `BudgetApiClient.EnsureSuccessStatusCode()` throws an unhandled exception. Ensure the backend is deployed and healthy before testing the frontend dashboard.

---

*For more details on the deployment workflows, see `.github/workflows/` directory*

