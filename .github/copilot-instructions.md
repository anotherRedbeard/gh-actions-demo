# GitHub Copilot Instructions

## Project Overview

This is a demo Weather application with a .NET MVC frontend and a .NET Web API backend. The project is designed to illustrate GitHub Actions workflows for building and deploying to Azure.

## Architecture

- **Frontend (WeatherApp.Web)**: ASP.NET Core MVC application with Bootstrap styling
- **Backend (WeatherApp.Api)**: ASP.NET Core Web API that provides weather data

## Key Guidelines

### Keep It Simple
- This is a demonstration project, so keep implementations straightforward
- Use mock data where appropriate (like weather forecasts)
- Focus on showcasing GitHub Actions capabilities

### Code Style
- Use .NET 8.0 features and best practices
- Follow C# naming conventions (PascalCase for public members)
- Keep controllers thin, business logic in services
- Use dependency injection for all services

### Frontend
- Use Bootstrap for styling - keep it clean and modern
- Razor views should be simple and focused
- Use view models for data transfer between controllers and views

### Backend API
- RESTful API design with proper HTTP methods
- Return appropriate status codes (200, 400, 404, 500)
- Use the /api prefix for all API endpoints
- Enable CORS for frontend communication

### Testing
- Add unit tests for services and controllers
- Integration tests for API endpoints
- Keep tests focused and readable

### GitHub Actions
- Build workflow runs on PRs and pushes
- Deploy workflow deploys to Azure on main branch pushes
- Use environment variables for configuration
- Store secrets in GitHub Secrets

## Common Tasks

### Running Locally
```bash
# Build all projects
dotnet build

# Run the API (port 7001)
cd src/WeatherApp.Api && dotnet run

# Run the Web app (port 5001)
cd src/WeatherApp.Web && dotnet run
```

### Adding New Features
1. Create models in the Models folder
2. Add services in the Services folder (with interfaces)
3. Register services in Program.cs
4. Create controllers with proper route attributes
5. Add views in the appropriate Views folder
