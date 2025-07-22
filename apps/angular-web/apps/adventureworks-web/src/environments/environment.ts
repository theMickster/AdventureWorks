import { Environment } from '@adventureworks-web/shared/util';

export const environment: Environment = {
  production: true,
  defaultLocale: 'en',
  api: {
    primary: { baseUrl: '/api', name: 'AdventureWorks API' },
  },
  appInsights: {
    connectionString: '__APP_INSIGHTS_CONNECTION_STRING__',
    cloudRoleName: 'AdventureWorks.Web',
  },
  auth: {
    authority: '__ENTRA_AUTHORITY__',
    clientId: '__ENTRA_CLIENT_ID__',
    redirectUri: '__ENTRA_REDIRECT_URI__',
    postLogoutRedirectUri: '__ENTRA_POST_LOGOUT_REDIRECT_URI__',
    scopes: ['__ENTRA_API_SCOPE__'],
  },
};
