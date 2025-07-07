# AdventureWorks - Claude Code Configuration

Enterprise application demonstrating modern software architecture patterns across multiple platforms and services.

## Repository Overview

This monorepo contains:
- **.NET 10.0 REST API** (`apps/api-dotnet/`) - Clean Architecture with CQRS
- **Angular Web Application** (`apps/angular-web/`) - Coming soon
- **Microservices** (`apps/microservices/`) - Coming soon
- **Database Scripts** (`database/`) - AdventureWorks schema and migrations

## Working with Child CLAUDE.md Files

Each application/service has its own CLAUDE.md with technology-specific instructions:

- **For .NET API work**: See `apps/api-dotnet/.claude/CLAUDE.md`
- **For Angular work**: See `apps/angular-web/.claude/CLAUDE.md` (future)
- **For Microservices**: See `apps/microservices/{service}/.claude/CLAUDE.md` (future)

**When starting work in a specific application:**
1. Navigate to that application's directory
2. Read its CLAUDE.md file for context-specific instructions
3. Follow technology-specific patterns and conventions

## Repository-Wide Standards

### Git Workflow

**Branch Strategy:**
- `main` - Production-ready code
- `develop` - Integration branch (future)
- `feature/*` - Feature branches
- `bugfix/*` - Bug fix branches

**Commit Message Format:**
```
<type>(<scope>): <subject>

<body>

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
```

**Types**: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`
**Scopes**: `api`, `web`, `db`, `auth`, `sales`, etc.

**Example:**
```
feat(api): add customer CRUD endpoints

Implements Create, Read, Update, Delete operations for customers
following Clean Architecture and CQRS patterns.

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
```

### Creating Commits

**ONLY create commits when the user explicitly asks.** Follow these steps:

1. Run `git status` and `git diff` (both staged and unstaged)
2. Draft a commit message following the format above
3. Add relevant files: `git add <files>`
4. Create commit with Co-Authored-By trailer
5. Run `git status` after commit to verify

**NEVER:**
- Use `git commit --amend` unless explicitly requested
- Use `--no-verify` or `--no-gpg-sign` flags
- Force push to `main`/`master`
- Commit files with secrets (`.env`, `credentials.json`)

### Creating Pull Requests

**When the user asks to create a PR:**

1. Run `git status`, `git diff`, and `git log` to understand full scope
2. Review ALL commits that will be included (not just latest)
3. Draft PR summary covering all changes
4. Use format:
   ```markdown
   ## Summary
   - Bullet points of changes

   ## Test plan
   - [ ] Checklist of testing steps

   🤖 Generated with [Claude Code](https://claude.com/claude-code)
   ```
5. Create PR: `gh pr create --title "..." --body "$(cat <<'EOF' ... EOF)"`

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

### Security Rules

**MANDATORY across all applications:**

1. **Authentication Required** - All write endpoints (POST/PUT/PATCH/DELETE) require authentication
2. **Validate All Input** - Never trust user input, validate at boundaries
3. **No Secrets in Code** - Use Azure Key Vault (prod) or User Secrets (dev)
4. **Principle of Least Privilege** - Request minimum permissions needed
5. **Correlation IDs** - Include request tracing IDs in all responses

### Documentation Standards

**NEVER proactively create documentation files (*.md, README, etc.) unless explicitly requested.**

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

## Technology Stack

### Current
- **.NET 10.0** - Backend API
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping
- **xUnit** - Testing framework

### Future
- **Angular 17+** - Frontend web application
- **Docker** - Containerization
- **Kubernetes** - Orchestration (maybe)
- **Azure Services** - Cloud infrastructure

## Project Structure

```
AdventureWorks/
├── .claude/
│   └── CLAUDE.md                    # This file (repo-wide standards)
│
├── apps/
│   ├── api-dotnet/                  # .NET REST API
│   │   ├── .claude/
│   │   │   ├── CLAUDE.md            # .NET-specific instructions
│   │   │   └── guides/              # Detailed implementation guides
│   │   ├── src/                     # Source code
│   │   └── tests/                   # Unit tests
│   │
│   ├── angular-web/                 # Angular SPA (future)
│   └── microservices/               # Additional services (future)
│
├── database/
│   ├── dbup/                        # Database migrations
│   └── scripts/                     # SQL scripts
│
└── docs/                            # Shared documentation
```

## Getting Started

**When beginning work in this repository:**

1. **Identify the application** you'll be working on
2. **Navigate to its directory** (e.g., `cd apps/api-dotnet`)
3. **Read its CLAUDE.md** for technology-specific context
4. **Check for guide files** in `.claude/guides/` for detailed walkthroughs
5. **Follow this file** for repository-wide standards (git, naming, security)

## Progressive Context Loading

**Claude should load additional context based on the task:**

- **"Add a feature to the API"** → Read `apps/api-dotnet/.claude/CLAUDE.md` + `guides/adding-features.md`
- **"Write tests for the API"** → Read `apps/api-dotnet/.claude/CLAUDE.md` + `guides/testing-guide.md`
- **"Create a new microservice"** → Read `apps/microservices/.claude/CLAUDE.md` (future)
- **"Build an Angular component"** → Read `apps/angular-web/.claude/CLAUDE.md` (future)

**Pattern:** Always read the application's CLAUDE.md + relevant guide files when starting work in that application.

---

**Version**: Monorepo Structure v1.0
**Last Updated**: 2026-01-17
**Primary Application**: .NET 10.0 REST API (Clean Architecture with CQRS)
