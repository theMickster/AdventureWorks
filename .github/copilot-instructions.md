# AdventureWorks Copilot Instructions

Purpose: provide repo-wide defaults for AI contributions. Keep diffs small, preserve existing architecture, and read the nearest scoped instruction file before changing app-specific code.

## Instruction Hierarchy

Use the most specific guidance that applies to the files you are touching:

- Repo-wide defaults: `.github/copilot-instructions.md`
- API-specific GitHub Copilot guidance: `.github/instructions/api-dotnet.instructions.md`
- Angular-specific GitHub Copilot guidance: `.github/instructions/angular-web.instructions.md`
- DbUp-specific guidance: `.github/instructions/dbup.instructions.md`
- API Claude-specific guidance: `apps/api-dotnet/.claude/CLAUDE.md`
- API implementation/testing walkthroughs:
  - `apps/api-dotnet/.claude/guides/adding-features.md`
  - `apps/api-dotnet/.claude/guides/testing-guide.md`

When guidance overlaps, prefer the more specific file.

## Repository Structure

This monorepo contains:

- `apps/api-dotnet/` — .NET 10 API
- `apps/angular-web/` — Angular 21 + Nx frontend
- `database/dbup/` — DbUp migration tooling
- `database/sql-change-automation/` — SQL Change Automation project
- `tools/console-apps/` — utility console apps

## Repo-Wide Rules

- Keep changes targeted. Do not refactor unrelated code while implementing a feature or fix.
- Match existing naming, file placement, and architectural boundaries.
- Use only existing build, test, lint, and formatting tools already present in the touched app.
- Never introduce secrets, credentials, or environment-specific values into committed files.
- Prefer explicit, user-visible behavior over implicit assumptions.
- Add or update tests when behavior changes, especially for failure paths and regressions.

## Cross-Cutting Quality Guardrails

- Thread cancellation/abort intent through async flows when the stack supports it.
- Keep exception and failure behavior intentional; the nearest scoped instruction file defines app-specific policies.
- Public-by-exception behavior must be explicit and documented close to the code.

## Validation Commands

Use the commands appropriate to the app you changed.

### API

Run from `apps/api-dotnet/`:

```bash
dotnet build AdventureWorks.sln
dotnet test AdventureWorks.sln
```

### Angular

Run from `apps/angular-web/`:

```bash
npm run lint
npm run test
npm run build
```

## When Unsure

- Read the nearest scoped instruction file first.
- Prefer established patterns already present in the target app.
- Default to explicit, reversible, low-risk changes.
