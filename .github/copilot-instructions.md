# AdventureWorks Copilot Instructions

Purpose: Guide AI code contributions for this repo. Optimize for correctness, security, and consistency with existing architecture. Keep diffs minimal.

## Repository Structure

This monorepo contains:
- `apps/api-dotnet/` — .NET 10.0 REST API (primary application)
- `database/dbup/` — DbUp SQL migration console app
- `database/sql-change-automation/` — SQL Change Automation project
- `tools/console-apps/` — Utility console applications

## Build & Test Commands

All commands run from `apps/api-dotnet/`:

```bash
# Build
dotnet build AdventureWorks.sln

# Run all tests
dotnet test AdventureWorks.sln

# Run a single test by name
dotnet test --filter "FullyQualifiedName~CreateStoreCommandHandler"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run the API
cd src/AdventureWorks.API && dotnet run

# Run DbUp migrations
cd ../../database/dbup && dotnet run --project AdventureWorks.DbUp.Console
```

## Solution Overview
Projects in `apps/api-dotnet/src/`:
- **Domain**: entities and rules (POCOs, persistence-agnostic).
- **Application**: CQRS (MediatR) requests/handlers, validators, mappings, repository/spec abstractions.
- **Infrastructure.Persistence**: EF Core DbContext, configurations, repository implementations.
- **API**: ASP.NET Core host, controllers, DI, middleware, versioning.
- **Models**: DTOs / API contracts only (no EF entities).
- **Common**: logging/constants/settings/attributes/utilities.
- **Tests**: `tests/AdventureWorks.UnitTests/` + `tests/AdventureWorks.Test.Common/`

Domain areas: `Sales`, `HumanResources`, `AddressManagement`, `Person`.

## Core Patterns
- CQRS: `IRequest<T>` + sealed `IRequestHandler<TReq,TRes>`; handlers thin and orchestrate repositories/services.
- Repository: Prefer `IReadOnlyAsyncRepository<T>`/`IAsyncRepository<T>` (or equivalents) + feature-specific repos (e.g., `IStoreRepository`).
- Specification: Encapsulate filters/sorts/includes; avoid ad‑hoc LINQ in handlers.

## Conventions
- Entities use `*Entity` suffix; `DbSet` properties are plural.
- DTOs in Models end with `Model` (`StoreModel`, `StoreCreateModel`, `StoreSearchResultModel`). Use records for immutable DTOs; classes for entities.
- Commands: `Create|Update|Delete` + Target (e.g., `CreateStoreCommand`). Queries: `Read` + Target/Scope (e.g., `ReadStoreListQuery`).
- Handlers named `<RequestName>Handler` and sealed. Repositories sealed.
- Use primary constructors, required members, file‑scoped namespaces, and collection expressions when clearer.
- Null/arg guards early with `ArgumentNullException.ThrowIfNull` and basic range checks (e.g., ids > 0).
- Async-only; never block with `.Result`/`.Wait()`.

## Feature Structure
- Application: `Features/{Area}/{Commands|Queries|Validators|Profiles}`.
- API: `Controllers/v{version}/{Area}/{Read|Write}*Controller.cs`.
- Infrastructure: `Persistence/{DbContexts|Configurations|Repositories}`.

## API Design
- Route template: `/api/v{version}/{resource}` and `/api/v{version}/{resource}/{id}`.
- Versioning: keep ApiVersioning settings consistent (default v1.0, URL segment + query/header/media-type readers).
- Controllers are thin; delegate to `_mediator.Send(...)`. Use `[Authorize]` unless an endpoint is explicitly public.
- Return proper results: 400 for invalid input, 404 for missing, 200/201 for success. Include XML doc summaries for Swagger.

## Validation & Mapping
- Validators via FluentValidation; one validator per request or per input model (`<Name>Validator`). Fail fast with meaningful messages.
- AutoMapper for translation between entities and models; maintain profiles per area. Avoid hand-mapping unless trivial.

## Persistence & Querying
- DbContext lives in Infrastructure. Application depends only on abstractions.
- Reads default to `AsNoTracking()`; enable tracking only for mutations.
- Shape queries server-side; project to DTOs when feasible to avoid loading large graphs.
- Prevent N+1 via includes or batched lookups (pattern like `CraftStoreModelsAsync`).
- Use CancellationToken for repository methods and handler calls.

## Dependency Injection
- Prefer constructor injection. No service locator.
- Use service lifetime attributes for reflection-based registration (`[ServiceLifetimeScoped]`, etc.).
- Scoped by default for repositories/services; singleton only for stateless thread-safe components.

## Error Handling & Logging
- Centralize exception handling in middleware; propagate `ValidationException` and config errors (`ConfigurationException`) as appropriate.
- Log with `ILogger<T>` using structured messages: `logger.LogInformation("Store {StoreId} created", id);`.
- Log warnings for expected domain issues; errors for unexpected exceptions. Never log secrets/PII.

## Security & Secrets
- JWT Bearer by default for protected endpoints; apply `[Authorize]` thoughtfully.
- Do not expose EF entities or internal identifiers unintentionally.
- No secrets in code. Use configuration providers (Key Vault or environment). Treat connection strings/tokens as sensitive.

## Tech Stack (summary)
- .NET 10.0, ASP.NET Core, EF Core, MediatR, FluentValidation, AutoMapper, Swagger/Swashbuckle, JWT Bearer (Microsoft Identity Web), Application Insights.
- Follow versions pinned in project files; avoid hardcoding versions in docs/samples.

## Performance Guidance
- Favor pagination and server-side filtering/sorting.
- Use indexes and EF configurations where necessary (migrations when requested).
- Avoid synchronous over async paths, excessive allocations, and unnecessary materialization.

## Testing Infrastructure
- **UnitTestBase**: base class for handler/service tests (provides `AutoFixture`, `Moq` mocks).
- **PersistenceUnitTestBase**: base class for EF Core repo tests (EF In-Memory provider).
- **Fixtures**: `SalesDomainFixtures`, `HumanResourcesDomainFixtures`, `LookupFixtures` for seeded test data.
- **Assertions**: `FluentAssertions` for readable assertions; `MockLoggerExtensions` for verifying log calls.
- Test folder mirrors src: `UnitTests/Application/Features/{Domain}/`, `UnitTests/Persistence/Repositories/`.

## Database Migrations (DbUp)
See `.github/instructions/dbup.instructions.md` for full DbUp conventions. Key rules:
- Scripts are forward-only, immutable after execution, tracked in `SchemaVersions` table.
- Filename format: `YYYYMMDD_HHmmss_BriefDescription.sql` (UTC timestamp, lexical ordering).
- All scripts must be idempotent (`IF NOT EXISTS` / `IF COL_LENGTH(...) IS NULL` guards).
- Set Build Action to `EmbeddedResource` in the `.csproj`.
- Schema-qualify all objects: `Person.PersonType`, `dbo.MyTable`.

## Canonical Endpoint Shapes (examples)
- POST `/api/v1/stores` → create store (auth required) returns 201 + id/location.
- GET  `/api/v1/stores/{id:int}` → read store (404 if missing).
- GET  `/api/v1/personTypes/{id:int}` → read person type.
- GET  `/api/v1/personTypes` → list person types with 404 on empty where consistent with existing behavior.

## Copilot Output Rules
- Keep changes small and localized; match existing style and nullability.
- When adding a feature, include: Request, Handler, Validator, Mapping profile updates, Controller endpoint, tests.
- Use specifications and repositories instead of direct DbContext in handlers/controllers.
- Add cancellation tokens and `AsNoTracking()` where applicable.
- Respect constants in `Common.Constants` for configuration keys.
- Avoid introducing new frameworks/patterns unless requested.

## Git Conventions
- Branch: `main` (prod), `feature/*`, `bugfix/*`
- Commit format: `<type>(<scope>): <subject>` — types: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`; scopes: `api`, `db`, `auth`, `sales`, `hr`, etc.
- Never force-push to `main`; never commit secrets.

## When Unsure
- Prefer established patterns already in the repo to maintain coherence.
- Default to explicit, minimal, and reversible changes.
