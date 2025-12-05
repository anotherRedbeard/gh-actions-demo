# GitHub Actions Demo - Personal Budget & Savings Tracker
## Copilot Instructions & Project Guidelines

---

## Project Overview

**Application Name**: Personal Budget & Savings Goal Tracker  
**Purpose**: Demonstrate GitHub Actions CI/CD with a fullstack .NET application  
**Architecture**: 
- **Frontend**: ASP.NET Core MVC → Azure Web App
- **Backend**: .NET Azure Functions (Isolated Process) → Azure Functions (Flex Consumption)

---

## Core Requirements

### Technology Stack
- **.NET Version**: Use .NET 9.0 (LTS)
- **Frontend**: ASP.NET Core MVC
- **Backend**: Azure Functions with .NET Isolated Process Model
- **Database**: Azure Cosmos DB or Azure SQL Database
- **Authentication**: Azure AD B2C or Managed Identity
- **Infrastructure**: Bicep for IaC (placed in `infra/` folder)
- **CI/CD**: GitHub Actions workflows

### Design Principles
1. **Keep It Simple**: Demo-focused code, avoid over-engineering
2. **Visually Pleasing UI**: Modern, responsive design with excellent UX
3. **Security First**: No hardcoded credentials, use Managed Identity
4. **Best Practices**: Follow Microsoft and Azure best practices
5. **GitHub Actions Focus**: Demonstrate comprehensive CI/CD workflows

---

## Application Features

### Frontend (MVC Web App)
- **Dashboard**: 
  - Budget overview with visual charts (Chart.js or similar)
  - Current month spending vs. budget
  - Savings goals progress bars
  - Spending by category (pie/donut charts)
  
- **Budget Management**:
  - Create/edit monthly budgets
  - Set category limits (groceries, utilities, entertainment, etc.)
  - Budget vs. actual comparison

- **Transactions**:
  - Add/edit/delete transactions
  - Categorize expenses
  - Date range filtering
  
- **Savings Goals**:
  - Create savings goals with target amounts and dates
  - Track progress with visual indicators
  - Calculate compound interest projections
  - Milestone tracking

- **Reports**:
  - Monthly/yearly spending trends
  - Category breakdowns
  - Savings rate calculations
  - Export to CSV/PDF

### Backend (Azure Functions)
- **HTTP Triggers**:
  - `GET /api/budgets` - Retrieve budgets
  - `POST /api/budgets` - Create/update budgets
  - `GET /api/transactions` - Get transactions with filters
  - `POST /api/transactions` - Add transactions
  - `GET /api/savings-goals` - Retrieve savings goals
  - `POST /api/savings-goals` - Create/update goals
  - `GET /api/analytics/spending-trends` - Calculate trends
  - `GET /api/analytics/category-breakdown` - Category analysis

- **Timer Triggers**:
  - Daily budget alert check (9 AM)
  - Monthly budget reset (1st of month)
  - Weekly savings goal progress notifications

- **Event Grid Triggers** (Optional):
  - Process bulk transaction imports
  - Handle data export requests

---

## Azure Best Practices to Follow

### General Azure Development
- **Authentication**: Always use Managed Identity (never key-based auth)
- **Secrets**: Store in Azure Key Vault, never hardcode
- **Error Handling**: Implement retry logic with exponential backoff
- **Logging**: Use Application Insights for monitoring and telemetry
- **Connection Pooling**: Use for database connections
- **Encryption**: Enable for all data at rest and in transit

### Azure Functions Specific
- **Programming Model**: Use .NET Isolated Process Model (v4)
- **Extension Bundles**: Use version `[4.*, 5.0.0)` in host.json
- **Authentication Level**: Default to `function` level (not anonymous)
- **Project Structure** (.NET Isolated):
  ```
  backend/
  ├── host.json
  ├── local.settings.json
  ├── Program.cs
  ├── Functions/
  │   ├── BudgetFunctions.cs
  │   ├── TransactionFunctions.cs
  │   └── SavingsGoalFunctions.cs
  └── Models/
      ├── Budget.cs
      ├── Transaction.cs
      └── SavingsGoal.cs
  ```

### Deployment Best Practices
- **Hosting Plan**: Use Flex Consumption (FC1) - never Y1 Dynamic
- **OS**: Linux for better performance and cost
- **Deployment Storage**: Always configure in functionAppConfig
- **Scaling**: Deploy one function per Function App for optimal scaling
- **Monitoring**: Enable Application Insights with exception tracking
- **Networking**: Consider VNET integration for production scenarios
- **IaC**: Use Bicep files in `infra/` folder
- **Reference Samples**:
  - https://github.com/Azure-Samples/functions-quickstart-javascript-azd/tree/main/infra
  - https://github.com/Azure-Samples/functions-quickstart-dotnet-azd-eventgrid-blob/tree/main/infra

### Security & RBAC
- **Management Plane**: Use built-in Azure roles, scope appropriately
- **Data Plane**: Implement fine-grained access control
- **Key Vault**: Disable purge protection is NOT allowed
- **Storage**: Disable anonymous pull access on Container Registry
- **API Versions**: Always use latest stable API versions

---

## UI/UX Guidelines

### Design System
- **Framework**: Bootstrap 5 or Tailwind CSS
- **Color Palette**: 
  - Primary: Financial blue (#1E3A8A)
  - Success: Green for goals achieved (#10B981)
  - Warning: Amber for budget alerts (#F59E0B)
  - Danger: Red for overspending (#EF4444)
  
- **Typography**: 
  - Headers: Bold, clear hierarchy
  - Body: Readable, appropriate line spacing
  
- **Components**:
  - Cards for budget/goal summaries
  - Progress bars for goals and spending
  - Charts for data visualization (Chart.js, ApexCharts)
  - Modals for forms
  - Toast notifications for feedback

### User Experience
- **Responsive**: Mobile-first design
- **Accessibility**: WCAG 2.1 AA compliance
- **Loading States**: Show skeleton screens or spinners
- **Error Handling**: User-friendly error messages
- **Feedback**: Immediate visual feedback for actions
- **Navigation**: Intuitive, clear menu structure

---

## GitHub Actions Workflows to Demonstrate

### 1. CI Workflow (`.github/workflows/ci.yml`)
**Triggers**: Push to feature branches, Pull Requests
**Jobs**:
- **Frontend Build & Test**:
  - Checkout code
  - Setup .NET 9
  - Restore dependencies
  - Build MVC project
  - Run unit tests with coverage
  - Run linting/code analysis
  
- **Backend Build & Test**:
  - Checkout code
  - Setup .NET 9 
  - Build Functions project
  - Run unit tests
  - Run security scanning (e.g., OWASP Dependency Check)

- **Infrastructure Validation**:
  - Bicep file linting
  - `az deployment group what-if` for changes preview

### 2. CD Workflow (`.github/workflows/deploy.yml`)
**Triggers**: Push to `main` branch, manual dispatch
**Environments**: dev, staging, prod
**Jobs**:
- **Deploy Infrastructure** (Bicep):
  - Azure login via OIDC or Service Principal
  - `azd provision --preview` for validation
  - Deploy Bicep templates
  - Store outputs as secrets

- **Deploy Frontend** (MVC to Web App):
  - Build and publish MVC app
  - Deploy to Azure Web App
  - Run smoke tests
  - Health check endpoint validation

- **Deploy Backend** (Functions):
  - Build and publish Functions
  - Deploy to Azure Functions (Flex Consumption)
  - Verify function endpoints
  - Check Application Insights telemetry

### 3. Security Scanning (`.github/workflows/security.yml`)
**Triggers**: Schedule (daily), manual
**Jobs**:
- CodeQL analysis
- Dependency vulnerability scanning
- Secret scanning verification
- SAST (Static Application Security Testing)

### 4. Performance Testing (`.github/workflows/performance.yml`)
**Triggers**: Manual, post-deployment to staging
**Jobs**:
- Load testing with Azure Load Testing
- API response time benchmarks
- Database query performance

### 5. Database Migration (`.github/workflows/db-migration.yml`)
**Triggers**: Manual with environment selection
**Jobs**:
- Run EF Core migrations or SQL scripts
- Backup database before migration
- Validate migration success

---

## Folder Structure

```
gh-actions-demo/
├── .github/
│   ├── workflows/
│   │   ├── ci.yml
│   │   ├── deploy.yml
│   │   ├── security.yml
│   │   └── performance.yml
│   └── copilot-instructions.md (this file)
├── src/
│   ├── frontend/
│   │   ├── BudgetTracker.Web/ (MVC project)
│   │   │   ├── Controllers/
│   │   │   ├── Models/
│   │   │   ├── Views/
│   │   │   ├── wwwroot/
│   │   │   │   ├── css/
│   │   │   │   ├── js/
│   │   │   │   └── lib/
│   │   │   ├── Program.cs
│   │   │   └── appsettings.json
│   │   └── BudgetTracker.Web.Tests/
│   └── backend/
│       ├── BudgetTracker.Functions/
│       │   ├── Functions/
│       │   ├── Models/
│       │   ├── Services/
│       │   ├── host.json
│       │   ├── local.settings.json
│       │   └── Program.cs
│       └── BudgetTracker.Functions.Tests/
├── infra/
│   ├── main.bicep
│   ├── modules/
│   │   ├── web-app.bicep
│   │   ├── function-app.bicep
│   │   ├── database.bicep
│   │   └── monitoring.bicep
│   └── main.parameters.json
├── tests/
│   ├── integration/
│   └── e2e/
├── docs/
│   ├── architecture.md
│   └── deployment-guide.md
├── README.md
├── .gitignore
└── azure.yaml (for azd)
```

---

## Development Commands

### Local Development
```bash
# Frontend (MVC)
cd src/frontend/BudgetTracker.Web
dotnet restore
dotnet build
dotnet run

# Backend (Functions)
cd src/backend/BudgetTracker.Functions
func start
```

### Testing
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Deployment (Manual)
```bash
# Validate infrastructure
azd provision --preview

# Deploy everything
azd up

# Deploy specific service
azd deploy frontend
azd deploy backend
```

---

## Key Reminders for Copilot

1. **Always check best practices** before generating Azure-related code
2. **Use Managed Identity** for all Azure service authentication
3. **Place Bicep files in `infra/`** folder
4. **Use Flex Consumption (FC1)** for Functions, not Y1
5. **Enable Application Insights** for all deployments
6. **Validate deployments** with `--preview` or `what-if` commands
7. **Keep UI modern and responsive** - prioritize visual appeal
8. **Focus on GitHub Actions** - this is the primary demo goal
9. **Simplicity over complexity** - it's a demo, not production
10. **Document workflows** - explain what each action does

---

## GitHub Actions Best Practices

- **Secrets Management**: Use GitHub Secrets for Azure credentials, never commit
- **Reusable Workflows**: Create composite actions for repeated tasks
- **Matrix Builds**: Test across multiple .NET versions if needed
- **Caching**: Cache NuGet packages and npm dependencies
- **Artifacts**: Store build outputs and test results
- **Status Badges**: Add workflow status badges to README
- **Pull Request Comments**: Auto-comment with deployment URLs
- **Rollback Strategy**: Implement automated rollback on failure
- **Environment Protection**: Require approvals for production deployments

---

## Success Criteria

✅ Clean, modern UI with excellent visual design  
✅ Functional budget tracking and savings goals  
✅ All Azure services deployed via Bicep  
✅ Comprehensive GitHub Actions workflows  
✅ Zero hardcoded secrets or credentials  
✅ Application Insights monitoring enabled  
✅ Automated testing in CI pipeline  
✅ Security scanning integrated  
✅ Documentation complete and clear  
✅ Demo-ready and easy to understand  

---

*Last Updated: December 5, 2025*
