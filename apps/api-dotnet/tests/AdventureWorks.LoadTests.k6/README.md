# API Load Tests (k6 + TypeScript)

## Prerequisites

- Install [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/).
- Run the API locally (recommended profile: `AdventureWorks.API.LoadTesting`).

### Install k6 on macOS / Windows

- **macOS (Homebrew):**

  ```bash
  brew install k6
  ```

  - Homebrew: https://brew.sh/
  - k6 install docs: https://grafana.com/docs/k6/latest/set-up/install-k6/

- **Windows (winget - official manifest):**

  ```powershell
  winget install k6 --source winget
  ```

  - winget: https://github.com/microsoft/winget-cli
  - k6 install docs: https://grafana.com/docs/k6/latest/set-up/install-k6/

- **Windows (Chocolatey - unofficial package):**

  ```powershell
  choco install k6
  ```

  - Chocolatey: https://chocolatey.org/

- **Windows (official MSI installer):**
  - https://dl.k6.io/msi/k6-latest-amd64.msi

## Quick Start

From `apps/api-dotnet/tests/AdventureWorks.LoadTests.k6`:

```bash
./run-tests.sh smoke
./run-tests.sh load
./run-tests.sh stress
./run-tests.sh smoke-human-resources
./run-tests.sh smoke-person
./run-tests.sh smoke-production
./run-tests.sh smoke-product-review
./run-tests.sh smoke-sales-order
./run-tests.sh smoke-sales-person
./run-tests.sh smoke-store
```

`smoke-human-resources` exercises the 5 HumanResources endpoints (employees, departments, shifts) with a p95 < 300ms threshold; every endpoint requires auth, so this profile always needs `LOADTEST_*` credentials or a pre-set `K6_AUTH_TOKEN`/`AUTH_TOKEN` — there is no unauthenticated fallback.

`smoke-person` exercises the Person endpoints (plus two anonymous Country/State reads) with a p95 < 300ms threshold, including a negative-path check that a nonexistent Person id returns 404. The Person list/by-id checks require auth, so `run-tests.sh` requires `LOADTEST_*` credentials for this profile, same as `smoke-human-resources` — there's no partial-anonymous run through the script. The Country/State reads don't need a token themselves; if you invoke the profile directly with `k6 run` (bypassing `run-tests.sh`) without credentials, they still execute and the Person checks are skipped with a warning.

`smoke-production` exercises `GET /api/v1/products`, `GET /api/v1/products/{id}`, and `GET /api/v1/product-models` with a p95 < 300ms threshold, plus a negative-path check that a nonexistent product id (`999999999`) returns 404 — Product's route id parameter is `int`-typed, so (unlike Departments' `short`) this is a clean 404 with no binding-width quirk. `GET /api/v1/products/categories` is `[AllowAnonymous]` and runs first, unconditionally. The remaining checks require auth, so `run-tests.sh` requires `LOADTEST_*` credentials for this profile, same as `smoke-human-resources`.

`smoke-product-review` exercises `GET /api/v1/products/{productId}/reviews` and `GET /api/v1/products/{productId}/reviews/statistics` for product id `937` (confirmed via the local AdventureWorks DB to have 2 reviews, the most of any product) with a p95 < 300ms threshold. Both endpoints require auth, so `run-tests.sh` requires `LOADTEST_*` credentials for this profile, same as `smoke-human-resources`. If you invoke the profile directly with `k6 run` (bypassing `run-tests.sh`) without a token, it takes the negative path instead: both requests are sent without a bearer token and assert `401` explicitly, satisfying the "unset/invalid token" acceptance case without ever surfacing as an unhandled script error.

`smoke-sales-order` exercises `GET /api/v1/sales-orders`, `GET /api/v1/sales-orders/{id}` (id `43659`/SO43659, confirmed via the local AdventureWorks DB), `GET /api/v1/sales/dashboard`, `GET /api/v1/sales-reasons`, `GET /api/v1/territories`, `GET /api/v1/ship-methods`, and `GET /api/v1/special-offers` with a p95 < 300ms threshold, plus a negative-path check that a nonexistent sales order id (`999999999`) returns 404. The ADO acceptance criteria text for this story says "sales-territories", but there is no such route — the real controller (`ReadSalesTerritoryController`) exposes `GET /api/v1/territories`, which is what the script calls. Every endpoint requires auth, so this profile always needs `LOADTEST_*` credentials or a pre-set `K6_AUTH_TOKEN`/`AUTH_TOKEN` — there is no unauthenticated fallback.

`smoke-sales-person` exercises `GET /api/v1/salespersons`, `GET /api/v1/salespersons/{id}` (id `275`, Michael Blythe, confirmed via the local AdventureWorks DB), and `GET /api/v1/salespersons/{id}/performance` with a p95 < 300ms threshold, plus a negative-path check that a nonexistent sales person id (`999999999`) returns 404. Every endpoint requires auth, so this profile always needs `LOADTEST_*` credentials or a pre-set `K6_AUTH_TOKEN`/`AUTH_TOKEN` — there is no unauthenticated fallback.

`smoke-store` exercises `GET /api/v1/stores`, `GET /api/v1/stores/{id}` (id `292`, Next-Door Bike Store, confirmed via the local AdventureWorks DB), `POST /api/v1/stores/search`, `GET /api/v1/stores/{id}/addresses`, and `GET /api/v1/stores/{id}/contacts` with a p95 < 300ms threshold, plus a negative-path check that a nonexistent store id (`999999999`) returns 404. That 404 check targets `GET /api/v1/stores/{id}` specifically — `ReadStoreAddressController` and `ReadStoreContactController` don't verify the parent store exists (they return `200` with an empty list for any positive id), so a 404 assertion against those endpoints would fail. Every endpoint requires auth, so this profile always needs `LOADTEST_*` credentials or a pre-set `K6_AUTH_TOKEN`/`AUTH_TOKEN` — there is no unauthenticated fallback.

## Environment Variables

- `BASE_URL` (optional): defaults to `https://localhost:44369`
- `K6_AUTH_TOKEN` (preferred for protected endpoints)
- `AUTH_TOKEN` (fallback alias if `K6_AUTH_TOKEN` is not set)
- `K6_INSECURE_SKIP_TLS_VERIFY` (optional): defaults to `true` for local HTTPS certs
- `K6_COMPATIBILITY_MODE` (optional): defaults to `extended` (`base` also supported)

Example with auth token:

```bash
K6_AUTH_TOKEN="<jwt-token>" BASE_URL="https://localhost:44369" ./run-tests.sh load
```

### Automatic token acquisition (LOADTEST_\*)

`run-tests.sh` can acquire `K6_AUTH_TOKEN` automatically via MSAL Node's ROPC (Resource Owner
Password Credential) flow, so you no longer have to paste a JWT by hand before running `load` or
`stress`. Set these five variables:

- `LOADTEST_TENANT_ID`
- `LOADTEST_CLIENT_ID`
- `LOADTEST_API_SCOPE`
- `LOADTEST_USERNAME`
- `LOADTEST_PASSWORD`

**`load` and `stress` profiles**: all five vars are required. If any is missing, the script exits
non-zero before k6 starts and names exactly which variable(s) are missing.

**`smoke` profile**: these vars are optional. If none are set, `smoke` runs unauthenticated
exactly as it always has (health/version checks only) — zero-config behavior is preserved. If you
set even one of the five vars, the script treats it as an authenticated smoke run and applies the
same fail-fast validation as `load`/`stress`.

```bash
LOADTEST_TENANT_ID="<tenant-id>" \
LOADTEST_CLIENT_ID="<client-id>" \
LOADTEST_API_SCOPE="api://<api-client-id>/.default" \
LOADTEST_USERNAME="<test-user>@<tenant>.onmicrosoft.com" \
LOADTEST_PASSWORD="<test-user-password>" \
./run-tests.sh load
```

The first time an authenticated profile actually runs, `run-tests.sh` installs the token script's
dependencies (`@azure/msal-node`, `tsx`) into a local `node_modules/` automatically via
`npm install`. A pure unauthenticated `smoke` run never triggers this install.

**Security caveat**: `acquireTokenByUsernamePassword` implements the ROPC flow, which Microsoft
documents specifically for test-automation scenarios
(learn.microsoft.com/entra/identity-platform/test-automate-integration-testing) but discourages
for general use — it has no MFA support and is marked `@deprecated` in `@azure/msal-node` with no
replacement API. `LOADTEST_*` credentials must point at a dedicated test tenant and app
registration with MFA excluded via Conditional Access — never at production Entra ID
configuration.

## Reports

Each run writes reports to `tests/AdventureWorks.LoadTests.k6/results/`:

- `<profile>-summary.html`
- `<profile>-summary.json`

The profile scripts and shared helpers are written in TypeScript (`.ts`) and run directly with k6 native TS support. `handleSummary` generates local HTML/JSON reports without external runtime script imports.
