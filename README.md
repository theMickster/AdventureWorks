<p align="center"><img width="128" height="128" src="_media/AdventureWorksIconBlue01.png" alt="AdventureWorks logo"></p>

# AdventureWorks

AdventureWorks is a sample application built around Microsoft's AdventureWorks SQL Server database. This repository contains the web application, APIs, database tooling, infrastructure, and local development tools.

## Tech Stack

| Area                    | Technology                              |
| ----------------------- | --------------------------------------- |
| **Backend**             | .NET 10                                 |
| **Functions**           | .NET 10 Azure Functions                 |
| **Frontend**            | Angular 21 and Nx 22                    |
| **Design system**       | Tailwind CSS 4 and DaisyUI 5            |
| **Database**            | SQL Server, Entity Framework Core, DbUp |
| **Messaging**           | Azure Service Bus                       |
| **Local orchestration** | .NET Aspire                             |
| **Containers**          | Docker Compose                          |
| **Testing**             | xUnit, Vitest, and Playwright           |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 24 or later](https://nodejs.org/) and npm
- A Docker-compatible container runtime
- Access to a SQL Server instance containing the AdventureWorks database
- [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- Optional: [SQL Server Management Studio](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms)

## Quick Start

Run these commands from the repository root:

```bash
cd apps/angular-web
npm install
cd ../..

dotnet user-secrets set "ConnectionStrings:AdventureWorks" "YOUR_CONNECTION_STRING" --project tools/aspire/AdventureWorks.AppHost
dotnet run --project tools/aspire/AdventureWorks.AppHost
```

Continue with the [Aspire local development guide](tools/aspire/README.md) for all other setup and usage instructions.

## Project Structure

```text
AdventureWorks/
├── apps/ # Application workspaces
│   ├── angular-web/ # Angular workspace
│   ├── api-dotnet/ # .NET API
│   │   ├── src/ # API source projects
│   │   └── tests/ # API test projects
│   └── functions-dotnet/ # Azure Functions workspace
│       ├── src/ # Functions source projects
│       └── tests/ # Functions test projects
├── database/ # Database projects and migration tooling
├── docker/ # Docker Compose configuration
├── docs/ # Repository documentation
├── infra/ # Infrastructure as code
├── pipelines/ # CI/CD definitions and templates
└── tools/ # Local development and utility projects
    ├── aspire/ # Aspire AppHost and its documentation
    └── console-apps/ # Command-line utilities
```

## Documentation

- [Aspire local development](tools/aspire/README.md)
- [Docker Compose workflow](docker/README.md)
- [Database setup and migrations](database/dbup/AdventureWorks.DbUp/README.md)

## License

This project is licensed under the [MIT License](LICENSE.md).
