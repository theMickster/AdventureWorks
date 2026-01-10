#!/usr/bin/env bash
# Builds the handler and starts the Functions host against it. Extra arguments are passed to `func start`
# (for example `--functions bom-cost health` to skip the Service Bus trigger when no emulator is running).
# Start the backing services first: docker compose -f local-dev/docker-compose.yml up -d
set -euo pipefail
cd "$(dirname "$0")/.."

if [[ ! -f local.settings.json ]]; then
  echo "local.settings.json is missing. Copy local.settings.json.example and fill in the values." >&2
  exit 1
fi

cargo build
ln -sf target/debug/handler handler
exec func start "$@"
