# AdventureWorks Web API

**Purpose**: Guide code generation for this .NET 9.0 Clean Architecture API with CQRS patterns.

## Project Boundaries (CRITICAL)

**What Can Reference What:**
- **API** → Application, Models, Common (NOT Infrastructure or Domain directly)
- **Application** → Domain, Common (NOT Infrastructure - only via interfaces)
- **Infrastructure.Persistence** → Application (interfaces), Domain, Common
- **Domain** → Nothing (pure entities, no dependencies)
- **Models** → Nothing (pure DTOs)

**Rule**: Never let Infrastructure leak into Application. Use interface abstractions in Application.

## Domain Areas
- **Sales** - Store, SalesPerson, SalesTerritory, SalesOrderHeader/Detail
- **AddressManagement** - Address, AddressType, StateProvince, CountryRegion
- **HumanResources** - Employee entities

## Naming Conventions (Follow These)

- **Entities**: `*Entity` suffix → `StoreEntity`, `SalesPersonEntity`
- **Commands**: `Create*Command`, `Update*Command`
- **Queries**: `Read*Query`, `Read*ListQuery`
- **Handlers**: `*CommandHandler`, `*QueryHandler` (sealed classes)
- **Validators**: `Create*Validator`, `Update*Validator`
- **DTOs**: `*Model`, `*CreateModel`, `*UpdateModel`, `*SearchModel`
- **Repositories**: `I*Repository` interface, `*Repository` implementation

## DON'Ts (Anti-Patterns to Avoid)

- ❌ **NO** `.Result` or `.Wait()` - Use async/await throughout
- ❌ **NO** returning Entity classes from API - Use Models/DTOs only
- ❌ **NO** DbContext in Controllers/Handlers - Use repositories
- ❌ **NO** magic strings for config - Use constants in `Common.Constants`
- ❌ **NO** swallowing exceptions - Let middleware handle them
- ❌ **NO** N+1 queries - Use `.Include()` or projection

## When Adding a New Feature

Create these artifacts in `/Features/{FeatureName}/`:
1. **Command/Query** class in `/Commands` or `/Queries`
2. **Handler** (sealed, uses `IRequestHandler<TRequest, TResponse>`)
3. **Validator** in `/Validators` (extends base validator if available)
4. **AutoMapper Profile** in `/Profiles` (separate for Create/Update/Read)
5. **Controller** endpoint in API (thin, delegates to `_mediator.Send()`)
6. **Tests** in test project

## CQRS Pattern

- **Commands** (Write): Return int (ID), Guid, or void
- **Queries** (Read): Return Model/DTO
- All go through MediatR: `await _mediator.Send(request, cancellationToken)`
- Handlers are sealed, use primary constructors

## Validation & Security

- **Validate** all external input (IDs > 0, required fields non-empty)
- **FluentValidation** for all commands (separate validator class)
- **Authorization**: New endpoints default to `[Authorize]` unless explicitly public
- **Never** expose internal entity properties to API consumers

## Data Access Rules

- **Repositories**: Define interfaces in Application, implement in Infrastructure
- **Tracking**: Use `.AsNoTracking()` for read-only queries
- **Queries**: Avoid loading large collections - project to DTOs in EF query when possible

## Technology Stack
- .NET 9.0, EF Core 9.0.5, SQL Server
- MediatR 12.5.0, FluentValidation 12.0.0, AutoMapper 14.0.0
- JWT Bearer auth, Swagger, Azure Key Vault

## Code Quality

- **Sealed classes** for all handlers and repositories
- **Primary constructors** for DI (C# 12)
- **Feature folders**: `/Features/{Area}/{Commands|Queries|Validators|Profiles}/`
- **Async throughout**: Include `CancellationToken` parameters
- Use `ArgumentNullException.ThrowIfNull()` for null checks

## Example API Routes
```
POST   /api/v1/stores          - Create (auth required)
GET    /api/v1/stores/{id}     - Read
PUT    /api/v1/stores/{id}     - Update (auth required)
POST   /api/v1/stores          - Create store (requires auth)
GET    /api/v1/stores/{id}     - Read store
PUT    /api/v1/stores/{id}     - Update store (requires auth)
POST   /api/v1/addresses       - Create address (requires auth)
GET    /api/v1/addresses/{id}  - Read address
POST   /api/v1/salespersons    - Create sales person (requires auth)
GET    /api/v1/salespersons/{id} - Read sales person
```