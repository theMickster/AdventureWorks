# AdventureWorks - Claude Code Configuration

Enterprise application demonstrating modern software architecture patterns across multiple platforms and services.

## Repository Overview

For the current project description, tech stack, and project structure, read the root README — it is the single source of truth:

> Use the `Read` tool on `README.md` at the repository root.

## Working with Child CLAUDE.md Files

Each application/service has its own CLAUDE.md with technology-specific instructions:

- **For .NET API work**: See `apps/api-dotnet/.claude/CLAUDE.md`
- **For Angular work**: See `apps/angular-web/.claude/CLAUDE.md`
**When starting work in a specific application:**

1. Navigate to that application's directory
2. Read its CLAUDE.md file for context-specific instructions
3. Follow technology-specific patterns and conventions

## Repository-Wide Standards

### Git Workflow

**Branch Strategy:**

- `main` - We are working in a capstone project. Keep it simple and only work off of main for now.
- **NEVER** create a feature branch nor worktree unless specifically asked to do so.
- **NEVER** commit code nor suggest to commit code. Claude Code writes code. Humans commit
- **NEVER** raise a pull-request nor suggest to raise a pull request. Humans do that work.

### Naming Conventions

**General Rules:**

- `PascalCase` - Classes, interfaces, public members, constants
- `camelCase` - Private fields, parameters, local variables
- `_camelCase` - Private readonly fields (underscore prefix)
- `kebab-case` - File names (except C# which uses PascalCase)

**Directory Structure:**

- Feature-based organization over layer-based
- Group by domain/feature, not by technical type

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

When documentation is needed:

- Use concise, technical language
- Focus on "why" over "what"
- Include code references with file paths and line numbers
- Keep examples minimal and relevant
- Update existing docs rather than creating new ones

### Environment Configuration

**Development:**

- Local User Secrets for sensitive configuration
- `appsettings.Development.json` for non-sensitive overrides
- Local SQL Server / Docker containers

**Production:**

- Azure Key Vault for all secrets
- `appsettings.Production.json` for environment config
- Managed identities where possible

### Docker Local Development

- `docker-compose.yml` at repo root — 2 services: `api` (.NET 10.0) and `web` (Angular/nginx)
- Database is NOT containerized — developers point the API at their existing SQL Server via `CONNECTION_STRING` env var
- API Dockerfile: `apps/api-dotnet/src/AdventureWorks.API/Dockerfile` — 4-stage multi-stage build, `aspnet:10.0` runtime with curl for healthcheck, non-root
- Angular Dockerfile: `apps/angular-web/Dockerfile` — `node:24-alpine` build, `nginxinc/nginx-unprivileged:alpine` serve
- nginx.conf at `apps/angular-web/nginx.conf` — reverse proxies `/api/` to the API container, SPA fallback, security headers, CSP
- `appsettings.Docker.json` — skips Key Vault, dummy App Insights, loaded when `ASPNETCORE_ENVIRONMENT=Docker`
- `environment.docker.ts` — no auth block (MSAL disabled), `baseUrl: '/api'`
- `docker` build configuration in Angular `project.json`
- `.env.docker.example` committed, `.env.docker` gitignored
- Ports: API 5000, Web 4200
- Full docs in `DOCKER.md` at repo root

### Infrastructure as Code

- `infra/` directory at repo root contains modular Bicep templates
- `main.bicep` orchestrator + 6 modules under `modules/` (appServicePlan, appService, sqlServer, sqlDatabase, keyVault, appInsights)
- `parameters.dev.json` and `parameters.prod.json` for environment-specific values -- no secrets in param files (passwords passed at deploy time)
- Validate with: `az bicep build --file infra/main.bicep`
- Deploy strategy: same resource group for dev + prod; shared plan/SQL/Key Vault/App Insights, env-specific App Services
- Budget: ~$18/mo (B1 Linux plan + Basic 5 DTU SQL on MSDN subscription)

## Getting Started

**When beginning work in this repository:**

1. **Identify the application** you'll be working on
2. **Navigate to its directory** (e.g., `cd apps/api-dotnet`)
3. **Read its CLAUDE.md** for technology-specific context
4. **Check for guide files** in `.claude/guides/` for detailed walkthroughs
5. **Follow this file** for repository-wide standards (git, naming, security)

## Progressive Context Loading

**Claude should load additional context based on the task:**

| Working on                   | Triggers                                                                                                                                                                        | Load                                                                          |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| **.NET REST API**            | Adding an endpoint, writing a controller, MediatR command/query handler, FluentValidation validator, AutoMapper profile, EF Core repository, anything Clean Architecture / CQRS | [`apps/api-dotnet/.claude/CLAUDE.md`](../apps/api-dotnet/.claude/CLAUDE.md)   |
| **Angular Web**              | Building a component, generating an Nx library, wiring an NgRx SignalStore, MSAL / Entra auth, Tailwind / DaisyUI / Alpine Circuit styling, anything signals-based              | [`apps/angular-web/.claude/CLAUDE.md`](../apps/angular-web/.claude/CLAUDE.md) |
| **Database Migrations**      | Schema change, new migration, creating or updating a stored procedure, making DDL/DML idempotent, ordering DbUp scripts                                                         | [`database/dbup/.claude/CLAUDE.md`](../database/dbup/.claude/CLAUDE.md)       |

## CI/CD and Infrastructure Rules

### Before Using Any CLI Flag or Task Input

Verify it exists — read the schema, run `--help`, or check `project.json` executor options. Never assume a flag works.

### Gitignored Files Do Not Exist in CI

`environment.development.ts` and similar dev files are gitignored. CI commands must use explicit `--configuration` flags or `--exclude` to avoid referencing them.

### Read the Source Before Writing Mocks

Read the actual service/component to get exact property names and `inject()` dependencies. Never guess signal names or method signatures.

### Nx Tests: App ≠ Workspace

`nx test <app>` runs ONE project's tests. Use `nx run-many -t test` to run all libraries. Always verify the test count, not just pass/fail.

### CI Cache Keys Must Hit

Never put commit SHA or build ID in primary cache keys. For npm, cache `~/.npm`, not `node_modules` (`npm ci` deletes it).

### Azure Pipelines

- Use `PublishPipelineArtifact@1` / `DownloadPipelineArtifact@2` (not deprecated v1)
- Use `continueOnError: true` on tasks, never `|| true` in scripts
- Deployment jobs: use `$(Build.SourcesDirectory)` for absolute paths, not relative
- Cross-RG deployment names: `{name}-${environment}` to avoid collisions

### Verify Azure Resource Names

Run `az resource list` before writing Bicep or docs. Never assume names follow a convention.

### Root Cause First

If a fix produces the same category of failure, stop. State the root cause, find all instances, fix them in one pass.
