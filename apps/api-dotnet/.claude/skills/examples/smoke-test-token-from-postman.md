# Example: Getting a Dev Token from Postman

How to get a short-lived Bearer token from Postman in under 2 minutes.

---

## Steps

**1. Open the Sales collection**

In Postman, open the `Sales` collection (`aw-sales.postman_collection.json`).

**2. Go to the Authorization tab**

Click the `Sales` collection → `Authorization` tab (not a request's auth tab — the collection-level one).

**3. Request a new token**

Scroll down to `Current Token`. If there's no valid token, click **Get New Access Token**.

Postman will open an OAuth PKCE flow:

- It will open a browser (or embedded browser) pointing at `https://login.microsoftonline.com/.../oauth2/v2.0/authorize`
- Log in as `michael.scott.AdventureWorks@mickletofsky.com` (or your dev account)
- Postman will capture the auth code and exchange it for a token automatically

**4. Use the token**

After the flow completes, Postman shows the token in the **Manage Access Tokens** dialog. Click **Use Token**.

The token is now active in Postman AND visible in the `Current Token` → `Access Token` field.

**5. Copy the raw token value**

In the Authorization tab, you'll see:

```
Token Type: Bearer
Access Token: eyJ0eXAiOiJKV1Q...
```

Copy the value in `Access Token` (NOT the full `Bearer eyJ...` string — just the `eyJ...` part).

**6. Paste it to Claude**

Paste it in the chat as:

```
TOKEN: eyJ0eXAiOiJKV1Q...
```

---

## Token Lifetime

Entra-issued tokens for this app expire after **~80 minutes** (`exp` claim). Claude will validate the expiry before running tests and tell you if the token is stale.

Signs a token is expired:

- All curl requests return `401`
- The token's `exp` timestamp is in the past (Claude will print the expiry time when it validates)

To refresh: go back to Postman → Collection auth → Get New Access Token → repeat steps 3–5.

---

## Environment Note

The token you get from Postman is scoped to `api://26575822-ff7c-4cce-acf7-86212dfc3b3b` — the AdventureWorks API app registration. It works against:

- `https://localhost:44369` (local dev)
- `https://*.azurewebsites.net/` (Azure staging/prod, same tenant)

It does NOT work against other APIs (Bitwarden, etc.).
