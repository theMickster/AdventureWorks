# AdventureWorks Aspire Dashboard

Single-command local orchestration of all AdventureWorks services — API, Angular, database migrations, and SQL Server health — via the .NET Aspire dashboard.

## Prerequisites

- .NET SDK 10 (already required by this repo)
- Node.js / npm (already required for Angular)
- OrbStack running with a SQL Server 2025 container
- AppHost user secret configured (see below)

## One-Time Setup

Set the database connection string in the AppHost's user secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "CONNECTION_STRING_GOES_HERE" --project tools/aspire/AdventureWorks.AppHost
```

> The AppHost holds its own copy of `DefaultConnection` independently of the API's user secrets, allowing the Aspire-injected connection string and the API's own secrets to coexist without conflict.

## Launch

**CLI:**

```bash
dotnet run --project tools/aspire/AdventureWorks.AppHost
```

**VS Code:** Run & Debug → **AdventureWorks Aspire**

The dashboard URL is printed to the terminal on startup. Open it in a browser.

## Dashboard Resources

| Resource      | Initial state       | Notes                                                      |
| ------------- | ------------------- | ---------------------------------------------------------- |
| `tosk-mssql`  | Running / Unhealthy | TCP health check against the OrbStack container on :1433   |
| `api`         | Running             | https://localhost:44369                                    |
| `angular-web` | Running             | http://localhost:4200                                      |
| `dbup`        | Not started         | Click ▶ **Start** to run migrations; **Restart** to re-run |

## Configuration

If your SQL Server container has a different name than `tosk-mssql`, override it in `AdventureWorks.AppHost/appsettings.json` or via environment variable:

```json
{
  "SqlServer": {
    "ContainerName": "your-container-name"
  }
}
```

## Limitations

- **DbUp has its own connection string.** It reads `ConnectionStrings:AdventureWorks` from its own user secrets under `database/dbup/AdventureWorks.DbUp` — not from the AppHost. Set that secret separately before running migrations.
- **API telemetry only flows to the Aspire dashboard when launched via the AppHost.** Running the API standalone routes telemetry to Application Insights only; the Aspire Traces and Metrics tabs will be empty.
- **Do not change `isProxied` for the Angular resource.** Routing the Angular dev server through Aspire's reverse proxy breaks HMR WebSocket connections.
