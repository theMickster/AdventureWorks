---
name: smoke-testing-dotnet-api
description: Runs live HTTP smoke tests against the AdventureWorks .NET API using a short-lived Bearer token. Executes curl against defined endpoint groups, renders a pass/fail result table, and prints curl debug output for any failures. Use when the user says "smoke test the API", "test the endpoints", "verify the API is up", "run smoke tests", "check these routes with my token", or pastes a Bearer token and asks you to verify endpoints.
---

# AdventureWorks API Smoke Test

A short-lived Bearer token is the only thing needed. Tests run via curl — no extra tooling, no external dependencies.

---

## Reference Files

Read these before executing anything:

| File                                            | Purpose                                                                                       |
| ----------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `references/smoke-test-runner.md`               | curl mechanics, SSL rules, header file pattern, sandbox PATH constraints, result table format |
| `references/endpoint-catalog/sales-lookups.md`  | All 16 sales-lookups test cases (SalesReason, Currency, SpecialOffer, ShipMethod)             |
| `references/endpoint-catalog/sales-existing.md` | Reachability stubs for older Sales endpoints (SalesPerson, Territory, Stores)                 |
| `examples/smoke-test-sales-lookups.md`          | Complete worked example — passing output, failure output, diagnostics                         |
| `examples/smoke-test-token-from-postman.md`     | Step-by-step: get a token from Postman in under 2 minutes                                     |

---

## Invocation

The user triggers this skill by:

1. A trigger phrase: "smoke test", "test endpoints", "verify the API is up", "run smoke tests", "check these routes"
2. A Bearer token, pasted as `TOKEN: eyJ...` or inline
3. Optional: `--group sales-lookups` / `--group sales-existing` / `--group all`

**No token provided?** Ask for one. Point to `examples/smoke-test-token-from-postman.md`. Do not attempt to acquire one yourself — `az account get-access-token` does not work for this app registration without admin consent.

**No group specified?** Default to `--group sales-lookups`. After a clean run, offer to run `--group all`.

---

## Execution

Follow `references/smoke-test-runner.md` exactly.

### Step 1 — Validate token

```bash
python3 -c "
import base64, json, time
token = '$TOKEN'
payload = token.split('.')[1]
payload += '=' * (4 - len(payload) % 4)
data = json.loads(base64.urlsafe_b64decode(payload))
exp = data['exp']
now = int(time.time())
remaining = exp - now
if remaining > 0:
    print(f'Token valid — {remaining}s remaining ({time.ctime(exp)})')
else:
    print(f'EXPIRED {-remaining}s ago')
    exit(1)
"
```

If expired: stop. Point to `examples/smoke-test-token-from-postman.md` step 4.

### Step 2 — Write headers file

```bash
HFILE=$(mktemp)
printf 'Authorization: Bearer %s\nAccept: application/json\n' "$TOKEN" > "$HFILE"
```

Never inline the token in `-H "..."` — long JWTs exceed shell argument limits and produce silent 401s.

### Step 3 — Run tests from catalog

Load entries from the catalog file(s) matching the requested group. For each entry:

```bash
STATUS=$(/usr/bin/curl -sk -o /dev/null -w "%{http_code}" -H @"$HFILE" "$BASE/$path")
```

For entries with field assertions (e.g. `isActive`, `shipBase`), capture the full body and parse with python3. See `references/smoke-test-runner.md` for the pattern.

### Step 4 — Render result table

```
=== AdventureWorks API Smoke Test — <group> ===

STATUS  EXPECTED  PATH                                    NOTES
------  --------  ----                                    -----
✅ 200  200       sales-reasons                           array, non-empty
✅ 200  200       sales-reasons/1                         salesReasonId=1 ✓
✅ 404  404       sales-reasons/2147483647                not found ✓
✅ 400  400       sales-reasons/-1                        bad request ✓
...

N passed / M failed
```

On any `❌`: print the curl command and first 500 chars of the response body immediately below the failing row.

---

## Supported Groups

| Group            | Catalog             | Test count   |
| ---------------- | ------------------- | ------------ |
| `sales-lookups`  | `sales-lookups.md`  | 16 requests  |
| `sales-existing` | `sales-existing.md` | 2 requests   |
| `all`            | Both                | ~18 requests |

To add a new group: create `references/endpoint-catalog/<name>.md` and update this table.
