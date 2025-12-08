/**
 * Copy this file to `environment.playwright.ts` and fill in your test-tenant values.
 * The actual playwright environment file is gitignored; CI materializes it from
 * pipeline secrets (see pipelines/templates/e2e.yml). It is served by the
 * `adventureworks-web:serve:playwright` configuration that the E2E webServer boots,
 * so it can hold the dedicated Entra test tenant without touching your
 * `environment.development.ts`.
 */
import { Environment } from '@adventureworks-web/shared/util';

export const environment: Environment = {
  production: false,
  defaultLocale: 'en',
  api: {
    primary: { baseUrl: 'http://localhost:5000/api', name: 'AdventureWorks API' },
  },
  auth: {
    authority: 'https://login.microsoftonline.com/<TEST_TENANT_ID>',
    clientId: '<TEST_SPA_CLIENT_ID>',
    redirectUri: 'http://localhost:4200',
    postLogoutRedirectUri: 'http://localhost:4200',
    scopes: ['api://<TEST_API_CLIENT_ID>/access_via_group_assignments'],
  },
  signalr: {
    hubUrl: 'http://localhost:5000/hubs/dashboard',
  },
};
