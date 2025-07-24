# Contributing to AdventureWorks

## Getting Started

1. Fork the repository
2. Create a branch from `main` (`feature/*`, `bugfix/*`, or `refactor/*`)
3. Make your changes
4. Submit a pull request

## Development Setup

**Prerequisites**: .NET 10.0 SDK, Node.js 22+, SQL Server

```bash
# .NET API
cd apps/api-dotnet && dotnet build

# Angular Web
cd apps/angular-web && npm install && npx nx serve adventureworks-web
```

## Commit Messages

```
<type>(<scope>): <subject>
```

**Types**: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`
**Scopes**: `api`, `web`, `db`, `auth`, `sales`, `hr`

## Code Standards

- Follow existing patterns in the codebase
- Write unit tests for new functionality
- No hardcoded secrets — use User Secrets (dev) or Key Vault (prod)
- Async/await for all I/O operations
- See each app's `.claude/CLAUDE.md` for technology-specific conventions
