# Budget Tracker - GitHub Actions Demo

A fullstack personal budget and savings goal tracker built with .NET 9, designed to demonstrate GitHub Actions CI/CD workflows with Azure deployment.

## 🎯 Project Overview

This application showcases:
- **Frontend**: ASP.NET Core MVC with modern, responsive UI
- **Backend**: Azure Functions (Isolated Process Model) 
- **Demo Purpose**: GitHub Actions workflows for CI/CD automation
- **Cloud Platform**: Azure (Web App + Functions)

## ✨ Features

### Current Features (v1.0)
- 📊 **Interactive Dashboard** - Visual overview of budgets, spending, and savings
- 💰 **Budget Management** - Create and track monthly budgets by category
- 📝 **Transaction Tracking** - Record income and expenses
- 🎯 **Savings Goals** - Set and monitor financial goals with progress tracking
- 📈 **Charts & Visualizations** - Beautiful data visualization with Chart.js
- 💾 **In-Memory Data** - Sample data for demo purposes

### Planned Features
- ✅ Database integration (Cosmos DB or Azure SQL)
- ✅ Infrastructure as Code (Bicep templates)
- ✅ Authentication (Azure AD B2C)
- ✅ GitHub Actions workflows (CI/CD, security scanning, etc.)

## 🛠️ Technology Stack

- **.NET 9** - Latest LTS version
- **ASP.NET Core MVC** - Frontend web framework
- **Azure Functions** - Serverless backend API
- **Bootstrap 5** - Responsive UI framework
- **Chart.js** - Data visualization
- **Bootstrap Icons** - Icon library

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- IDE: Visual Studio 2022, VS Code, or Rider

### Running Locally

#### Frontend (MVC Web App)

```bash
cd src/frontend/BudgetTracker.Web
dotnet restore
dotnet build
dotnet run
```

The web app will be available at `https://localhost:5001` (or the port shown in console).

#### Backend (Azure Functions)

```bash
cd src/backend/BudgetTracker.Functions
dotnet restore
dotnet build
func start
```

The API will be available at `http://localhost:7071`.

### Project Structure

```
gh-actions-demo/
├── .github/
│   └── copilot-instructions.md    # Project guidelines
├── src/
│   ├── frontend/
│   │   └── BudgetTracker.Web/     # ASP.NET Core MVC project
│   │       ├── Controllers/       # MVC controllers
│   │       ├── Models/           # Data models
│   │       ├── Views/            # Razor views
│   │       ├── Services/         # Business logic
│   │       └── wwwroot/          # Static assets (CSS, JS)
│   └── backend/
│       └── BudgetTracker.Functions/  # Azure Functions project
│           ├── Functions/        # HTTP-triggered functions
│           ├── Models/           # Data models
│           └── Services/         # Business logic
├── README.md
└── .gitignore
```

## 📱 Screenshots

### Dashboard
The main dashboard provides a comprehensive view of:
- Budget summary cards
- Spending vs. planned budget chart
- Savings goals progress
- Recent transactions list

### Features
- **Budget Overview**: Visual comparison of planned vs. actual spending
- **Savings Goals**: Track multiple goals with progress indicators
- **Transactions**: Complete transaction history with filtering

## 🔗 API Endpoints

### Budgets
- `GET /api/budgets` - Get all budgets
- `GET /api/budgets/{id}` - Get budget by ID
- `POST /api/budgets` - Create new budget

### Transactions
- `GET /api/transactions` - Get all transactions
- `GET /api/transactions/{id}` - Get transaction by ID
- `POST /api/transactions` - Create new transaction

### Savings Goals
- `GET /api/savings-goals` - Get all savings goals
- `GET /api/savings-goals/{id}` - Get savings goal by ID
- `POST /api/savings-goals` - Create new savings goal

## 🎨 Design System

### Color Palette
- **Primary**: #1E3A8A (Financial Blue)
- **Success**: #10B981 (Green)
- **Warning**: #F59E0B (Amber)
- **Danger**: #EF4444 (Red)
- **Info**: #3B82F6 (Blue)

### UI Principles
- Clean, modern design
- Mobile-first responsive layout
- Accessibility (WCAG 2.1 AA)
- Smooth transitions and animations

## 🔄 Upcoming: GitHub Actions Workflows

This project will demonstrate:
1. **CI Workflow** - Build, test, and validate on every PR
2. **CD Workflow** - Deploy to Azure on merge to main
3. **Security Scanning** - CodeQL and dependency checks
4. **Performance Testing** - Load testing with Azure Load Testing
5. **Infrastructure Deployment** - Bicep IaC automation

## 📝 License

This project is for demonstration purposes.

## 🤝 Contributing

This is a demo project, but suggestions and improvements are welcome!

---

Built with ❤️ using .NET 9 and Azure
Demo repo for github actions using .net. Frontend and backend app that will be deployed to Azure using actions.
