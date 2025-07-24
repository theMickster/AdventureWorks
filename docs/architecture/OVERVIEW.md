# Architecture Overview

## What This Is

Enterprise application built on the Microsoft AdventureWorks sample database, demonstrating modern architecture patterns across multiple platforms.

## Components

| Component     | Tech                                           | Location              |
| ------------- | ---------------------------------------------- | --------------------- |
| REST API      | .NET 10.0, Clean Architecture, CQRS (MediatR)  | `apps/api-dotnet/`    |
| Web App       | Angular 21, Nx 22, Tailwind + DaisyUI, Signals | `apps/angular-web/`   |
| Database      | SQL Server, AdventureWorks OLTP schema         | `database/`           |
| Microservices | Azure Functions (planned)                      | `apps/microservices/` |

## API Architecture

Clean Architecture layers: API (controllers) → Application (CQRS handlers) → Domain (entities) → Infrastructure (EF Core).

Domains implemented: **Sales** (Stores, SalesPersons, Territories), **HR** (Employees, Departments, Shifts), **Person** (Addresses, Contacts).

## Angular Architecture

Nx monorepo with library-based module boundaries. Zoneless (no Zone.js). Signal-based services. NgRx SignalStore for domain state (planned). MSAL for Entra ID auth.

```
apps/adventureworks-web/    # Shell app
libs/shared/                # Shared UI, utilities, layout
libs/sales/                 # Sales domain (planned)
libs/hr/                    # HR domain (planned)
```

## Auth

Microsoft Entra ID. MSAL redirect flow on the frontend, JWT bearer validation on the API.

## Observability

Azure Application Insights with correlation IDs on all HTTP requests.
