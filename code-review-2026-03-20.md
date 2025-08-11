# Code Review: Local Changes — 2026-03-20

**Date:** 2026-03-20 | **Reviewed by:** Claude Code (Opus 4.6)

## Summary

| Severity      | Count |
| ------------- | ----- |
| 🛑 Blocker    | 0     |
| ⚠️ Important  | 1     |
| ♻️ Refactor   | 0     |
| 💡 Suggestion | 2     |

Feature #644 (Environment & Secrets Management Strategy) introduces a Bicep app setting for Key Vault URI, a bash token replacement script, and two documentation files. The Bicep change is correct and uses cloud-portable `az.environment()` for the Key Vault DNS suffix. The documentation files are factually accurate with no secrets committed. The bash script has one important sed metacharacter issue and two minor suggestions.

## Findings

### ⚠️ Important

#### sed replacement corrupts output if env var values contain `&`

`infra/scripts/replace-tokens.sh:21`

  <details><summary>Details</summary>

  The `sed` substitution interpolates `${value}` directly into the replacement pattern:

  ```bash
  find "$DIST_DIR" -type f \( -name "*.js" -o -name "*.html" \) -exec sed -i "s|${token}|${value}|g" {} +
  ```

  In sed replacement strings, `&` means "the entire matched text." If any environment variable value contains `&` (e.g., a redirect URI with query parameters like `https://example.com/callback?foo=1&bar=2`), sed will insert the matched placeholder token text at each `&` position, silently producing a corrupted JS bundle.

  The `|` delimiter avoids `/` conflicts (good), but `&` and `\` remain dangerous. Current known values (GUIDs, simple URLs, semicolon-delimited connection strings) are safe today, but this is fragile.

  **Suggested fix** — escape sed metacharacters before substitution:

  ```bash
  escaped_value=$(printf '%s' "$value" | sed 's/[&\|]/\\&/g')
  find "$DIST_DIR" -type f \( -name "*.js" -o -name "*.html" \) -exec sed -i "s|${token}|${escaped_value}|g" {} +
  ```

  </details>

### 💡 Suggestions

#### `sed -i` without suffix argument is GNU-only (fails on macOS)

`infra/scripts/replace-tokens.sh:21`

  <details><summary>Details</summary>

  `sed -i "s|..."` without a backup suffix is GNU sed syntax. On macOS (BSD sed), `sed -i` requires a suffix argument, even if empty (`sed -i ''`). Since this script is designed for Linux CI/CD agents, this is unlikely to be hit in practice, but a developer testing locally on macOS would get a confusing error. Consider adding a comment documenting the Linux-only requirement, or using a portable alternative like `perl -pi -e`.

  </details>

#### Script reports success even if no tokens were actually replaced

`infra/scripts/replace-tokens.sh:19-23`

  <details><summary>Details</summary>

  The script prints `Replaced ${token}` unconditionally after each `sed` call, regardless of whether any file actually contained that token. If the Angular build output changes or a token name is misspelled in the environment config, the script will report success while deploying an app with broken `__PLACEHOLDER__` values. The project's `msalInstanceFactory` runtime guard catches `__ENTRA_CLIENT_ID__`, but the other 5 tokens would deploy silently broken.

  Consider adding a post-replacement grep check:

  ```bash
  remaining=$(grep -rl '__[A-Z_]\+__' "$DIST_DIR" --include="*.js" --include="*.html" || true)
  if [ -n "$remaining" ]; then
    echo "ERROR: Unreplaced tokens found" >&2
    exit 1
  fi
  ```

  </details>

## Reviewed and Dismissed

   <details><summary>🔍 2 initial findings dismissed after validation</summary>

   #### `declare -A` requires Bash 4+ (macOS ships Bash 3.2)
   `infra/scripts/replace-tokens.sh:10`
   **Original confidence:** 76/100
   **Dismissed because:** Same portability bucket as the `sed -i` finding — not a distinct issue. Both are consequences of the script being Linux-only, which is its intended execution environment (ADO pipeline agents).

   #### Git executable bit may not be preserved
   `infra/scripts/replace-tokens.sh:1`
   **Original confidence:** 80/100
   **Dismissed because:** `chmod +x` was already run on the file; the filesystem has the executable bit set, and git will track it when staged.

   </details>
