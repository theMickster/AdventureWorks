# Postman Guide — AdventureWorks API

How Postman is organized for this API and the working agreement that keeps edits from corrupting it. **Read before touching anything under `apps/api-dotnet/postman/`.**

## TL;DR

- Files live under `apps/api-dotnet/postman/`.
- Five feature-aligned collections (`aw-*.postman_collection.json`) mirror `Application/Features/`.
- `regression-tests.postman_collection.json` is **parked**.
- Real env files **gitignored**; `.example` templates committed.
- Auth: collection-level OAuth 2.0 (Authorization Code + PKCE). Credentials live in env vars, **never** at collection scope.
- Edits follow the **anti-poop pledge**: diff-first, UID preservation, locality, one enhancement at a time, user-driven verification.

---

## Layout

```
apps/api-dotnet/postman/
├── collections/
│   ├── aw-address-management.postman_collection.json
│   ├── aw-human-resources.postman_collection.json
│   ├── aw-person.postman_collection.json
│   ├── aw-platform.postman_collection.json
│   ├── aw-sales.postman_collection.json
│   └── regression-tests.postman_collection.json   # PARKED
└── environments/
    ├── *.example.postman_environment.json         # committed; placeholders
    └── *.postman_environment.json                 # GITIGNORED; real values
```

Filenames: kebab-case. Display names (`info.name`) are human-readable Feature labels.

---

## Collections — split by Application Feature

| Source folder(s) | Target collection |
|---|---|
| `AKV` | `aw-platform`* |
| `Address` | `aw-address-management` |
| `Address Type`, `Contact Type`, `Country Region`, `Person Type`, `Phone Type`, `State Province` | `aw-person` |
| `Departments`, `Employee`, `Shifts` | `aw-human-resources` |
| `Sales Person`, `Sales Territory`, `Stores` | `aw-sales` |

\* `aw-platform` covers cross-cutting/infra controllers (AKV today; future health checks, diagnostics) — no Application Feature counterpart.

Add new collections (`aw-production`, `aw-product-review`) only when the first request is going into them. Empty shells are noise.

---

## Environments

Three envs: `local`, `azure-development`, `azure-production`. Each holds:

| Key | Type | Notes |
|---|---|---|
| `adventureWorksApi`, `apiVersion`, `authenticationToken` | default | Base URL, version, empty token fallback |
| `clientId`, `tenantId`, `scope`, `callbackUrl` | default | Azure AD app registration values |
| `clientSecret` | **secret** | Stored in Postman's encrypted vault — never plaintext on disk |

New dev: copy each `.example` to its non-`.example` filename, import into Postman, paste real values via Postman's environment editor.

---

## Authentication

JWT Bearer on the API ([`RegisterServices.cs`](../../src/AdventureWorks.API/libs/RegisterServices.cs)), Microsoft Identity Web. OAuth 2.0 Authorization Code + PKCE configured at collection level — request-level auth blocks should be left alone.

| OAuth field | Value |
|---|---|
| Auth URL / Token URL | `https://login.microsoftonline.com/{{tenantId}}/oauth2/v2.0/{authorize,token}` |
| Client ID / Secret / Scope / Callback | `{{clientId}}` / `{{clientSecret}}` / `{{scope}}` / `{{callbackUrl}}` |
| PKCE | `S256` |
| Token Name | `AdventureWorks` |

Get a token: any request → **Authorization** tab → **Get New Access Token**.

---

## Variable scoping

| Scope | Contents |
|---|---|
| **Environment** | All auth credentials + base URL + apiVersion |
| **Collection** | Runtime helpers (`currentDateTime`, `currentDate`, `randomString`); per-flow state (`lastCreatedEmployeeId`, etc.) — committed empty, repopulated at runtime |
| **Globals** | **Nothing.** Do not introduce. |

**Secrets MUST NEVER be at collection scope.** Collection JSONs are committed; collection-scope vars leak instantly. The original Phase-0 sanitization rotated and removed an Azure AD `clientSecret` that had been there — do not recreate that mistake.

---

## The working agreement (anti-poop pledge)

1. **Diff-first.** Unified diff before the user re-imports. No "trust me."
2. **UID preservation.** `info._postman_id` and any `id` / `_postman_id` fields untouched.
3. **Locality.** Only requests/folders explicitly named in the task are modified.
4. **One enhancement at a time.** No opportunistic refactors.
5. **User-driven verification.** User imports + smoke-tests before we move on.
6. **Re-export round-trip check.** After a Postman-side edit, user exports and we diff.

---

## Adding new tests — workflow

1. Map the endpoint to its Feature collection (Stores → `aw-sales`).
2. Read the collection JSON, locate the request item by name and folder path.
3. Add `pm.test()` blocks inside the request's `event[]` test script (`"listen": "test"`). Don't modify request body, headers, URL, or auth unless asked.
4. Show the unified diff with a one-line summary.
5. User imports the affected collection, smoke-tests.
6. Commit only when the user says so.

When the user adds a new API endpoint via [`adding-features.md`](adding-features.md), Postman coverage is a **separate, follow-up** change — never wedged in.

---

## Common script patterns

```javascript
// Status + JSON shape
pm.test("Status is 200 OK", () => pm.response.to.have.status(200));
const body = pm.response.json();
pm.test("Response has id", () => pm.expect(body.id).to.be.a("number"));

// Correlation ID is mandatory on every API response
pm.test("Has X-Correlation-Id header", () =>
    pm.response.to.have.header("X-Correlation-Id")
);

// Store an ID for downstream requests in the same flow
pm.collectionVariables.set("lastCreatedEmployeeId", body.id);
```

`{{currentDateTime}}` and `{{currentDate}}` are pre-populated by the collection-level prerequest script and available in any request.

---

## What NOT to do

- Put `clientSecret` (or any secret) at collection scope.
- Add new globals (`pm.globals.set`).
- Reorder, rename, or merge folders.
- Modify `regression-tests.postman_collection.json` (parked).
- Modify or delete `development-tests.postman_collection.json` (user-controlled retirement).
- Bypass the diff-review step or auto-commit.
- Introduce Postman Flows (no Newman/CI integration; orchestration, not testing).
- Introduce collection-level type definitions (git-incompatible).
- Use Postman's built-in git integration — local JSON is the source of truth.

---

## Verification commands

From `apps/api-dotnet/postman/collections/`:

```bash
# Top-level folders
jq -r '[.item[].name]' aw-sales.postman_collection.json

# Leaf request count (excludes example responses)
jq '[recurse(.item // empty | .[]) | select(has("request"))] | length' aw-sales.postman_collection.json

# Secret-hygiene scan — only {{clientSecret}} template refs should match
grep -rn -E 'clientSecret"[[:space:]]*:[[:space:]]*"[^{]' apps/api-dotnet/postman/

# What git would stage (no real env files should appear)
git add --dry-run apps/api-dotnet/postman/ | sort
```

---

## Related files

- [`apps/api-dotnet/.claude/CLAUDE.md`](../CLAUDE.md) — entry point
- [`adding-features.md`](adding-features.md) — .NET endpoint flow (Postman coverage is a separate change)
- [`testing-guide.md`](testing-guide.md) — xUnit templates
- [`apps/api-dotnet/src/AdventureWorks.Application/Features/`](../../src/AdventureWorks.Application/Features/) — drives the collection split
