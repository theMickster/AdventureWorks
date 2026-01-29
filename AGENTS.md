# AdventureWorks: Codex instructions

## Start with the right context

- Read the root [README.md](README.md) for the current stack, project layout, and local setup.
- Before changing code, read the guidance for every area you will touch:

| Area | Scoped guidance |
| --- | --- |
| .NET API | [API Copilot instructions](.github/instructions/api-dotnet.instructions.md) and [API Claude guidance](apps/api-dotnet/.claude/CLAUDE.md) |
| Angular and Nx | [Angular Copilot instructions](.github/instructions/angular-web.instructions.md), [Angular Claude guidance](apps/angular-web/.claude/CLAUDE.md), and [workspace README](apps/angular-web/README.md) |
| .NET Functions | [Functions Copilot instructions](.github/instructions/functions-dotnet.instructions.md) and [Functions Claude guidance](apps/functions-dotnet/.claude/CLAUDE.md) |
| Rust Functions | [Rust Copilot instructions](.github/instructions/functions-rust.instructions.md) and [Rust Claude guidance](apps/functions-rust/.claude/CLAUDE.md) |
| DbUp migrations | [DbUp Claude guidance](database/dbup/.claude/CLAUDE.md) and [DbUp README](database/dbup/AdventureWorks.DbUp/README.md) |

For CI, infrastructure, database projects other than DbUp, and tools, start with the nearest README and project configuration; consult the [root Claude guidance](.claude/CLAUDE.md) where relevant. Verify commands, paths, framework versions, and APIs against the current project. The [DbUp Copilot instructions](.github/instructions/dbup.instructions.md) contain an obsolete path and .NET version; use the current DbUp project and its guidance above.

## Work in this repository

- Keep changes focused on the request. Match existing feature organization, naming, and architectural boundaries; do not refactor unrelated code.
- Read the code and project configuration before adding dependencies, using CLI flags, or writing mocks. Follow existing patterns and use the touched project's build, test, lint, and formatting tools.
- Use async I/O without blocking calls, propagate cancellation where supported, validate input at boundaries, and handle expected failures deliberately.
- Protect write endpoints by default. Make any intentionally public endpoint explicit in code and explain why near the endpoint.
- Never commit secrets, credentials, connection strings, or machine-specific configuration. Use the project's existing local secrets and Azure identity/Key Vault patterns.
- Add or update meaningful tests when behavior changes, including important failure paths and regressions. Run validation appropriate to the changed area and report what ran and any limits.
- Do not create documentation files unless the user asks for them; update existing docs when a change requires it.

## Dependency security

**Dependency security: 7-day cool-off (MANDATORY).** Never add, upgrade, pin, or recommend any package version (npm, NuGet, Cargo, container image, GitHub Action, or anything else from a public registry) published fewer than 7 days (168 hours) ago, even if it is the `latest` tag and even for a security fix; if the only fix is younger than 7 days, stop and ask the user. Before proposing a version, build candidates from the registry's `versions` list plus dist-tags (the `time` field also lists unpublished versions), then confirm the publish date. npm projects enforce this with `min-release-age=7` in their `.npmrc`; never remove it, override it, or install with `@latest`, `ng update`, or `nx migrate` defaults without pinning explicit eligible versions. After any lockfile change run `npm audit` and `npm audit signatures` and report the results.

## Git ownership

Work on the current checkout. Do not create a branch or worktree, commit, or open a pull request unless the user explicitly asks. Humans own those Git steps by default.
