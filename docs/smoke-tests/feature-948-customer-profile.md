# Smoke Test — Feature 948: Customer Profile with LTV Metrics

Manual end-to-end check for `GET /api/v1/customers/{id}` (US-954) and the `/sales/customers/:id` Angular page (US-955/956).

## Prerequisites

- API running locally at `https://localhost:44369` (`dotnet run` from `apps/api-dotnet/src/AdventureWorks.API`)
- Web running at `http://localhost:4200` (`npx nx serve adventureworks-web` from `apps/angular-web/`)
- A Bearer token. Grab one from Postman — see `apps/api-dotnet/.claude/skills/examples/smoke-test-token-from-postman.md`.

Set up the token in a header file rather than inlining it — long JWTs blow past shell argument limits and fail as silent 401s:

```bash
HFILE=$(mktemp)
printf 'Authorization: Bearer %s\nAccept: application/json\n' "$TOKEN" > "$HFILE"
API=https://localhost:44369/api/v1
```

## Test data

Verified against the live AdventureWorks database. `totalCustomerCount` is **19,820** for every customer. `avgOrderValue` = `totalSpend / orderCount`.

| Id      | Display name         | Type       | Rank   | Orders | Total spend  | Last order   | Inactive? | Covers                            |
| ------- | -------------------- | ---------- | ------ | ------ | ------------ | ------------ | --------- | --------------------------------- |
| `29486` | Riders Company       | Store      | 31     | 12     | `584949.1308` | 2014-05-01   | No        | Store happy path, store link      |
| `11091` | Dalton Perez         | Individual | 7,742  | 28     | `1314.2103`   | 2014-06-10   | No        | Individual happy path, plain name |
| `29562` | Golf and Cycle Store | Store      | 21     | 8      | `665292.3810` | 2013-04-30   | **Yes**   | Inactive badge                     |
| `1`     | A Bike Store         | Store      | 19,120 | 0      | `0`           | `null`       | **Yes**   | Zero-order empty state            |

A customer is inactive when its last order predates 2013-06-30 (the newest order in the database, 2014-06-30, minus 12 months) or it has no orders at all.

## Backend

All five cases are already in the Postman collection under **Sales → Customer Detail** (`apps/api-dotnet/postman/collections/aw-sales.postman_collection.json`) — run that folder instead of curl if you prefer.

| # | Request                                | Expect                                                                                             |
| - | -------------------------------------- | -------------------------------------------------------------------------------------------------- |
| 1 | `GET $API/customers/29486`             | `200`; `displayName: "Riders Company"`, `customerType: "Store"`, `storeId: 296`, `ltvRank: 31`, `totalCustomerCount: 19820`, `isInactive: false` |
| 2 | `GET $API/customers/11091`             | `200`; `displayName: "Dalton Perez"`, `customerType: "Individual"`, `storeId: null`, `firstName`/`lastName` populated, `ltvRank: 7742` |
| 3 | `GET $API/customers/9999999`           | `404` with a structured body — `error`, `correlationId`, `timestamp`                               |
| 4 | `GET $API/customers/0`                 | `400`                                                                                              |
| 5 | `GET $API/customers/29486` — no token  | `401`                                                                                              |

```bash
for id in 29486 11091 9999999 0; do
  printf '%s -> %s\n' "$id" "$(curl -sk -o /dev/null -w '%{http_code}' -H @"$HFILE" "$API/customers/$id")"
done
curl -sk -o /dev/null -w '401 check -> %{http_code}\n' "$API/customers/29486"
```

Spot-check one full body to confirm the field set and that `avgOrderValue` is present and non-zero:

```bash
curl -sk -H @"$HFILE" "$API/customers/29486" | python3 -m json.tool
```

## Frontend

Sign in at `http://localhost:4200` first.

1. **List → detail navigation.** Go to `/sales/customers`. Click any row. URL becomes `/sales/customers/:id` and the profile renders.
2. **Store customer.** Go to `/sales/customers/29486`. Header shows "Riders Company" as a **link**; rank line reads `#31 of 19,820`. Click the name — it navigates to `/sales/stores/296`.
3. **Individual customer.** Go to `/sales/customers/11091`. Header shows "Dalton Perez" as **plain text, not a link**; rank line reads `#7742 of 19,820` — only the denominator is thousands-separated, the rank itself is not.
4. **Metric tiles.** On both of the above, four tiles render left to right — Total Spend, Order Count, Avg Order Value, Last Order Date. For `29486`: `$584,949.13` / `12` / `$48,745.76` / `May 1, 2014`. For `11091`: `$1,314.21` / `28` / `$46.94` / `Jun 10, 2014`.
5. **Inactive badge present.** Go to `/sales/customers/29562`. A badge reading `inactive` (lowercase — there is no `inactive` key in `en.json`, so ngx-translate echoes the raw status) sits at the top right of the header card. Confirm it is **absent** on `29486` and `11091`.
6. **Zero-order customer.** Go to `/sales/customers/1`. Tiles read `$0.00` / `0` / `$0.00` / `—`, and a "No orders found" empty state renders below the tiles. The `inactive` badge is also present here.
7. **Not found.** Go to `/sales/customers/9999999`. Stays on the URL and renders the "Customer not found" empty state — it does **not** redirect.
8. **Invalid id redirects.** Go to `/sales/customers/abc`, then `/sales/customers/0`. Each redirects immediately back to `/sales/customers`.
9. **Back button.** The "Customers" button at the top left returns to `/sales/customers`.

Keep the browser console open throughout — it should stay clean apart from the expected `404` network entry in step 7.
