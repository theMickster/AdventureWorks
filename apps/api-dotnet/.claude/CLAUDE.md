# AdventureWorks Web API - Claude Code Configuration

RESTful API built with .NET 10.0 implementing Clean Architecture with CQRS for the AdventureWorks Cycling database.

## Overview

- Exposes REST endpoints for the Sales, HumanResources, Person, Production, ProductReview, Purchasing, Dashboard, and AddressManagement feature folders under `AdventureWorks.Application/Features/`
- CQRS via MediatR with FluentValidation and AutoMapper; JWT auth via Microsoft Identity Web; secrets from Azure Key Vault

### Key Concepts

- **Clean Architecture**: Project references point inward: API -> Infrastructure.Persistence -> Application -> Domain. Infrastructure is reached only through interfaces; a reverse reference would be circular and the build rejects it.
- **CQRS**: Commands write and return the new ID, `Unit`, or a model; queries read with `AsNoTracking()` and return DTOs, never entities.
- **Naming**: DTOs are `*Model`, `*CreateModel`, `*UpdateModel`. Domain entities carry no consistent suffix (older ones use `*Entity`, newer ones such as `WorkOrder` and `Vendor` do not); match the neighbouring entities in the same domain folder and do not rename.
- **Repository Pattern**: New interfaces go in `Application/PersistenceContracts/Repositories/{Domain}/` and implementations in `Infrastructure.Persistence/Repositories/{Domain}/`. Older ones sit at the folder roots; leave them there.
- **Feature Folders**: Organized by domain, not technical layer.

## Progressive Context Loading

| Task                                                                                                                          | Guide                                                                      |
| ----------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------- |
| Adding or implementing a feature / endpoint                                                                                   | [`guides/adding-features.md`](guides/adding-features.md)                   |
| Writing unit tests (handler / controller / validator)                                                                         | [`guides/testing-guide.md`](guides/testing-guide.md)                       |
| Writing integration tests (HTTP pipeline, middleware, auth gate)                                                              | [`guides/testing-guide.md`](guides/testing-guide.md) — Integration section |
| Editing Postman collections, environments, or scripts — **required reading before any edit under `apps/api-dotnet/postman/`** | [`guides/postman-guide.md`](guides/postman-guide.md)                       |
| Configuring the VS Code debugger (FluentValidation / KeyNotFound / Conflict exception filters)                                 | [`guides/debugging-guide.md`](guides/debugging-guide.md)                   |
| Smoke-testing live endpoints with a dev token (curl)                                                                          | [`skills/smoke-testing-dotnet-api.md`](skills/smoke-testing-dotnet-api.md) |

Handler structure is in the path-scoped rule `.claude/rules/dotnet-handlers.md`; repository query invariants are in `.claude/rules/dotnet-repository-queries.md`. Both load automatically from the repo root when you edit matching files.

## Architecture

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

```
apps/api-dotnet/src/
+-- AdventureWorks.API/              # Controllers, middleware, hubs, Program.cs
+-- AdventureWorks.Application/      # Features/{Domain}/Commands|Queries|Validators|Profiles, PersistenceContracts
+-- AdventureWorks.Domain/           # Entities (no dependencies)
+-- AdventureWorks.Infrastructure.Persistence/  # Repositories, DbContext, notification handlers
+-- AdventureWorks.Models/           # DTOs
+-- AdventureWorks.Common/           # Constants, filtering parameters, helpers

apps/api-dotnet/tests/
+-- AdventureWorks.UnitTests/        # xUnit, Moq, FluentAssertions, EF Core InMemory, UnitTestBase
+-- AdventureWorks.IntegrationTests/ # WebApplicationFactory<Program>, CustomWebApplicationFactory, TestAuthHandler
+-- AdventureWorks.Test.Common/      # Fixtures shared by both test projects
+-- AdventureWorks.LoadTests.k6/     # k6 TypeScript profiles; see its README and run-tests.sh

libs/dotnet/AdventureWorks.Connections/  # IConnectionCatalog; the API reads ConnectionStrings:AdventureWorks through it
```

## Core Patterns

- **Controllers** are thin and delegate to MediatR: `[ApiController]`, `[ApiVersion("1.0")]`, explicit auth intent. Commands return `CreatedAtRoute()`, queries `Ok(model)`. Return DTOs, never entities.
- **Validators** use `WithErrorCode("Rule-NN")` and static message properties; handlers call `ValidateAndThrowAsync()`. Delete commands have no validator; every other command does.
- **AutoMapper** profiles: one per mapping direction. Ignore audit fields (`ModifiedDate`, `Rowguid`, `Id`); use `ForPath()` for nested members.
- **Exception middleware** (`AdventureWorks.API/libs/Middleware/ExceptionHandlerMiddleware.cs`): `ValidationException` -> 400 with errors, `KeyNotFoundException` -> 404, `ConflictException` -> 409, anything else -> 500 with a generic message. Every response carries `X-Correlation-Id`.

### Composite-Key Rewrite

A column that is part of a composite primary key cannot be updated. Change it through a repository `Replace*Async` that deletes the row and inserts the replacement inside one `BeginTransactionAsync` so handlers stay storage-agnostic. See `PersonPhoneRepository.ReplacePhoneAsync`, `BusinessEntityContactEntityRepository.ReplaceContactTypeAsync`, and `BusinessEntityAddressRepository.ReplaceAddressTypeAsync`; the same transactional shape closes and opens temporal rows in `EmployeeRepository.TransferEmployeeDepartmentAsync`. The handler checks uniqueness against the target key before calling the repository, and the controller issues a follow-up read query for the response.

Add `.ValueGeneratedOnAdd()` to any IDENTITY column inside a composite `HasKey`, or EF inserts 0 instead of letting SQL Server assign it. See `EmailAddressConfiguration`.

## Exception and Contract Policy

- Every `[ProducesResponseType]` status a controller declares has a test that produces it.
- Pick one not-found strategy per flow: return `null` and map to `NotFound()` in the controller, or throw `KeyNotFoundException`. Never both. Any other exception type becomes a 500.
- Every new exception type thrown for a normal client failure gets a `case` in `ExceptionHandlerMiddleware.HandleException` in the same change.
- Do not rely on a follow-up read after a command to correct an exception path that already failed.
- Echo only messages the app wrote (`KeyNotFoundException`, `ConflictException`). Never echo framework or database exception text.

## Validation Guardrails

- Check referenced IDs with `MustAsync` against the repository in the validator (see `StoreBaseModelValidator`), not by catching FK violations or `DbUpdateException`. The unique-constraint-to-409 translation in `EmployeeRepository` is a backstop, not the primary check.
- Patch handlers validate the patched model before saving. See `PatchStoreCommandHandler`.
- Validators cover constrained values and relationships, not just required fields. Put bounded code sets in a `private static readonly HashSet<string?>`, never an inline array inside `Must()`. See `ProductBaseModelValidator`.

## Async and Query Guardrails

- Every repository method takes `CancellationToken cancellationToken = default` and passes it to the EF call; controllers and handlers pass it through. `IAddressRepository.GetAddressByIdAsync` and `ICountryRegionRepository.GetByIdAsync` are known violations; do not copy them.
- Include flags decide whether a query or `Include` runs; never load and then clear. See `ReadStoreQueryHandler`.
- Wrap user text in `LikePatternHelper.EscapeLikePattern` before interpolating it into an `EF.Functions.Like` pattern, because unescaped `%` and `_` match everything and are a DoS vector on public endpoints. See `StoreRepository`.

## Auth Intent Guardrails

- Write endpoints are protected by default; `WriteEndpointAuthorizationTests` fails if one is not.
- Mark an intentionally public endpoint with `[AllowAnonymous]` and a `<remarks>` saying why. See `ReadCountryRegionController`.
- If a read-only operation uses `POST`, put the auth attribute on the action so the intent is visible at the call site.

## Testing

```bash
dotnet test                                            # Run all (from apps/api-dotnet)
dotnet test --filter "FullyQualifiedName~CreateStore"  # Filtered; exit code 8 means a project matched zero tests, not a failure
```

Use integration tests to verify HTTP pipeline behavior (middleware, routing, auth gates, health/version endpoints) that unit tests cannot observe. See `guides/testing-guide.md` for templates.

For new or changed API flows, cover: success; validation failure; not-found read and update; invalid referenced IDs; auth intent when protected, public, or read-only-over-POST; and repository behavior when include flags change the loaded data.

## Formatting and Tooling

- `apps/api-dotnet/.editorconfig` carries analyzer severities only. Use 4-space indentation, file-scoped namespaces, and alphabetically sorted usings (`AdventureWorks.*` sorts before `Microsoft.*`). Line endings are mixed in the tree; keep whatever the file already uses.
- VS Code breaks on `FluentValidation.ValidationException`, `KeyNotFoundException`, and `ConflictException` because the middleware catches them. `guides/debugging-guide.md` shows how to add them to the User-Unhandled ignore list.

## AI Done Gate

Before finishing an API task, verify:

- controller attributes, XML docs, and response behavior agree, and every declared status has a test
- expected exceptions are mapped intentionally in the controller or middleware
- validators cover referenced IDs and update/patch edge cases
- new async methods accept and forward `CancellationToken`
- include/filter flags change data access, not only the mapped output
- a new list filter query parameter is also added to the Angular list component's `queryParams` subscription, or it will not round-trip through the URL
