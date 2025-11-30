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

From `apps/api-dotnet/tests/load`:

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

## Reports

Each run writes reports to `tests/load/results/`:

- `<profile>-summary.html`
- `<profile>-summary.json`

The profile scripts and shared helpers are written in TypeScript (`.ts`) and run directly with k6 native TS support. `handleSummary` generates local HTML/JSON reports without external runtime script imports.
