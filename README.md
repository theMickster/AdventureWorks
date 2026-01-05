<p align="center"><img width=128 height=128 src="https://github.com/theMickster/AdventureWorks/blob/main/_media/AdventureWorksIconBlue01.png"></p>

# AdventureWorks

Adventure Works is a modern enterprise application built with **.NET 10**, **Angular 21**, **Entity Framework Core**, and **Tailwind CSS + DaisyUI**. The architecture follows **Clean Architecture** with **CQRS** patterns, powered by the classic Adventure Works Cycling SQL database from Microsoft.

## Tech Stack

| Layer             | Technology                                                                         |
| ----------------- | ---------------------------------------------------------------------------------- |
| **Backend API**   | .NET 10 (Clean Architecture + CQRS + MediatR)                                      |
| **Functions**     | .NET 10 isolated worker Azure Functions (Durable Functions, Service Bus-triggered) |
| **Frontend**      | Angular 21 + Nx 22 monorepo (Signals, zoneless)                                    |
| **Design System** | Alpine Circuit v2 (Tailwind CSS v4 + DaisyUI v5)                                   |
| **Database**      | SQL Server + Entity Framework Core                                                 |
| **Messaging**     | Azure Service Bus (Standard, topics + subscriptions)                               |
| **Icons**         | Font Awesome Free 7                                                                |
| **Testing**       | xUnit, Vitest, Playwright                                                          |
| **Tooling**       | VS Code, Visual Studio 2026, Azure DevOps                                          |

## Getting Started

### Prerequisites

- [Visual Studio 2026](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with the Angular Language Service extension
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 24+](https://nodejs.org/) (for the Angular workspace)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (local or Docker)
- [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

### Quick Start (canonical local stack)

```bash
dotnet run --project tools/aspire/AdventureWorks.AppHost
```

The Aspire dashboard starts the API, Angular app, Functions, Azurite, Service Bus emulator, and the Sales Order Saga test harness. Configure `ConnectionStrings:DefaultConnection` in the AppHost user secrets first; see [`tools/aspire/README.md`](tools/aspire/README.md) for setup and dashboard smoke-test commands.

### Local Development Dashboard (Aspire)

For the containerized API/web-only workflow, see [`docker/README.md`](docker/README.md). Aspire remains the unified local-development workflow.

## Project Structure

```bash
AdventureWorks/
├── apps/
│   ├── angular-web/             # Angular 21 SPA (Nx monorepo)
│   ├── api-dotnet/              # .NET 10 REST API (Clean Architecture)
│   └── functions-dotnet/        # .NET 10 isolated Azure Functions
│       ├── src/                 # Sales Order Saga and its shared wire contracts
│       └── tests/               # Unit, orchestration, harness, and harness test projects
├── database/
│   ├── dbup/                    # DbUp migration runner and SQL scripts
│   └── sql-change-automation/   # SQL Server Database Project (schema migrations)
├── docs/                        # Shared documentation
├── docker/                      # Containerized API/web-only Compose workflow
├── infra/                       # Bicep IaC templates (Azure App Service, SQL, Key Vault)
├── pipelines/                   # Azure DevOps CI/CD pipeline templates
└── tools/
    ├── aspire/                  # .NET Aspire local dev dashboard (one-command stack launch)
    └── console-apps/            # CLI utilities
```

## Database Enhancements

Please visit and read the following ReadMe to understand the changes to the default AdventureWorks database.

- [Enhancement ReadMe](/database/dbup/AdventureWorks.DbUp/README.md)

## License

This project is licensed under the MIT License.
