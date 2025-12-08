import { InteractionType } from '@azure/msal-browser';
import { Environment } from '../environment/environment.model';
import { msalGuardConfigFactory, msalInstanceFactory, msalInterceptorConfigFactory } from './msal-config';

const baseAuth = {
  authority: 'https://login.microsoftonline.com/tenant-id',
  clientId: 'client-id',
  redirectUri: 'http://localhost:4200',
  postLogoutRedirectUri: 'http://localhost:4200',
  scopes: ['api://client-id/access'],
};

describe('msalInterceptorConfigFactory', () => {
  it('should map single API to global scopes', () => {
    const env: Environment = {
      production: false,
      defaultLocale: 'en',
      api: { primary: { baseUrl: 'https://api.example.com', name: 'Primary' } },
      auth: baseAuth,
    };

    const config = msalInterceptorConfigFactory(env);

    expect(config.interactionType).toBe(InteractionType.Redirect);
    expect(config.protectedResourceMap.size).toBe(1);
    expect(config.protectedResourceMap.get('https://api.example.com/*')).toEqual(baseAuth.scopes);
  });

  it('should map multiple APIs each to their own scopes', () => {
    const env: Environment = {
      production: false,
      defaultLocale: 'en',
      api: {
        primary: { baseUrl: 'https://api.example.com', name: 'Primary', scopes: ['api://primary/read'] },
        functions: { baseUrl: 'https://func.example.com', name: 'Functions', scopes: ['api://func/execute'] },
      },
      auth: baseAuth,
    };

    const config = msalInterceptorConfigFactory(env);

    expect(config.protectedResourceMap.size).toBe(2);
    expect(config.protectedResourceMap.get('https://api.example.com/*')).toEqual(['api://primary/read']);
    expect(config.protectedResourceMap.get('https://func.example.com/*')).toEqual(['api://func/execute']);
  });

  it('should fall back to global auth scopes when API has no scopes', () => {
    const env: Environment = {
      production: false,
      defaultLocale: 'en',
      api: {
        primary: { baseUrl: 'https://api.example.com', name: 'Primary' },
        functions: { baseUrl: 'https://func.example.com', name: 'Functions', scopes: ['api://func/execute'] },
      },
      auth: baseAuth,
    };

    const config = msalInterceptorConfigFactory(env);

    expect(config.protectedResourceMap.get('https://api.example.com/*')).toEqual(baseAuth.scopes);
    expect(config.protectedResourceMap.get('https://func.example.com/*')).toEqual(['api://func/execute']);
  });

  it('should always use Redirect interaction type', () => {
    const env: Environment = {
      production: false,
      defaultLocale: 'en',
      api: { primary: { baseUrl: 'https://api.example.com', name: 'Primary' } },
      auth: baseAuth,
    };

    const config = msalInterceptorConfigFactory(env);
    expect(config.interactionType).toBe(InteractionType.Redirect);
  });
});

describe('getAuthConfig (Docker / disabled-auth guard)', () => {
  const envWithoutAuth: Environment = {
    production: false,
    defaultLocale: 'en',
    api: { primary: { baseUrl: '/api', name: 'AdventureWorks API' } },
  };

  it('does not throw when environment.auth is entirely absent', () => {
    expect(() => msalInstanceFactory(envWithoutAuth)).not.toThrow();
    expect(() => msalGuardConfigFactory(envWithoutAuth)).not.toThrow();
    expect(() => msalInterceptorConfigFactory(envWithoutAuth)).not.toThrow();
  });

  it('still throws when environment.auth is present but unsubstituted', () => {
    const env: Environment = {
      ...envWithoutAuth,
      auth: { ...baseAuth, clientId: '__CLIENT_ID__' },
    };

    expect(() => msalInstanceFactory(env)).toThrow();
  });
});
