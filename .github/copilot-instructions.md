# AdventureWorks Copilot Instructions

Purpose: Guide AI code contributions for this repo. Optimize for correctness, security, and consistency with existing architecture. Keep diffs minimal.

## Solution Overview
Projects:
- Domain: entities and rules (POCOs, persistence-agnostic).
- Application: CQRS (MediatR) requests/handlers, validators, mappings, repository/spec abstractions.
- Infrastructure.Persistence: EF Core DbContext, configurations, repository implementations.
- API: ASP.NET Core host, controllers, DI, middleware, versioning.
- Models: DTOs / API contracts only (no EF entities).
- Common: logging/constants/settings/attributes/utilities.
- Tests: unit and integration.

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
- .NET 9, ASP.NET Core, EF Core, MediatR, FluentValidation, AutoMapper, Swagger/Swashbuckle, JWT auth, Application Insights.
- Follow versions pinned in project files; avoid hardcoding versions in docs/samples.

## Performance Guidance
- Favor pagination and server-side filtering/sorting.
- Use indexes and EF configurations where necessary (migrations when requested).
- Avoid synchronous over async paths, excessive allocations, and unnecessary materialization.

## Testing Guidance
- Handlers: arrange mocks, call `Handle`, assert outputs/interactions (repository calls, log events when relevant).
- Validators: assert failure for bad inputs and success for good ones; verify error messages and property names.
- Mapping: assert AutoMapper configuration is valid and representative projections map correctly.
- API: minimal controller tests focusing on routing/status mapping; prefer handler/unit tests for logic.

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

## When Unsure
- Prefer established patterns already in the repo to maintain coherence.
- Default to explicit, minimal, and reversible changes.
