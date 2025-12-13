import { describe, expect, it, vi } from 'vitest';
import { acquireToken, validateEnvVars, type LoadTestCredentials } from './get-token';

const ALL_VARS = {
  LOADTEST_TENANT_ID: 'tenant-id',
  LOADTEST_CLIENT_ID: 'client-id',
  LOADTEST_API_SCOPE: 'api://scope/.default',
  LOADTEST_USERNAME: 'user@example.com',
  LOADTEST_PASSWORD: 'super-secret',
};

const CREDS: LoadTestCredentials = {
  tenantId: 'tenant-id',
  clientId: 'client-id',
  apiScope: 'api://scope/.default',
  username: 'user@example.com',
  password: 'super-secret',
};

function stubPca(acquireTokenByUsernamePassword: (...args: unknown[]) => unknown) {
  return () => ({ acquireTokenByUsernamePassword }) as never;
}

describe('validateEnvVars', () => {
  it('returns credentials when all vars are present', () => {
    const result = validateEnvVars(ALL_VARS);

    expect(result).toEqual(CREDS);
  });

  it('names exactly the one missing var', () => {
    const env = { ...ALL_VARS, LOADTEST_PASSWORD: undefined };

    expect(() => validateEnvVars(env)).toThrow(
      'Missing required environment variable(s): LOADTEST_PASSWORD',
    );
  });

  it('names all missing vars, comma-joined', () => {
    const env = { ...ALL_VARS, LOADTEST_TENANT_ID: undefined, LOADTEST_PASSWORD: undefined };

    expect(() => validateEnvVars(env)).toThrow(
      'Missing required environment variable(s): LOADTEST_TENANT_ID, LOADTEST_PASSWORD',
    );
  });
});

describe('acquireToken', () => {
  it('returns the access token when MSAL resolves', async () => {
    const pcaFactory = stubPca(vi.fn().mockResolvedValue({ accessToken: 'the-token' }));

    const token = await acquireToken(CREDS, pcaFactory);

    expect(token).toBe('the-token');
  });

  it('propagates MSAL rejection verbatim, unwrapped', async () => {
    const authError = new Error('AADSTS50126: invalid username or password');
    const pcaFactory = stubPca(vi.fn().mockRejectedValue(authError));

    await expect(acquireToken(CREDS, pcaFactory)).rejects.toBe(authError);
  });

  it('throws instead of returning an empty string when accessToken is falsy', async () => {
    const pcaFactory = stubPca(vi.fn().mockResolvedValue({ accessToken: '' }));

    await expect(acquireToken(CREDS, pcaFactory)).rejects.toThrow();
  });

  it('throws instead of returning an empty string when MSAL resolves with no result', async () => {
    const pcaFactory = stubPca(vi.fn().mockResolvedValue(null));

    await expect(acquireToken(CREDS, pcaFactory)).rejects.toThrow();
  });
});
