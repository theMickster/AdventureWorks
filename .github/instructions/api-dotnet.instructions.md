---
applyTo: "apps/api-dotnet/**/*"
---

# AdventureWorks API Copilot Instructions

**Applies to:** `apps/api-dotnet/**`

Use this file together with:

- `apps/api-dotnet/.claude/CLAUDE.md`
- `apps/api-dotnet/.claude/guides/adding-features.md`
- `apps/api-dotnet/.claude/guides/testing-guide.md`

## Architecture Defaults

- Follow Clean Architecture and CQRS exactly: controllers -> MediatR -> handlers -> repositories.
- Use DTOs in `AdventureWorks.Models`; never expose EF/domain entities from the API surface.
- Keep controllers thin and push business/data-access logic into handlers and repositories.

## Exception Policy

- Documented response codes must match real behavior.
- Expected client-facing failures must be intentional:
  - missing resource -> `404`
  - invalid input -> `400`
  - unauthorized/forbidden -> auth response, not ad hoc logic
- Use one deliberate not-found pattern per flow:
  - return `null` and translate to `NotFound(...)`, or
  - throw a known exception type and map it intentionally
- If you introduce `KeyNotFoundException` or another expected exception type in handlers, update middleware/controller handling in the same change.
- Never assume a post-command read will “fix” a missing-resource path after the command already threw.
- Do not leak raw exception messages for expected request failures.

## Validation Rules

- Every create/update/patch input needs validation beyond superficial required fields.
- Validate FK-backed or externally referenced IDs when practical before persistence.
- Do not rely on SQL foreign keys or `DbUpdateException` as the first user-facing validation layer.
- For patch flows, validate the patched model after applying operations and before saving.

## Auth and Public Endpoint Rules

- Write endpoints (`POST`, `PUT`, `PATCH`, `DELETE`) are protected by default.
- If an endpoint is intentionally public, make that explicit in code with `[AllowAnonymous]` plus a short rationale in docs/comments.
- If a semantically read-only endpoint uses `POST`, call out why it is public or protected; do not leave that decision implicit.

## Async and Data-Access Rules

- Thread `CancellationToken` end-to-end: controller -> `_mediator.Send(...)` -> handler -> repository -> EF async call.
- New repository methods for reads must accept cancellation tokens when the underlying stack supports them.
- Read queries default to `AsNoTracking()` unless mutation requires tracking.
- Include/filter flags must shape the repository query itself. Do not eagerly load full graphs and then clear collections after mapping.
- Avoid unnecessary eager loading, duplicate include graphs, or result-shaping that wastes query work.
- When two or more repository methods share the same `Include` chain or paging execution, extract to a `private IQueryable<T> Build{Name}Query(...)` helper — do not duplicate the include graph across methods.
- Never use `.Result` or `.Wait()`.
- User input embedded in `EF.Functions.Like(...)` patterns must have SQL LIKE wildcards (`%`, `_`, `[`, `]`) escaped before interpolation. Use a shared `EscapeLikePattern(string input)` utility. This is a correctness and DoS risk; severity is higher on unauthenticated endpoints.
  - ❌ `EF.Functions.Like(p.Name, $"%{input}%")`
  - ✅ `EF.Functions.Like(p.Name, $"%{EscapeLikePattern(input)}%")`

## Validator Implementation Rules

- Fixed domain code sets (e.g., ProductLine, Class, Style allowed values) must be `private static readonly HashSet<string?>` fields, not inline `new[] { ... }` inside `Must(...)` predicates.

## Logging and Constants

- Use structured logging.
- Use `AppLoggingConstants.Http{Verb}RequestErrorCode` matching the HTTP verb of the endpoint — `HttpGetRequestErrorCode` for GET, `HttpPostRequestErrorCode` for POST, etc. Never copy-paste a logging line from a controller with a different verb.
- Do not log secrets, tokens, or sensitive identifiers beyond existing project norms.

## Minimum Test Expectations

When adding or changing API behavior, add or update tests for:

- success path
- invalid input / bad request
- not-found path for reads and updates when applicable
- invalid referenced IDs / validation failures when applicable
- protected vs intentionally public endpoint intent when auth behavior changes
- repository/query behavior when include flags materially affect loaded data

Favor controller tests, handler tests, validator tests, and repository tests that mirror the project’s existing layout.

## Done Gate

Before finishing an API change, verify:

- controller docs, attributes, and actual responses agree
- expected exceptions are translated intentionally
- validators cover referenced IDs and patch/update edge cases
- all new async methods accept and forward `CancellationToken`
- query flags change data access, not only mapped output
- tests cover both happy path and the main failure paths

## Build and Test

Run from `apps/api-dotnet/`:

```bash
dotnet build AdventureWorks.sln
dotnet test AdventureWorks.sln
```
