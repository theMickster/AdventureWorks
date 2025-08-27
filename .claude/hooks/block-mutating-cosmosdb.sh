#!/bin/bash
set -euo pipefail

# PreToolUse hook: blocks mutating CosmosDB operations in az commands
# that reference COSMOS_ environment variables. Only fires on commands that
# contain BOTH "az" and "COSMOS_" — all other Bash commands pass through
# untouched.
#
# Three check layers:
#   1. Destructive az cosmosdb subcommands (create, delete, update, etc.)
#   2. REST API bypass via curl/wget using CosmosDB env vars
#   3. SDK access via CosmosDB client library imports

input=$(cat)
command=$(echo "$input" | jq -r '.tool_input.command // empty')

# Pass through anything that isn't an az command using CosmosDB env vars
if [[ ! "$command" =~ az ]] || [[ ! "$command" =~ COSMOS_ ]]; then
  exit 0
fi

# Normalize to uppercase for case-insensitive matching
upper_command=$(echo "$command" | tr '[:lower:]' '[:upper:]')

# --- Layer 1: Destructive az cosmosdb subcommands ---
# These modify CosmosDB resources. The skill only needs read operations:
# az cosmosdb sql query, container list, container show, container throughput show.
az_deny_patterns=(
  'CONTAINER[[:space:]]+DELETE'
  'CONTAINER[[:space:]]+CREATE'
  'CONTAINER[[:space:]]+UPDATE'
  'DATABASE[[:space:]]+DELETE'
  'DATABASE[[:space:]]+CREATE'
  'DATABASE[[:space:]]+UPDATE'
  'STORED-PROCEDURE'
  'TRIGGER'
  'USER-DEFINED-FUNCTION'
  'KEYS[[:space:]]+REGENERATE'
  'THROUGHPUT[[:space:]]+UPDATE'
  'THROUGHPUT[[:space:]]+MIGRATE'
  'ROLE[[:space:]]'
  'FAILOVER-PRIORITY-CHANGE'
)

for pattern in "${az_deny_patterns[@]}"; do
  if echo "$upper_command" | grep -qE "$pattern"; then
    cat <<HOOKEOF
{
  "hookSpecificOutput": {
    "permissionDecision": "deny"
  },
  "systemMessage": "BLOCKED by read-only hook: Destructive CosmosDB operation detected (pattern: $pattern). The CosmosDB reader skill only permits read operations: query, container list, container show, and throughput show."
}
HOOKEOF
    exit 0
  fi
done

# --- Layer 2: REST API bypass ---
# Block direct HTTP calls that use CosmosDB env vars to bypass the CLI.
if echo "$upper_command" | grep -qE '(CURL|WGET|INVOKE-WEBREQUEST|INVOKE-RESTMETHOD).*COSMOS_'; then
  cat <<HOOKEOF
{
  "hookSpecificOutput": {
    "permissionDecision": "deny"
  },
  "systemMessage": "BLOCKED by read-only hook: Direct HTTP request using CosmosDB credentials detected. Use az cosmosdb sql query for all database access — direct REST API calls are not permitted."
}
HOOKEOF
  exit 0
fi

if echo "$upper_command" | grep -qE 'COSMOS_.*(CURL|WGET|INVOKE-WEBREQUEST|INVOKE-RESTMETHOD)'; then
  cat <<HOOKEOF
{
  "hookSpecificOutput": {
    "permissionDecision": "deny"
  },
  "systemMessage": "BLOCKED by read-only hook: Direct HTTP request using CosmosDB credentials detected. Use az cosmosdb sql query for all database access — direct REST API calls are not permitted."
}
HOOKEOF
  exit 0
fi

# --- Layer 3: SDK access ---
# Block CosmosDB client library imports/usage that bypass the CLI.
sdk_deny_patterns=(
  'COSMOSCLIENT'
  'FROM[[:space:]]+AZURE\.COSMOS'
  '@AZURE/COSMOS'
  'MICROSOFT\.AZURE\.COSMOS'
)

for pattern in "${sdk_deny_patterns[@]}"; do
  if echo "$upper_command" | grep -qE "$pattern"; then
    cat <<HOOKEOF
{
  "hookSpecificOutput": {
    "permissionDecision": "deny"
  },
  "systemMessage": "BLOCKED by read-only hook: CosmosDB SDK usage detected (pattern: $pattern). The CosmosDB reader skill only permits access via az cosmosdb sql query — SDK client libraries are not permitted."
}
HOOKEOF
    exit 0
  fi
done

exit 0
