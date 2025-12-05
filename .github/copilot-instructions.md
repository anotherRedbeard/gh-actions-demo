# GitHub Copilot Instructions

## Project Overview

This is a simple demo application to illustrate GitHub Actions deployment with .NET. Keep it simple!

### Architecture

- **Frontend**: ASP.NET Core MVC (.NET 8) web application
- **Backend**: Azure Functions (.NET 8 isolated worker) API

### Key Guidelines

1. **Keep It Simple**: This is a demo app meant to illustrate concepts, not production-ready code
2. **Use .NET MVC**: The frontend uses traditional ASP.NET Core MVC patterns
3. **Use Azure Functions**: The backend is an Azure Function with HTTP triggers
4. **Mock Data**: Weather data is mocked for demonstration purposes - no external API calls

### Frontend Structure

```
src/WeatherApp.Frontend/
├── Controllers/          # MVC controllers
│   ├── HomeController.cs
│   └── WeatherController.cs
├── Models/               # View models and data models
├── Views/                # Razor views
│   ├── Home/
│   ├── Weather/
│   └── Shared/
└── wwwroot/              # Static files
```

### Backend Structure

```
src/WeatherApp.Api/
├── Functions/            # Azure Functions
│   └── WeatherFunction.cs
├── Program.cs            # Function app startup
└── host.json             # Azure Functions host configuration
```

### Development Tips

- Run the Azure Function locally on port 7071
- Run the MVC frontend on a different port (default 5000/5001)
- The frontend calls the backend API at `/api/weather/{zipCode}`
- Weather data is generated based on the zip code for consistent results

### Testing

- Enter any 5-digit US zip code in the weather form
- Common test zip codes: 10001 (NYC), 90210 (Beverly Hills), 60601 (Chicago)

### Deployment

This project is designed to be deployed using GitHub Actions to Azure:
- Frontend → Azure App Service
- Backend → Azure Functions
