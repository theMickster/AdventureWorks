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

| Task                                                               | Guide                                                  |
| ------------------------------------------------------------------ | ------------------------------------------------------ |
| Adding or implementing a feature / endpoint                        | [`guides/adding-features.md`](guides/adding-features.md) |
| Writing tests (handler / controller / validator)                   | [`guides/testing-guide.md`](guides/testing-guide.md)     |
| Editing Postman collections, environments, or scripts — **required reading before any edit under `apps/api-dotnet/postman/`** | [`guides/postman-guide.md`](guides/postman-guide.md)     |
| Configuring the VS Code debugger (FluentValidation / KeyNotFound exception filters) | [`guides/debugging-guide.md`](guides/debugging-guide.md) |

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
- Attributes: `[ApiController]`, `[ApiVersion("1.0")]`, plus explicit auth intent (`[Authorize]` for protected endpoints, `[AllowAnonymous]` only for deliberate public exceptions)
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
Unhandled exception types fall through to a 500 with a sanitized generic message; the original `exception.Message` is intentionally not echoed in the response body and is captured in structured logs only.
Any new expected exception type used for normal API flows must be translated in middleware or handled explicitly in the controller in the same change.
See: `AdventureWorks.API/libs/Middleware/ExceptionHandlerMiddleware.cs`

#### Composite-Key Rewrite Pattern (delete + insert in transaction)

When a junction-table row needs to change a column that is part of its composite primary key, a true UPDATE is impossible. The handler must replace the row: open an EF transaction, delete the existing entity, insert a new entity with the new key values, and commit. The repository owns the transaction so handlers stay storage-agnostic.

Reference implementations: `IBusinessEntityContactEntityRepository.ReplaceContactTypeAsync` (changes a store contact's `ContactTypeId`) and `IBusinessEntityAddressRepository.ReplaceAddressTypeAsync` (changes a store address's `AddressTypeId`). Both follow the same shape — the handler enforces uniqueness against the target composite key before calling the repository, and re-hydrates the row through `GetWithDetailsByCompositeKeyAsync` after the swap.

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

1. **JWT Required**: Write endpoints (POST/PUT/PATCH/DELETE) MUST have `[Authorize]`
2. **Never Expose Entities**: Return DTOs only
3. **Validate All Input**: Every command needs FluentValidation
4. **No Secrets in Code**: Use Key Vault (Prod) or User Secrets (Dev)
5. **CorrelationId**: Middleware handles automatically
6. **Public Endpoint Exceptions**: A semantically read-only public endpoint is allowed only when it is explicitly marked and justified (`[AllowAnonymous]` + short rationale)

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
- Protected endpoints use `[Authorize]`; intentionally public endpoints opt out explicitly with `[AllowAnonymous]`
- CORS: Configure restrictively for Production

---

## Exception and Contract Policy

- Documented HTTP responses must match actual runtime behavior.
- Expected request failures are translated intentionally:
  - missing resource -> `404`
  - invalid input -> `400`
  - auth failure -> framework auth response
- If a controller advertises `404`, confirm the handler/controller/middleware path actually returns `404` and does not fall through to `500`.
- Use one deliberate not-found strategy per flow:
  - return `null` and translate to `NotFound(...)`, or
  - throw a known exception type and map it intentionally
- If you introduce a new expected exception type, update middleware/controller handling in the same change.
- Do not rely on a follow-up read after a command to correct an exception path that already failed.
- Do not expose raw exception text for expected client-facing failures.

## Validation Guardrails

- Validate FK-backed or externally referenced IDs when practical before persistence.
- Do not rely on database constraints or `DbUpdateException` as the primary way to reject bad client input.
- Patch flows must validate the patched model before saving.
- Validators should cover more than required-field checks when the model controls relationships or constrained values.

## Async and Query Guardrails

- `CancellationToken` must be threaded end-to-end: controller -> MediatR -> handler -> repository -> EF async call.
- New repository methods should accept `CancellationToken cancellationToken = default` unless there is a clear reason they cannot.
- Read flags such as include/exclude options must change query shape, not just clear mapped collections after loading.
- Keep reads `AsNoTracking()` unless mutation requires tracking.
- Never use `.Result` or `.Wait()`, even after `Task.WhenAll(...)`.
- User input in `EF.Functions.Like(...)` patterns must have `%`, `_`, `[`, `]` escaped via `EscapeLikePattern(input)` before interpolation; unescaped wildcards bypass all filtering and are a DoS risk on public endpoints.

## Auth Intent Guardrails

- Write endpoints are protected by default.
- If an endpoint is intentionally public, make that explicit in code with `[AllowAnonymous]` and add a short rationale.
- If a read-only operation uses `POST`, do not leave its auth intent implicit.

## Minimum Test Coverage for API Changes

For new or changed API flows, cover the relevant combination of:

- success path
- bad request / validation failure
- not-found read/update path
- invalid referenced IDs
- auth intent when an endpoint is protected or intentionally public
- repository/query behavior when include flags change loaded data

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

## AI Done Gate

Before finishing an API task, verify all of the following:

- controller attributes, XML docs, and response behavior agree
- expected exceptions are mapped intentionally in controller or middleware
- validators cover referenced IDs and update/patch edge cases
- new async methods accept and forward `CancellationToken`
- include/filter flags affect data access, not only final mapped output
- tests cover both the main happy path and the important failure paths

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
- Use inline `new[] { "A", "B" }` inside `Must(...)` validator predicates for bounded domain code sets — use `private static readonly HashSet<string?>`

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
- **VS Code breaks on `FluentValidation.ValidationException` / `KeyNotFoundException`**: These are caught by `ExceptionHandlerMiddleware` and translated to 400/404. See [guides/debugging-guide.md](guides/debugging-guide.md) to add them to the User-Unhandled exception ignore list.
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

- [Adding Features Guide](guides/adding-features.md) | [Testing Guide](guides/testing-guide.md) | [Postman Guide](guides/postman-guide.md) | [Debugging Guide](guides/debugging-guide.md)
- [.NET 10.0](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10) | [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/) | [EF Core](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR](https://github.com/jbogard/MediatR/wiki) | [FluentValidation](https://docs.fluentvalidation.net/) | [AutoMapper](https://docs.automapper.org/)

---

**Version**: .NET 10.0 | **Architecture**: Clean Architecture with CQRS
