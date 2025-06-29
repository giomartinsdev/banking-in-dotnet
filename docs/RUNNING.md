# Running the Banking.Core Project

This document explains how to run the Banking.Core project in Visual Studio Code.

## Prerequisites

Before running this project, ensure you have .NET Aspire properly set up in Visual Studio Code. Follow the comprehensive setup guide:

📚 **Setup Guide**: [Running .NET Aspire in Visual Studio Code](https://github.com/giomartinsdev/run-dotnet-aspire-in-visual-studio-code)

## Project Structure

The Banking.Core solution contains two main runnable projects:

- **Banking.Core.API** - The main API service
- **Banking.Core.AppHost** - The Aspire orchestration host

## Running the Project

### Option 1: Using the Debug Configuration (Recommended)

1. Open the project in Visual Studio Code
2. Press `F5` or go to **Run and Debug** (Ctrl+Shift+D)
3. Select "Launch Project" from the dropdown
4. Choose your project type:
   - **API** - Runs the Banking.Core.API project directly
   - **AppHost** - Runs the Banking.Core.AppHost (Aspire orchestration)

### Option 2: Using VS Code Tasks

The project includes several predefined tasks accessible via `Ctrl+Shift+P` → "Tasks: Run Task":

#### For Development (with hot reload):
- **watch-api** - Runs the API with file watching enabled
- **watch-apphost** - Runs the AppHost with file watching enabled

#### For Standard Execution:
- **run-api** - Runs the API project once
- **run-apphost** - Runs the AppHost project once

#### For Building:
- **build-solution** - Builds the entire solution
- **restore** - Restores NuGet packages
- **clean** - Cleans build artifacts

### Option 3: Using Terminal Commands

Open a terminal in the project root and run:

```powershell
# Run the API directly
dotnet run --project src/Banking.Core.API

# Run the AppHost (Aspire orchestration)
dotnet run --project src/Banking.Core.AppHost

# Build the solution
dotnet build

# Run tests
dotnet test
```

## Recommended Workflow

1. **For Development**: Use the **AppHost** configuration to get the full Aspire experience with dashboard and orchestration
2. **For API Testing**: Use the **API** configuration if you only need to test the API endpoints
3. **For Continuous Development**: Use the watch tasks (`watch-apphost` or `watch-api`) for automatic restarts on file changes

## Aspire Dashboard

When running the AppHost, the Aspire dashboard will be available at:
- **Dashboard URL**: http://localhost:15888 (or as configured)

The dashboard provides:
- Service monitoring
- Distributed tracing
- Metrics visualization
- Log aggregation

## Environment Variables

The project is configured with the following development environment variables:

- `ASPNETCORE_ENVIRONMENT`: Development
- `ASPNETCORE_URLS`: https://localhost:7001;http://localhost:5001
- `ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL`: http://localhost:18889
- `ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL`: http://localhost:18890

## Troubleshooting

If you encounter issues:

1. **Check Prerequisites**: Ensure you've followed the [setup guide](https://github.com/giomartinsdev/run-dotnet-aspire-in-visual-studio-code)
2. **Restore Packages**: Run the "restore" task or `dotnet restore`
3. **Clean and Rebuild**: Run the "clean" task followed by "build-solution"
4. **Check Ports**: Ensure the configured ports (7001, 5001, 18889, 18890) are available

## Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Setup Guide for VS Code](https://github.com/giomartinsdev/run-dotnet-aspire-in-visual-studio-code)
- [Banking.Core Architecture](./architecture.png)
