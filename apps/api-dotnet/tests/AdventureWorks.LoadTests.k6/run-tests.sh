#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROFILE="${1:-}"

if ! command -v k6 >/dev/null 2>&1; then
  echo "k6 is not installed. Install from https://k6.io/docs/get-started/installation/."
  exit 1
fi

if [[ -z "${PROFILE}" ]]; then
  echo "Usage: ./run-tests.sh <smoke|load|stress|smoke-human-resources|smoke-person|smoke-production|smoke-product-review|smoke-sales-order|smoke-sales-person|smoke-store|smoke-work-order>"
  exit 1
fi

case "${PROFILE}" in
  smoke|load|stress|smoke-human-resources|smoke-person|smoke-production|smoke-product-review|smoke-sales-order|smoke-sales-person|smoke-store|smoke-work-order)
    ;;
  *)
    echo "Unsupported profile '${PROFILE}'. Use one of: smoke, load, stress, smoke-human-resources, smoke-person, smoke-production, smoke-product-review, smoke-sales-order, smoke-sales-person, smoke-store, smoke-work-order."
    exit 1
    ;;
esac

REQUIRED_LOADTEST_VARS=(LOADTEST_TENANT_ID LOADTEST_CLIENT_ID LOADTEST_API_SCOPE LOADTEST_USERNAME LOADTEST_PASSWORD)
missing_vars=()
present_vars=()
for var in "${REQUIRED_LOADTEST_VARS[@]}"; do
  if [[ -z "${!var:-}" ]]; then
    missing_vars+=("${var}")
  else
    present_vars+=("${var}")
  fi
done

if [[ "${PROFILE}" != "smoke" || ${#present_vars[@]} -gt 0 ]]; then
  if [[ ${#missing_vars[@]} -gt 0 ]]; then
    echo "Missing required environment variable(s) for token acquisition: ${missing_vars[*]}" >&2
    exit 1
  fi

  if [[ ! -d "${SCRIPT_DIR}/node_modules" ]]; then
    echo "Installing k6 script dependencies (@azure/msal-node, tsx)..."
    (cd "${SCRIPT_DIR}" && npm ci)
  fi

  echo "Acquiring Entra access token via MSAL (LOADTEST_* credentials)..."
  if ! K6_AUTH_TOKEN="$(cd "${SCRIPT_DIR}" && npx tsx get-token.ts)"; then
    echo "Token acquisition failed; see MSAL error above." >&2
    exit 1
  fi
  export K6_AUTH_TOKEN
else
  echo "No LOADTEST_* credentials set; running smoke without auth (health/version checks only)."
fi

mkdir -p "${SCRIPT_DIR}/results"

echo "Running '${PROFILE}' profile against ${BASE_URL:-https://localhost:44369}"
echo "K6_AUTH_TOKEN provided: $(if [[ -n "${K6_AUTH_TOKEN:-}" || -n "${AUTH_TOKEN:-}" ]]; then echo yes; else echo no; fi)"
echo "K6_COMPATIBILITY_MODE: ${K6_COMPATIBILITY_MODE:-extended}"

cd "${SCRIPT_DIR}"
k6 run --compatibility-mode "${K6_COMPATIBILITY_MODE:-extended}" "./profiles/${PROFILE}.ts"
