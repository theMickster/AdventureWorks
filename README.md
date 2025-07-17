<p align="center"><img width=128 height=128 src="https://github.com/theMickster/AdventureWorks/blob/main/_media/AdventureWorksIconBlue01.png"></p>

# AdventureWorks

Adventure Works is a modern enterprise application built with **.NET 10**, **Angular 21**, **Entity Framework Core**, and **Tailwind CSS + DaisyUI**. The architecture follows **Clean Architecture** with **CQRS** patterns, powered by the classic Adventure Works Cycling SQL database from Microsoft.

## Tech Stack

| Layer             | Technology                                       |
| ----------------- | ------------------------------------------------ |
| **Backend API**   | .NET 10 (Clean Architecture + CQRS + MediatR)    |
| **Frontend**      | Angular 21 + Nx 22 monorepo (Signals, zoneless)  |
| **Design System** | Alpine Circuit v2 (Tailwind CSS v4 + DaisyUI v5) |
| **Database**      | SQL Server + Entity Framework Core               |
| **Icons**         | Font Awesome Free 7                              |
| **Testing**       | xUnit, Vitest, Playwright                        |
| **Tooling**       | VS Code, Visual Studio 2026, Azure DevOps        |

## Getting Started

### Prerequisites

- [Visual Studio 2026](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with the Angular Language Service extension
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) (for the Angular workspace)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (local or Docker)
- [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

### Quick Start

```bash
# Backend API
cd apps/api-dotnet
dotnet restore && dotnet run

# Frontend (separate terminal)
cd apps/angular-web
npm install && npx nx serve adventureworks-web
```

## Project Structure

```
AdventureWorks/
├── apps/
│   ├── api-dotnet/          # .NET 10 REST API (Clean Architecture)
│   ├── angular-web/         # Angular 21 SPA (Nx monorepo)
│   └── microservices/       # Future microservices
├── database/
│   ├── dbup/                # Database migrations
│   └── scripts/             # SQL scripts
└── docs/                    # Shared documentation
```

## Database Enhancements

Please visit and read the following ReadMe to understand the changes to the default AdventureWorks database.

- [Enhancement ReadMe](/docs/README.md)

## License

This project is licensed under the MIT License.
