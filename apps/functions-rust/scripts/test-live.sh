#!/usr/bin/env bash
# Runs the ignored live integration tests against the local SQL Server, Redis and the real Cosmos container
# named by Cosmos__Container (bom-cost-results-local). Needs `az login`; the Cosmos endpoint and database come from Key Vault.
# Loads local.settings.json into the environment first. Start the backing services beforehand:
#   docker compose -f local-dev/docker-compose.yml up -d
# The RabbitMQ messaging tests (tests/rabbitmq_flow.rs) run too when RabbitMQ is reachable on its host port
# (compose profile rabbitmq: `docker compose -f local-dev/docker-compose.yml --profile rabbitmq up -d`).
set -euo pipefail
cd "$(dirname "$0")/.."

if [[ ! -f local.settings.json ]]; then
  echo "local.settings.json is missing. Copy local.settings.json.example and fill in the values." >&2
  exit 1
fi

eval "$(python3 -c '
import json, shlex
for key, value in json.load(open("local.settings.json"))["Values"].items():
    print(f"export {key}={shlex.quote(value)}")
')"

cargo test --test live -- --ignored --nocapture "$@"

if (exec 3<>/dev/tcp/127.0.0.1/5675) 2>/dev/null; then
  cargo test --test rabbitmq_flow -- --ignored --nocapture "$@"
else
  echo "RabbitMQ is not listening on 127.0.0.1:5675; skipping tests/rabbitmq_flow.rs (start the rabbitmq compose profile)." >&2
fi
