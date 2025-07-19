/**
 * Copy this file to `environment.development.ts` and fill in your values.
 * The actual development environment file is gitignored.
 */
import { Environment } from '@adventureworks-web/shared/util';

export const environment: Environment = {
  production: false,
  defaultLocale: 'en',
  api: {
    primary: { baseUrl: 'https://localhost:5001/api', name: 'AdventureWorks API' },
  },
  appInsights: {
    connectionString: '<YOUR_APP_INSIGHTS_CONNECTION_STRING>',
    cloudRoleName: 'AdventureWorks.Web',
  },
};
