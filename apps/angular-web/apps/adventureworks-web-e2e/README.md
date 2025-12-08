# AdventureWorks Web E2E (Playwright)

Playwright end-to-end smoke tests for `adventureworks-web`. Page Objects live in
`src/page-objects/`; the authenticated storage-state fixture lives in `src/support/`.

## Required environment variables

| Variable             | Required for                                                                                                                                           |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `E2E_TEST_TENANT_ID` | The `setup` project (ROPC token acquisition — dedicated test tenant)                                                                                   |
| `E2E_TEST_CLIENT_ID` | The `setup` project (dedicated test app registration's client ID — separate from the production SPA client ID)                                         |
| `E2E_TEST_API_SCOPE` | The `setup` project (API scope requested via ROPC)                                                                                                     |
| `E2E_TEST_USERNAME`  | The `setup` project (ROPC token acquisition)                                                                                                           |
| `E2E_TEST_PASSWORD`  | The `setup` project (ROPC token acquisition)                                                                                                           |
| `BASE_URL`           | Optional. Overrides the target origin (defaults to `http://localhost:4200`). Point this at a deployed Dev environment instead of the local dev server. |

The `setup` project authenticates via the ROPC (Resource Owner Password Credential) flow —
Microsoft's documented pattern for automating MSAL.js + Playwright testing
(learn.microsoft.com/entra/identity-platform/test-automate-integration-testing) — against a
**dedicated test tenant and app registration**, not the app's real production Entra config.
That test app registration must have "Allow public client flows" enabled, and the test user/app
must be excluded from any MFA-requiring Conditional Access policy in that tenant (ROPC cannot
satisfy an MFA challenge).

Any run that needs the authenticated storage state (the `chromium`, `firefox`, `webkit`
projects) fails fast with a clear error listing exactly which vars are missing. The
`chromium-unauthenticated` project (`auth-boundary.spec.ts`) does **not** depend on the
`setup` project and runs without these variables.

## Local setup

Real values are never committed. The committed config holds empty placeholders; each layer
gets its values injected:

**1. Angular dev server** — the E2E `webServer` boots the `playwright` serve configuration,
which file-replaces `environment.ts` with the gitignored `environment.playwright.ts` (so your
`environment.development.ts` stays on your normal dev tenant). Even the
`chromium-unauthenticated`-only run boots the webServer, so this file must exist:

```bash
cd apps/angular-web/apps/adventureworks-web/src/environments
cp environment.playwright.example.ts environment.playwright.ts
# fill in the test tenant authority, SPA client ID, and API scope
```

**2. API** — runs under `ASPNETCORE_ENVIRONMENT=PlaywrightTesting`, which binds JWT auth to the
`AzureAdPlaywrightTesting` config section (selected via `AzureAdSettings:CurrentSectionName`, so
the test tenant coexists with the dev tenant's `AzureAd:*` user secrets). Supply values via
`dotnet user-secrets` (locally) or `AzureAdPlaywrightTesting__*` /
`ConnectionStrings__PlaywrightTestingConnection` environment variables (CI):

```bash
cd apps/api-dotnet/src/AdventureWorks.API
dotnet user-secrets set "AzureAdPlaywrightTesting:Instance" "https://login.microsoftonline.com/"
dotnet user-secrets set "AzureAdPlaywrightTesting:TenantId" "<TEST_TENANT_ID>"
dotnet user-secrets set "AzureAdPlaywrightTesting:ClientId" "<TEST_API_CLIENT_ID>"
dotnet user-secrets set "AzureAdPlaywrightTesting:Audience" "<TEST_API_CLIENT_ID>"
dotnet user-secrets set "ConnectionStrings:PlaywrightTestingConnection" "<LOCAL_SQL_CONNECTION_STRING>"

ASPNETCORE_ENVIRONMENT=PlaywrightTesting dotnet run --no-launch-profile
# --no-launch-profile matches CI: Kestrel defaults to http://localhost:5000,
# which is what environment.playwright.example.ts points at.
```

**3. Playwright runner** — export the `E2E_TEST_*` vars from the table above in the shell that
runs the suite.

## Running locally

```bash
cd apps/angular-web

# Full suite (spins up the `playwright`-configured dev server automatically)
npx nx e2e adventureworks-web-e2e

# Only the tests that don't require a real Entra account
npx playwright test --project=chromium-unauthenticated --config=apps/adventureworks-web-e2e/playwright.config.ts

# Against a deployed environment instead of the local dev server
BASE_URL=https://adventureworks-dev.example.com npx nx e2e adventureworks-web-e2e
```

Nx caches this target. If nothing it considers an input changed (app source, test files), a
re-run replays the previous result instead of actually executing — you'll see `Nx read the
output from the cache instead of running the command` and the whole "run" finishes in
milliseconds. That's expected for a deterministic build, but e2e tests hit a live API/DB whose
state Nx can't see, so a cached "pass" doesn't guarantee the next real run would also pass. Force
a real run with:

```bash
npx nx e2e adventureworks-web-e2e --skip-nx-cache
```

## Viewing the HTML report

The command Playwright prints after a run (`npx playwright show-report
../../dist/.playwright/apps/adventureworks-web-e2e/playwright-report`) assumes you're standing in
`apps/adventureworks-web-e2e/` — it will fail with "No report found" if run from
`apps/angular-web` (where `nx e2e` is actually invoked from in the instructions above). From
`apps/angular-web`, use the path without `../../` instead:

```bash
npx playwright show-report dist/.playwright/apps/adventureworks-web-e2e/playwright-report
```

`dist/.playwright/...` is Nx's standard output convention (every project's build/test artifacts
land under the workspace's shared top-level `dist/`), not something specific to this project.

## Visual regression baselines

`visual-regression.spec.ts` uses `expect(page).toHaveScreenshot()`. Baselines are committed
under `test-snapshots/`. On the first run — or whenever a baseline needs to change
intentionally — regenerate them with:

```bash
npx nx e2e adventureworks-web-e2e -- --update-snapshots
```

Review the diff before committing regenerated baselines.

## Structure

- `src/page-objects/` — thin Playwright wrappers around existing selectors (`app-layout.page.ts`,
  `login-failed.page.ts`, `public-landing.page.ts`). They do not add new IDs to app markup.
- `src/support/global-setup.ts` — the Playwright `setup` project's auth flow. Acquires a token via
  ROPC against the dedicated test tenant/app registration (`@azure/msal-node`), injects the
  resulting MSAL token cache into the page's `localStorage` before navigating, verifies the
  authenticated app shell actually renders, and persists storage state to
  `playwright/.auth/user.json` (gitignored). Drives no UI login.
- `src/support/storage-state-path.ts` — shared constant for the storage-state file path,
  anchored on `workspaceRoot` (not `__dirname`, which the Nx Playwright plugin evaluates in a
  context where it is undefined).
- `src/app-shell.spec.ts` — load, navigation, and theme smoke tests (US-682).
- `src/auth-boundary.spec.ts` — unauthenticated boundary smoke tests (US-683); runs under the
  `chromium-unauthenticated` project only.
- `src/accessibility.spec.ts` — axe-core WCAG AA scans (US-684).
- `src/visual-regression.spec.ts` — light/dark theme screenshot comparisons (US-684).
