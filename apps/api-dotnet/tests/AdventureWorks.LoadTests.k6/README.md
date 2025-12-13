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
```

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
