# Example: Smoke Test the Sales Lookup Endpoints

This example shows a complete run of `--group sales-lookups` against a locally running API. It demonstrates both the passing case and what a failure looks like.

---

## Invocation

```
smoke test the Sales lookup endpoints with my token

TOKEN: eyJ0eXAiOiJKV1QiL...
```

Or:

```
test the sales-lookups group — API is up, here's my token: eyJ...
```

---

## What Claude Does

1. Validates the token (checks `exp` field — refuses to run if expired)
2. Reads `references/endpoint-catalog/sales-lookups.md` for the 16 test cases
3. Writes a curl headers file to `/tmp/aw-smoke-headers-XXXX`
4. Runs 16 curl requests sequentially against `https://localhost:44369/api/v1`
5. Renders the result table
6. Cleans up the headers file

---

## Expected Output (All Passing)

```
Token valid — expires in 3831s (Thu May  7 12:50:48 2026)

=== AdventureWorks API Smoke Test — sales-lookups ===

STATUS  EXPECTED  PATH                                    NOTES
------  --------  ----                                    -----
✅ 200  200       sales-reasons                           array, 16 items, salesReasonId present
✅ 200  200       sales-reasons/1                         salesReasonId=1 ✓
✅ 404  404       sales-reasons/2147483647                not found ✓
✅ 400  400       sales-reasons/-1                        bad request ✓
✅ 200  200       currencies                              array, 105 items, currencyCode present
✅ 200  200       currencies/USD                          currencyCode=USD, name=US Dollar ✓
✅ 404  404       currencies/ZZZ                          not found ✓
✅ 400  400       currencies/%20                          bad request ✓
✅ 200  200       special-offers                          array, 16 items, isActive field present
✅ 200  200       special-offers/1                        isActive=false (expired: EndDate 2014-11-30) ✓
✅ 404  404       special-offers/2147483647               not found ✓
✅ 400  400       special-offers/-1                       bad request ✓
✅ 200  200       ship-methods                            array, 5 items, shipBase+shipRate present
✅ 200  200       ship-methods/1                          shipMethodId=1, shipBase+shipRate present ✓
✅ 404  404       ship-methods/2147483647                 not found ✓
✅ 400  400       ship-methods/-1                         bad request ✓

16 passed / 0 failed
```

---

## Example Failure Output

If `ship-methods/1` returned 500 instead of 200:

```
✅ 200  200       ship-methods                            array, 5 items, shipBase+shipRate present
❌ 500  200       ship-methods/1                          FAIL

--- FAILURE: ship-methods/1 ---
curl: /usr/bin/curl -sk -o /dev/null -w "%{http_code}" -H @/tmp/aw-smoke-headers-xyz https://localhost:44369/api/v1/ship-methods/1
response (500 chars): {"type":"https://tools.ietf.org/html/rfc7231#section-6.6.1","title":"An error occurred while processing your request.","status":500,"traceId":"00-abc123-def456-00"}

15 passed / 1 failed
```

In this case, Claude diagnoses before suggesting a fix:

- Is this endpoint actually deployed? (`git log --oneline -5`)
- Is the connection string pointing at a DB with ShipMethod data?
- Is there a query error visible in the API logs?

---

## Notes

- The `currencies/%20` test sends a URL-encoded space as the currency code. ASP.NET Core decodes it to `" "` and the controller's `IsNullOrWhiteSpace` check triggers the 400.
- `special-offers/1` will always return `isActive: false` in a stock AdventureWorks DB because all offer dates are from 2011–2014.
- The `--group all` run adds the `sales-existing` endpoints (SalesPerson list, Territory list, Store list) for 19 tests total.
