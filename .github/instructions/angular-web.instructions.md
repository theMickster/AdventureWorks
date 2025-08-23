# AdventureWorks Angular Web Copilot Instructions

**Applies to:** `apps/angular-web/**`

Use this file together with `apps/angular-web/README.md`.

## Workspace and Architecture

- This frontend is an Nx workspace rooted at `apps/angular-web/`.
- Respect library boundaries enforced by tags:
  - `feature -> feature, ui, data-access, util`
  - `ui -> ui, util`
  - `data-access -> data-access, util`
  - `util -> util`
- Respect scope boundaries (`shared`, `sales`, `hr`).
- Prefer placing reusable UI in `libs/shared/ui`, cross-cutting utilities in `libs/shared/util`, and API/domain access in `data-access` libraries.

## Angular Style Defaults

- Match the existing Angular 21 style:
  - standalone-first components
  - `ChangeDetectionStrategy.OnPush`
  - `inject(...)` for DI where that is the local pattern
  - Signals/computed state for local reactive state where appropriate
- Use the existing `@adventureworks-web/...` import paths instead of deep relative cross-library imports.
- Keep presentational UI components free of data-fetching or auth logic.

## API and Auth Rules

- Prefer shared HTTP/auth utilities over ad hoc wiring:
  - `libs/shared/util/src/lib/http/api.service.ts`
  - MSAL/auth utilities under `libs/shared/util/src/lib/auth/`
- Do not bypass shared interceptors, correlation ID handling, loading, or error handling without a strong reason.
- Preserve MSAL guard/interceptor patterns when changing authenticated routes or API calls.
- Never commit secrets or tenant-specific values; keep committed environment files placeholder-safe.

## UI and Behavior Guardrails

- Keep forms, tables, notifications, loading, and dialogs aligned with shared UI/util patterns before inventing new primitives.
- Prefer extending shared components/services over cloning similar components into feature areas.
- Maintain accessible, explicit empty/error/loading states for data-driven screens.
- Keep route changes aligned with the existing app route split (`sales`, `hr`, auth, not-found).

## Testing Expectations

- Add or update unit tests with Vitest for changed components, services, guards, and utilities.
- Add or update Playwright coverage when changes materially affect end-to-end flows.
- When fixing a bug, include a regression test where practical.

## Build and Validation

Run from `apps/angular-web/`:

```bash
npm run lint
npm run test
npm run build
```

Use project-specific Nx commands when narrower validation is enough, but prefer existing workspace targets over custom scripts.

## Done Gate

Before finishing an Angular change, verify:

- library/scope boundaries still hold
- imports use workspace aliases where appropriate
- shared auth/http patterns are preserved
- state and UI logic stay in the right layer
- tests cover the changed behavior
