# AdventureWorks - Claude Code Configuration

Enterprise application demonstrating modern software architecture patterns across multiple platforms and services.

For the current project description, tech stack, and project structure, read the root `README.md`. It is the single source of truth.

## Child CLAUDE.md Files and Rules

Each application has its own CLAUDE.md with technology-specific instructions. Read it before working in that directory.

| Working on                     | Triggers                                                                                                                                                                        | Load                                     |
| ------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------- |
| **.NET REST API**              | Adding an endpoint, writing a controller, MediatR command/query handler, FluentValidation validator, AutoMapper profile, EF Core repository, anything Clean Architecture / CQRS | `apps/api-dotnet/.claude/CLAUDE.md`      |
| **Angular Web**                | Building a component, generating an Nx library, wiring an NgRx SignalStore, MSAL / Entra auth, Tailwind / DaisyUI / Alpine Circuit styling, anything signals-based              | `apps/angular-web/.claude/CLAUDE.md`     |
| **Database Migrations**        | Schema change, new migration, creating or updating a stored procedure, making DDL/DML idempotent, ordering DbUp scripts                                                         | `database/dbup/.claude/CLAUDE.md`        |
| **Sales Order Saga Functions** | Service Bus trigger, Durable Functions orchestrator/activity, the local NuGet feed for Domain/Application, Azurite/Service Bus emulator local dev                               | `apps/functions-dotnet/.claude/CLAUDE.md` |
| **BOM Cost Functions (Rust)**  | Rust custom handler, tiberius/bb8 SQL access, BOM cost explosion, Redis result cache, Cosmos DB writer, `bom-batch-requests` Service Bus trigger, cargo fmt/clippy/test         | `apps/functions-rust/.claude/CLAUDE.md`  |

Path-scoped rules live in `.claude/rules/`. Each file declares `paths:` globs in its frontmatter and loads automatically when a matching file is read or edited. `libs/dotnet/AdventureWorks.Connections` is a shared .NET library consumed by the API, the Functions app, DbUp, and the console tools.

## Repository-Wide Standards

### Git Workflow

**Branch Strategy:**

- `main` - We are working in a capstone project. Keep it simple and only work off of main for now.
- **NEVER** create a feature branch nor worktree unless specifically asked to do so.
- **NEVER** commit code nor suggest to commit code. Claude Code writes code. Humans commit
- **NEVER** raise a pull-request nor suggest to raise a pull request. Humans do that work.

### Naming Conventions

- `PascalCase` - Classes, interfaces, public members, constants
- `camelCase` - Private fields, parameters, local variables
- `_camelCase` - Private readonly fields (underscore prefix)
- `kebab-case` - File names (except C# which uses PascalCase)
- Feature-based directory organization: group by domain/feature, not by technical type

### Code Quality Standards

**Required:**

- ✅ Async/await for all I/O operations (never `.Result` or `.Wait()`)
- ✅ Null checks at method boundaries (`ArgumentNullException.ThrowIfNull`)
- ✅ Explicit error handling (no silent catch blocks)
- ✅ Unit tests for new features (80%+ coverage target)
- ✅ No magic strings - use constants
- ✅ No secrets in code - use environment variables/Key Vault

**Forbidden:**

- ❌ Hardcoded credentials, connection strings, API keys
- ❌ Blocking async code (`.Result`, `.Wait()`, `Task.Run` for I/O)
- ❌ Empty catch blocks that swallow exceptions
- ❌ Magic numbers/strings without explanation
- ❌ Circular dependencies between projects
- ❌ Braceless `if` statements — every `if`/`else` body must use `{ }`, even single-liners

### Security Rules

**MANDATORY across all applications:**

1. **Authentication Required** - All write endpoints (POST/PUT/PATCH/DELETE) require authentication
2. **Validate All Input** - Never trust user input, validate at boundaries
3. **No Secrets in Code** - Use Azure Key Vault (prod) or User Secrets (dev)
4. **Principle of Least Privilege** - Request minimum permissions needed
5. **Correlation IDs** - Include request tracing IDs in all responses

### Documentation Standards

**NEVER proactively create documentation files (\*.md, README, etc.) unless explicitly requested.**

When documentation is needed: use concise, technical language; focus on "why" over "what"; include code references with file paths and line numbers; keep examples minimal; update existing docs rather than creating new ones.

### Environment Configuration

- Development: User Secrets for sensitive configuration, `appsettings.Development.json` for non-sensitive overrides, local SQL Server or containers
- Production: Azure Key Vault for all secrets, `appsettings.Production.json` for environment config, managed identities where possible

### Docker and Infrastructure

- Docker Compose lives in `docker/` (`compose.yml`, `.env.example`). The database is not containerized; the API points at an existing SQL Server. Full docs in `docker/README.md`.
- Bicep templates live in `infra/`. CI/CD and infrastructure rules load from `.claude/rules/ci-cd-infra.md` when you touch pipeline or infra files.

## Universal Working Rules

- **Verify flags before using them.** Read the schema, run `--help`, or check `project.json` executor options. Never assume a flag works.
- **Read the source before writing mocks.** Read the actual service/component to get exact property names and `inject()` dependencies. Never guess signal names or method signatures.
- **Root cause first.** If a fix produces the same category of failure, stop. State the root cause, find all instances, fix them in one pass.
