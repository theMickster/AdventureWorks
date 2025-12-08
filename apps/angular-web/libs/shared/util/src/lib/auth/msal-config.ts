import {
  BrowserCacheLocation,
  InteractionType,
  IPublicClientApplication,
  LogLevel,
  PublicClientApplication,
} from '@azure/msal-browser';
import { MsalGuardConfiguration, MsalInterceptorConfiguration } from '@azure/msal-angular';
import { Environment } from '../environment/environment.model';

/** Disabled-auth stub used when `environment.auth` is entirely absent (e.g. Docker local dev). */
const DISABLED_AUTH_CONFIG: NonNullable<Environment['auth']> = {
  clientId: '00000000-0000-0000-0000-000000000000',
  authority: 'https://login.microsoftonline.com/common',
  redirectUri: '/',
  postLogoutRedirectUri: '/',
  scopes: [],
};

function getAuthConfig(env: Environment): NonNullable<Environment['auth']> {
  if (!env.auth) {
    return DISABLED_AUTH_CONFIG;
  }
  if (env.auth.clientId.startsWith('__')) {
    throw new Error('Entra ID auth configuration is missing or has not been substituted by the deployment pipeline.');
  }
  return env.auth;
}

/** Creates a configured MSAL PublicClientApplication from environment settings. */
export function msalInstanceFactory(env: Environment): IPublicClientApplication {
  const auth = getAuthConfig(env);
  return new PublicClientApplication({
    auth: {
      clientId: auth.clientId,
      authority: auth.authority,
      redirectUri: auth.redirectUri,
      postLogoutRedirectUri: auth.postLogoutRedirectUri,
    },
    cache: {
      cacheLocation: BrowserCacheLocation.LocalStorage,
    },
    system: {
      loggerOptions: {
        logLevel: env.production ? LogLevel.Error : LogLevel.Warning,
        piiLoggingEnabled: false,
      },
    },
  });
}

/** Builds the MSAL interceptor config, mapping all API URLs to their OAuth scopes. */
export function msalInterceptorConfigFactory(env: Environment): MsalInterceptorConfiguration {
  const auth = getAuthConfig(env);
  const protectedResourceMap = new Map<string, string[]>();

  for (const key of Object.keys(env.api)) {
    const endpoint = env.api[key];
    protectedResourceMap.set(`${endpoint.baseUrl}/*`, endpoint.scopes ?? auth.scopes);
  }

  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap,
  };
}

/** Builds the MSAL guard config for redirect-based login with a fallback route. */
export function msalGuardConfigFactory(env: Environment): MsalGuardConfiguration {
  const auth = getAuthConfig(env);
  return {
    interactionType: InteractionType.Redirect,
    authRequest: { scopes: auth.scopes },
    loginFailedRoute: '/login-failed',
  };
}
