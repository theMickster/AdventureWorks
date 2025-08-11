#!/usr/bin/env bash
set -euo pipefail

# Replaces Angular placeholder tokens in built output with environment-specific values.
# Usage: ./replace-tokens.sh <angular-dist-dir>
# All environment variables must be set before running.

DIST_DIR="${1:?Usage: replace-tokens.sh <angular-dist-dir>}"

declare -A TOKENS=(
  ["__APP_INSIGHTS_CONNECTION_STRING__"]="${APP_INSIGHTS_CONNECTION_STRING:?}"
  ["__ENTRA_AUTHORITY__"]="${ENTRA_AUTHORITY:?}"
  ["__ENTRA_CLIENT_ID__"]="${ENTRA_CLIENT_ID:?}"
  ["__ENTRA_REDIRECT_URI__"]="${ENTRA_REDIRECT_URI:?}"
  ["__ENTRA_POST_LOGOUT_REDIRECT_URI__"]="${ENTRA_POST_LOGOUT_REDIRECT_URI:?}"
  ["__ENTRA_API_SCOPE__"]="${ENTRA_API_SCOPE:?}"
)

for token in "${!TOKENS[@]}"; do
  value="${TOKENS[$token]}"
  escaped_value=$(printf '%s' "$value" | sed 's/[&\|]/\\&/g')
  find "$DIST_DIR" -type f \( -name "*.js" -o -name "*.html" \) -exec sed -i "s|${token}|${escaped_value}|g" {} +
  echo "Replaced ${token}"
done

# Fail if any placeholder tokens remain in the output
remaining=$(grep -rl '__[A-Z_]\{2,\}__' "$DIST_DIR" --include="*.js" --include="*.html" || true)
if [ -n "$remaining" ]; then
  echo "ERROR: Unreplaced placeholder tokens found in:" >&2
  grep -rh '__[A-Z_]\{2,\}__' "$DIST_DIR" --include="*.js" --include="*.html" | sort -u >&2
  exit 1
fi

echo "Token replacement complete."
