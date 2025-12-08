import { PublicClientApplication } from '@azure/msal-node';
import { test as setup } from '@playwright/test';
import { STORAGE_STATE_PATH } from './storage-state-path';

/**
 * Playwright "setup" project (see playwright.config.ts `dependencies: ['setup']`). Acquires a
 * token via the ROPC (Resource Owner Password Credential) flow against a dedicated test tenant/
 * app registration — Microsoft's documented pattern for automating MSAL.js + Playwright testing
 * (learn.microsoft.com/entra/identity-platform/test-automate-integration-testing) — then injects
 * the resulting MSAL token cache directly into the page's `localStorage` before navigating, so
 * MSAL Browser picks it up as an already-signed-in session. No UI login is driven.
 *
 * Using a dependency-based setup project — rather than the config-level `globalSetup` hook —
 * means unauthenticated-only runs (e.g. `--project=chromium-unauthenticated`) never execute this
 * file and therefore never require the env vars below to be set.
 *
 * Requires `E2E_TEST_TENANT_ID`, `E2E_TEST_CLIENT_ID`, `E2E_TEST_API_SCOPE`,
 * `E2E_TEST_USERNAME`, `E2E_TEST_PASSWORD` in the environment — a dedicated test tenant/app
 * registration/test user is not provisioned yet (known, accepted gap). This fails fast with a
 * clear error listing exactly which vars are missing, rather than silently skipping or guessing
 * credentials.
 */
setup('authenticate', async ({ page, baseURL }) => {
  const requiredVars = [
    'E2E_TEST_TENANT_ID',
    'E2E_TEST_CLIENT_ID',
    'E2E_TEST_API_SCOPE',
    'E2E_TEST_USERNAME',
    'E2E_TEST_PASSWORD',
  ] as const;
  const missing = requiredVars.filter((name) => !process.env[name]);

  if (missing.length > 0) {
    throw new Error(
      `${missing.join(', ')} must be set to run authenticated Playwright specs (ROPC flow against ` +
        'the dedicated E2E test tenant/app registration). Set them in the environment before running ' +
        '`npx nx e2e adventureworks-web-e2e` (a real test tenant/app/user is not provisioned yet — see the e2e README).',
    );
  }

  const tenantId = process.env['E2E_TEST_TENANT_ID'] as string;
  const clientId = process.env['E2E_TEST_CLIENT_ID'] as string;
  const apiScope = process.env['E2E_TEST_API_SCOPE'] as string;
  const username = process.env['E2E_TEST_USERNAME'] as string;
  const password = process.env['E2E_TEST_PASSWORD'] as string;

  const pca = new PublicClientApplication({
    auth: {
      clientId,
      authority: `https://login.microsoftonline.com/${tenantId}`,
    },
  });

  // acquireTokenByUsernamePassword is marked @deprecated in @azure/msal-node — Microsoft
  // discourages the ROPC flow in general (no MFA support) and warns it may be removed in a
  // future release, but there is no replacement API for ROPC in this library. This is the exact
  // method Microsoft's own testing guidance documents for MSAL.js + Playwright automation
  // (learn.microsoft.com/entra/identity-platform/test-automate-integration-testing), used here
  // only against a dedicated test tenant/app registration with MFA excluded via Conditional
  // Access — never against the app's real production Entra config.
  await pca.acquireTokenByUsernamePassword({
    scopes: [apiScope],
    username,
    password,
  });

  // MSAL Node's unified cache schema is the same one MSAL Browser reads from localStorage —
  // inject each raw cache entry, plus the account/token "index" keys MSAL Browser uses to
  // enumerate the cache (getAllAccounts, acquireTokenSilent), before the app's own script runs.
  const cacheEntries = Object.entries(pca.getTokenCache().getKVStore());
  const accountKeys: string[] = [];
  const idTokenKeys: string[] = [];
  const accessTokenKeys: string[] = [];
  const refreshTokenKeys: string[] = [];

  for (const [key] of cacheEntries) {
    const lowerKey = key.toLowerCase();
    if (lowerKey.includes('-accesstoken-')) {
      accessTokenKeys.push(key);
    } else if (lowerKey.includes('-idtoken-')) {
      idTokenKeys.push(key);
    } else if (lowerKey.includes('-refreshtoken-')) {
      refreshTokenKeys.push(key);
    } else if (!lowerKey.startsWith('appmetadata-')) {
      accountKeys.push(key);
    }
  }

  const localStorageEntries: Record<string, string> = {};
  for (const [key, value] of cacheEntries) {
    localStorageEntries[key] = JSON.stringify(value);
  }
  localStorageEntries['msal.account.keys'] = JSON.stringify(accountKeys);
  localStorageEntries[`msal.token.keys.${clientId}`] = JSON.stringify({
    idToken: idTokenKeys,
    accessToken: accessTokenKeys,
    refreshToken: refreshTokenKeys,
  });

  await page.context().addInitScript((entries: Record<string, string>) => {
    for (const [key, value] of Object.entries(entries)) {
      window.localStorage.setItem(key, value);
    }
  }, localStorageEntries);

  await page.goto(baseURL ?? 'http://localhost:4200');
  await page.goto('/dashboard');
  await page.waitForURL(/\/dashboard$/, { timeout: 30_000 });
  // Real verification that MSAL Browser accepted the injected cache — not just an assumption.
  await page.locator('#aw-shell').waitFor({ state: 'visible', timeout: 30_000 });

  await page.context().storageState({ path: STORAGE_STATE_PATH });
});
