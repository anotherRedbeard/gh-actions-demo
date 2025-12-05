# Weather App Demo

Demo repo for GitHub Actions using .NET. A frontend and backend application designed for deployment to Azure using GitHub Actions.

## Overview

This is a simple demo application that illustrates:
- ASP.NET Core MVC frontend with Bootstrap styling
- Azure Functions backend API
- GitHub Actions deployment workflow

**Note:** This is a demonstration project with mock weather data to keep things simple and focus on the deployment aspects.

## Architecture

### Frontend (WeatherApp.Frontend)
- **Technology**: ASP.NET Core MVC (.NET 8)
- **Features**:
  - Home page with navigation
  - Weather lookup by US zip code
  - Responsive Bootstrap design

### Backend (WeatherApp.Api)
- **Technology**: Azure Functions (.NET 8 isolated worker)
- **Features**:
  - HTTP-triggered weather API
  - Returns mock weather data for any US zip code

## Getting Started

### Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools (for local development)

### Running Locally

1. **Start the Azure Function backend:**
   ```bash
   cd src/WeatherApp.Api
   func start
   ```

2. **Start the MVC frontend (in a new terminal):**
   ```bash
   cd src/WeatherApp.Frontend
   dotnet run
   ```

3. Open your browser to `https://localhost:5001` (or the port shown in the console)

### Testing

Enter any 5-digit US zip code in the weather form. Some examples:
- `10001` - New York, NY
- `90210` - Beverly Hills, CA
- `60601` - Chicago, IL
- `98101` - Seattle, WA

## Project Structure

```
├── .github/
│   └── copilot-instructions.md    # Copilot guidance
├── src/
│   ├── WeatherApp.Frontend/       # MVC Web Application
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Views/
│   │   └── wwwroot/
│   └── WeatherApp.Api/            # Azure Functions API
│       └── Functions/
└── WeatherApp.sln                 # Solution file
```

## Deployment

This project is designed to be deployed to Azure using GitHub Actions:
- **Frontend**: Azure App Service
- **Backend**: Azure Functions

## License

This project is for demonstration purposes.
