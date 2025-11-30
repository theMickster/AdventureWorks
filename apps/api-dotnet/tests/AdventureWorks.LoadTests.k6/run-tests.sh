#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROFILE="${1:-}"

if ! command -v k6 >/dev/null 2>&1; then
  echo "k6 is not installed. Install from https://k6.io/docs/get-started/installation/."
  exit 1
fi

if [[ -z "${PROFILE}" ]]; then
  echo "Usage: ./run-tests.sh <smoke|load|stress>"
  exit 1
fi

case "${PROFILE}" in
  smoke|load|stress)
    ;;
  *)
    echo "Unsupported profile '${PROFILE}'. Use one of: smoke, load, stress."
    exit 1
    ;;
esac

mkdir -p "${SCRIPT_DIR}/results"

echo "Running '${PROFILE}' profile against ${BASE_URL:-https://localhost:44369}"
echo "K6_AUTH_TOKEN provided: $(if [[ -n "${K6_AUTH_TOKEN:-}" || -n "${AUTH_TOKEN:-}" ]]; then echo yes; else echo no; fi)"
echo "K6_COMPATIBILITY_MODE: ${K6_COMPATIBILITY_MODE:-extended}"

cd "${SCRIPT_DIR}"
k6 run --compatibility-mode "${K6_COMPATIBILITY_MODE:-extended}" "./profiles/${PROFILE}.ts"
