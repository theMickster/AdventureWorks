# AdventureWorks Web API - Claude Code Configuration

Enterprise-grade RESTful API built with .NET 10.0 implementing Clean Architecture with CQRS patterns for the AdventureWorks Cycling database.

## Overview

### What This Project Does
- Exposes RESTful APIs for Sales, HumanResources, and AddressManagement domains using Clean Architecture
- Implements CQRS pattern via MediatR with FluentValidation and AutoMapper
- JWT-based authentication with Microsoft Identity and Azure Key Vault for secrets

### Key Concepts
- **Clean Architecture**: Dependency boundaries (Domain -> Application -> Infrastructure -> API)
- **CQRS**: Commands handle writes (return IDs), Queries handle reads (return DTOs), via MediatR
- **Entity Suffix Convention**: Domain objects use `*Entity` suffix to distinguish from DTOs
- **Repository Pattern**: Interfaces in Application layer, implementations in Infrastructure
- **Feature Folders**: Organized by domain (Sales, HumanResources) not technical layers

---

## Progressive Context Loading

**When adding/implementing features:** Read `guides/adding-features.md` for complete walkthrough with full code examples

**When writing tests:** Read `guides/testing-guide.md` for Handler, Controller, and Validator test templates

**When in doubt:** Read the relevant guide file first - they contain complete implementations you can copy/adapt

---

## Architecture & Patterns

### System Architecture

```
HTTP Request (JWT Token) -> Controller -> [Authorize] -> MediatR.Send()
                                                              |
                    +--------------------+--------------------+
                    |  Command (Write)   |   Query (Read)     |
                    |  Validator         |   Handler          |
                    |  Handler           |   Repository       |
                    |  Repository        |   (.AsNoTracking)  |
                    |  Return ID         |   Return DTO       |
                    +--------------------+--------------------+
                                              |
                              Response (DTO/ID) + X-Correlation-Id
```

### Code Organization

```
apps/api-dotnet/src/
+-- AdventureWorks.API/              # Controllers, middleware, Program.cs
+-- AdventureWorks.Application/      # Features/{Domain}/Commands|Queries|Validators|Profiles
+-- AdventureWorks.Domain/           # Entities (no dependencies)
+-- AdventureWorks.Infrastructure.Persistence/  # Repositories, DbContext
+-- AdventureWorks.Models/           # DTOs (StoreModel, StoreCreateModel, etc.)
+-- AdventureWorks.Common/           # Constants, utilities

tests/AdventureWorks.UnitTests/      # xUnit tests (handlers, controllers, repos)
```

### Key Principles
1. **Dependency Rule**: Dependencies point inward. Infrastructure accessed only via interfaces.
2. **Async All The Way**: Every I/O uses async/await with `CancellationToken`. Never `.Result` or `.Wait()`.

### Core Patterns

#### CQRS via MediatR

**Command Pattern**: `IRequest<int>` with Model, ModifiedDate, RowGuid properties
- Handler: IMapper, IRepository, IValidator -> Validate -> Map -> Persist -> Return ID

**Query Pattern**: `IRequest<TModel>` with Id or filter properties
- Handler: IMapper, IRepository -> Fetch (AsNoTracking) -> Map -> Return DTO

**Controller Pattern**: Thin, delegates to MediatR
- Attributes: `[ApiController]`, `[Authorize]`, `[ApiVersion("1.0")]`
- Commands: `CreatedAtRoute()` | Queries: `Ok(model)`

#### FluentValidation Pattern

Base validator with inheritance for Create/Update variants:
```csharp
RuleFor(x => x.Name).NotEmpty().WithErrorCode("Rule-01").WithMessage(MessageStoreNameEmpty);
```
Use error codes, static message properties, `ValidateAndThrowAsync()` in handlers.

#### AutoMapper Profile Pattern

One profile per mapping direction:
```csharp
CreateMap<Source, Dest>().ForMember(x => x.Prop, o => o.Ignore());
```
Ignore audit fields (ModifiedDate, Rowguid, Id). Use `ForPath()` for nested mappings.

#### Exception Handling Middleware

ValidationException -> 400 with error details. All responses include `X-Correlation-Id`.
See: `AdventureWorks.API/libs/Middleware/ExceptionHandlerMiddleware.cs`

---

## Development Guide

### Adding New Feature

See **[guides/adding-features.md](guides/adding-features.md)** for complete code examples.

**Quick Checklist:**
1. Entity (`Domain/Entities/{Domain}/`)
2. DTOs (`Models/Features/{Domain}/`)
3. Repository Interface (`Application/PersistenceContracts/`)
4. Repository Implementation (`Infrastructure.Persistence/Repositories/`)
5. Validator (`Application/Features/{Domain}/Validators/`)
6. AutoMapper Profiles (`Application/Features/{Domain}/Profiles/`)
7. Command + Handler (`Application/Features/{Domain}/Commands/`)
8. Query + Handler (`Application/Features/{Domain}/Queries/`)
9. Controller (`API/Controllers/v1/{Feature}/`)
10. Unit Tests (`UnitTests/Application/Features/`)

### Common Patterns

- **Primary Constructors (C# 12)**: DI with field assignment and null checks
- **Null Validation**: `ArgumentNullException.ThrowIfNull(request)` at method entry
- **Sealed Classes**: All handlers must be `sealed`
- **CancellationToken**: Required on all async methods

---

## Data Models

**Entity** (Domain layer): `*Entity` suffix, includes navigation properties, audit fields (ModifiedDate, Rowguid)

**Model** (DTOs): `*Model`, `*CreateModel`, `*UpdateModel` - expose only API-relevant properties

**Validation**: Error codes (`Rule-01`), static message properties, `ValidateAndThrowAsync()` in handlers

---

## Security & Configuration

### Security Rules (MANDATORY)

1. **JWT Required**: Write endpoints (POST/PUT/DELETE) MUST have `[Authorize]`
2. **Never Expose Entities**: Return DTOs only
3. **Validate All Input**: Every command needs FluentValidation
4. **No Secrets in Code**: Use Key Vault (Prod) or User Secrets (Dev)
5. **CorrelationId**: Middleware handles automatically

### Security Functions

| Function | Purpose |
|----------|---------|
| `[Authorize]` | JWT Bearer authentication on write operations |
| `ExceptionHandlerMiddleware` | Sanitizes exceptions, adds correlation IDs |
| `ArgumentNullException.ThrowIfNull()` | Fast-fail null checks |
| `ValidateAndThrowAsync()` | FluentValidation in command handlers |

### Environment Configuration

| Variable | Required | Description |
|----------|----------|-------------|
| `EntityFrameworkCoreSettings:CurrentConnectionStringName` | Yes | Database connection name |
| `EntityFrameworkCoreSettings:CommandTimeout` | Yes | EF query timeout (seconds) |
| `KeyVault:VaultUri` | Yes (Prod) | Azure Key Vault URL |

**Config Files:** `appsettings.json`, `appsettings.{Environment}.json`, User Secrets

### Authentication

- JWT Bearer via Microsoft Identity Web (`.AddMicrosoftIdentityWebApi()`)
- `[Authorize]` attribute on controllers
- CORS: Configure restrictively for Production

---

## Testing

See **[guides/testing-guide.md](guides/testing-guide.md)** for complete test templates.

```bash
dotnet test                                    # Run all
dotnet test --collect:"XPlat Code Coverage"   # With coverage
dotnet test --filter "FullyQualifiedName~CreateStore"  # Filtered
```

**Environment:** Moq, FluentAssertions, xUnit, EF Core In-Memory, `UnitTestBase` for fixtures

---

## Code Style & Standards

### Naming Conventions
- `PascalCase`: Classes, methods, properties | `camelCase`: parameters, locals | `_camelCase`: private fields
- Suffixes: `*Entity`, `*Model`, `*Command`, `*Query`, `*Handler`, `*Validator`
- Interfaces: `I*` prefix

### Formatting
- EditorConfig in solution root | 4 spaces, CRLF | File-scoped namespaces
- Imports: System first, then third-party, then project (alphabetical)

### Comments
- XML docs for public APIs | Inline only when non-obvious | No TODOs (use issues)

---

## Anti-Patterns

### DO
- async/await throughout (never `.Result`/`.Wait()`)
- Return DTOs from controllers
- Use repositories (not DbContext) in handlers
- `.AsNoTracking()` for queries
- `.Include()` or projection for related data
- `ArgumentNullException.ThrowIfNull()` at method entry
- `sealed` classes for handlers
- `CancellationToken` on all async methods
- `[Authorize]` on write endpoints
- Feature folder organization

### DON'T
- Skip input validation
- Expose `*Entity` from controllers
- Inject `DbContext` into Application layer
- Swallow exceptions silently
- Track entities for read queries
- Organize by technical layer
- Reference Infrastructure from Application

---

## Deployment

### Building
```bash
cd apps/api-dotnet/src/AdventureWorks.API
dotnet restore && dotnet build --configuration Release
dotnet publish --configuration Release --output ./publish
```

### Versioning
Semantic versioning: `MAJOR.MINOR.PATCH`
API Version: `v1.0` via `Asp.Versioning` package

### Publishing
```bash
# Azure App Service
az webapp deploy --resource-group <rg> --name <app> --src-path ./publish --type zip

# Docker
docker build -t adventureworks-api:latest . && docker run -p 8080:80 adventureworks-api
```

**Database Migrations:** DbUp (`database/dbup/AdventureWorks.DbUp`)

---

## Troubleshooting

### Common Issues
- **VS Debugger breaks on ValidationException**: Import `.vs/ExceptionSettings.xml` in Exception Settings
- **N+1 Queries**: Use `.Include()` or `.Select(x => new Model {...})`
- **Connection String Not Found**: `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."`
- **JWT 401 Errors**: Verify `AzureAd` config (Instance, TenantId, ClientId)
- **AutoMapper Missing Map**: Add profile in `Features/{Domain}/Profiles/`

### Debug Tips
- Verbose logging: `Logging:LogLevel:Default` = `Debug`
- EF queries: `Microsoft.EntityFrameworkCore.Database.Command` = `Information`
- Swagger: `/swagger` | Health: `/health`

---

## References

- [Adding Features Guide](guides/adding-features.md) | [Testing Guide](guides/testing-guide.md)
- [.NET 10.0](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10) | [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/) | [EF Core](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR](https://github.com/jbogard/MediatR/wiki) | [FluentValidation](https://docs.fluentvalidation.net/) | [AutoMapper](https://docs.automapper.org/)

---

**Version**: .NET 10.0 | **Architecture**: Clean Architecture with CQRS
