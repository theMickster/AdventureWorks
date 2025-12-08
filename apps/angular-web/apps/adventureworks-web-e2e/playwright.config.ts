import { defineConfig, devices } from '@playwright/test';
import { nxE2EPreset } from '@nx/playwright/preset';
import { workspaceRoot } from '@nx/devkit';
import { STORAGE_STATE_PATH } from './src/support/storage-state-path';

// For CI, you may want to set BASE_URL to the deployed application.
const baseURL = process.env['BASE_URL'] || 'http://localhost:4200';

/**
 * Read environment variables from file.
 * https://github.com/motdotla/dotenv
 */
// require('dotenv').config();

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  ...nxE2EPreset(__filename, { testDir: './src' }),
  /* Visual regression baselines are committed under test-snapshots/ (US-684). */
  snapshotDir: './test-snapshots',
  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  use: {
    baseURL,
    /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
    trace: 'on-first-retry',
  },
  /* Run your local dev server before starting the tests. The `playwright` serve configuration
   * file-replaces environment.ts with the gitignored environment.playwright.ts (test tenant),
   * leaving environment.development.ts untouched for normal dev runs. */
  webServer: {
    command: 'npx nx run adventureworks-web:serve:playwright',
    url: 'http://localhost:4200',
    reuseExistingServer: true,
    cwd: workspaceRoot,
  },
  projects: [
    // Real Entra login runs once here and persists storageState for the authenticated
    // projects below (`dependencies: ['setup']`). Selecting only `chromium-unauthenticated`
    // (e.g. `--project=chromium-unauthenticated`) never triggers this project, so
    // unauthenticated-only runs do not require E2E_TEST_USERNAME/E2E_TEST_PASSWORD.
    // trace: 'off' overrides the global 'on-first-retry' default. This project is where real
    // Entra tokens are actually minted (global-setup.ts performs ROPC token acquisition and
    // injects the MSAL token cache into localStorage) — a captured trace on a CI retry would
    // embed the live credentials/tokens into a published pipeline artifact.
    {
      name: 'setup',
      testMatch: /global-setup\.ts/,
      use: { trace: 'off' },
    },
    {
      name: 'chromium-unauthenticated',
      use: { ...devices['Desktop Chrome'] },
      testMatch: /auth-boundary\.spec\.ts/,
    },
    // trace: 'off' overrides the global 'on-first-retry' default — these projects run with a
    // real Entra storageState, and a captured trace would embed live access/ID/refresh tokens
    // (network Authorization headers, localStorage snapshot) into a CI artifact.
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'], storageState: STORAGE_STATE_PATH, trace: 'off' },
      testIgnore: [/auth-boundary\.spec\.ts/, /global-setup\.ts/],
      dependencies: ['setup'],
    },

    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'], storageState: STORAGE_STATE_PATH, trace: 'off' },
      testIgnore: [/auth-boundary\.spec\.ts/, /global-setup\.ts/],
      dependencies: ['setup'],
    },

    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'], storageState: STORAGE_STATE_PATH, trace: 'off' },
      testIgnore: [/auth-boundary\.spec\.ts/, /global-setup\.ts/],
      dependencies: ['setup'],
    },

    // Uncomment for mobile browsers support
    /* {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] },
    },
    {
      name: 'Mobile Safari',
      use: { ...devices['iPhone 12'] },
    }, */

    // Uncomment for branded browsers
    /* {
      name: 'Microsoft Edge',
      use: { ...devices['Desktop Edge'], channel: 'msedge' },
    },
    {
      name: 'Google Chrome',
      use: { ...devices['Desktop Chrome'], channel: 'chrome' },
    } */
  ],
});
