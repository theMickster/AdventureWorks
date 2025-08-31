# Smoke Test Runner Reference

## curl Command Construction

Always use `/usr/bin/curl` (full path) — `curl` may not be in PATH in the Claude Code sandbox.

### Headers file pattern

Do NOT inline the token directly in the curl command — long tokens exceed shell argument limits and cause silent 401s. Always write headers to a temp file:

```bash
HFILE=$(mktemp)
printf 'Authorization: Bearer %s\nAccept: application/json\n' "$TOKEN" > "$HFILE"
```

Use `-H @"$HFILE"` to pass headers from the file.

### SSL flag

| Base URL contains          | SSL flag                                                |
| -------------------------- | ------------------------------------------------------- |
| `localhost` or `127.0.0.1` | `-k` (skip cert verification — dev cert is self-signed) |
| Any other host             | _(no flag — use system trust store)_                    |

Never add `-k` for production or staging URLs.

### Status-only request (happy path check)

```bash
STATUS=$(/usr/bin/curl -sk -o /dev/null -w "%{http_code}" -H @"$HFILE" "$BASE/$path")
```

### Field assertion (inline python3)

Write body to a temp file for clean parsing:

```bash
BODY_FILE=$(mktemp)
/usr/bin/curl -sk -o "$BODY_FILE" -H @"$HFILE" "$BASE/$path"
python3 -c "
import json
data = json.load(open('$BODY_FILE'))
assert data.get('isActive') == False, f'isActive was {data.get(\"isActive\")}'
print('isActive=False ✅')
" 2>&1
```

For array responses:

```bash
python3 -c "
import json
items = json.load(open('$BODY_FILE'))
assert isinstance(items, list) and len(items) > 0, 'not a non-empty array'
for item in items:
    assert 'shipBase' in item and 'shipRate' in item, f'missing fields in {item}'
print(f'array OK — {len(items)} items, shipBase/shipRate present ✅')
" 2>&1
```

---

## Result Table Format

```
=== AdventureWorks API Smoke Test — <group> ===

STATUS  EXPECTED  PATH                                    NOTES
------  --------  ----                                    -----
✅ 200  200       sales-reasons                           array, non-empty
✅ 200  200       sales-reasons/1                         salesReasonId=1 ✓
✅ 404  404       sales-reasons/2147483647                not found ✓
✅ 400  400       sales-reasons/-1                        bad request ✓
❌ 500  200       currencies                              FAIL

N passed / M failed
```

- `✅` when actual status == expected status
- `❌` when they differ
- `NOTES`: key assertion verified (passing) or `FAIL` (failing)

### Failure detail block (printed immediately after the failing row)

```
--- FAILURE: currencies ---
curl: /usr/bin/curl -sk -o /dev/null -w "%{http_code}" -H @/tmp/... https://localhost:44369/api/v1/currencies
response (500 chars): {"type":"https://tools.ietf.org/html/rfc7231#section-6.6.1","title":"An error occurred..."}
```

---

## Token Validation

Always validate before running tests:

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
    print(f'Token valid — expires in {remaining}s ({time.ctime(exp)})')
else:
    print(f'Token EXPIRED {-remaining}s ago — get a new one from Postman')
    exit(1)
"
```

If expired: stop immediately and direct the user to `examples/smoke-test-token-from-postman.md`.

---

## Known Sandbox Constraints

| Command         | Status               | Workaround                                   |
| --------------- | -------------------- | -------------------------------------------- |
| `curl` (bare)   | Not in PATH          | Use `/usr/bin/curl`                          |
| `head` / `tail` | Not in PATH          | Use `python3` or read whole output           |
| `rm`            | Not in PATH          | Leave in `/tmp` — OS cleans it up            |
| `grep`          | Not in PATH          | Use `python3` string methods                 |
| `/usr/bin/curl` | ✅ Available (8.7.1) | Full path required                           |
| `python3`       | ✅ Available         | Use for all text processing and JSON parsing |
