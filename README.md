# Weather App Demo

A demo application showcasing GitHub Actions with a .NET MVC frontend and Web API backend, designed for deployment to Azure.

## Project Structure

```
├── .github/
│   ├── workflows/
│   │   ├── build.yml          # CI workflow for building and testing
│   │   └── deploy.yml         # CD workflow for Azure deployment
│   └── copilot-instructions.md
├── src/
│   ├── WeatherApp.Web/        # ASP.NET Core MVC Frontend
│   └── WeatherApp.Api/        # ASP.NET Core Web API Backend
├── WeatherApp.sln             # Solution file
└── README.md
```

## Features

- **Weather Lookup**: Search weather forecasts by US zip code
- **5-Day Forecast**: View temperature and conditions for the next 5 days
- **Modern UI**: Clean, responsive design using Bootstrap
- **RESTful API**: Backend API with Swagger documentation

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (Optional) [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) for deployment

### Running Locally

1. Clone the repository:
   ```bash
   git clone https://github.com/anotherRedbeard/gh-actions-demo.git
   cd gh-actions-demo
   ```

2. Build the solution:
   ```bash
   dotnet build
   ```

3. Run the API (Terminal 1):
   ```bash
   cd src/WeatherApp.Api
   dotnet run
   ```
   The API will be available at `https://localhost:7001` with Swagger at `/swagger`

4. Run the Web app (Terminal 2):
   ```bash
   cd src/WeatherApp.Web
   dotnet run
   ```
   The web app will be available at `https://localhost:5001`

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/weatherforecast` | Get default 5-day forecast |
| GET | `/api/weatherforecast/{zipCode}` | Get forecast for specific zip code |

## GitHub Actions Workflows

### Build (`build.yml`)
- Triggers on push to `main`/`develop` and pull requests to `main`
- Builds both projects
- Runs tests
- Creates deployment artifacts

### Deploy (`deploy.yml`)
- Triggers on push to `main` branch
- Deploys API and Web app to Azure App Service
- Requires Azure publish profiles in GitHub Secrets:
  - `AZURE_API_PUBLISH_PROFILE`
  - `AZURE_WEB_PUBLISH_PROFILE`

## Deployment to Azure

1. Create two Azure App Services (one for API, one for Web)
2. Download the publish profiles from Azure Portal
3. Add them as GitHub repository secrets
4. Update the app names in `.github/workflows/deploy.yml`
5. Push to main branch to trigger deployment

## License

MIT
