import { PublicClientApplication, type Configuration } from '@azure/msal-node';

/**
 * Acquires an Entra ID access token for authenticated k6 load-test runs, via the ROPC (Resource
 * Owner Password Credential) flow against a dedicated test tenant/app registration — the same
 * Microsoft-documented pattern used by the Playwright E2E suite's `global-setup.ts`
 * (learn.microsoft.com/entra/identity-platform/test-automate-integration-testing).
 * `acquireTokenByUsernamePassword` is marked `@deprecated` in `@azure/msal-node` — Microsoft
 * discourages ROPC in general (no MFA support) and warns it may be removed in a future release,
 * but there is no replacement API for ROPC in this library. Used here only against a dedicated
 * test tenant/app registration with MFA excluded via Conditional Access — never against
 * production Entra config.
 *
 * `run-tests.sh` invokes this file via `npx tsx get-token.ts` and captures stdout with `$(...)`:
 * stdout is reserved exclusively for the bare token string on success. All diagnostics
 * (validation errors, MSAL rejections) go to stderr so they still print live to the terminal
 * without being captured into `K6_AUTH_TOKEN`.
 */

export interface LoadTestCredentials {
  tenantId: string;
  clientId: string;
  apiScope: string;
  username: string;
  password: string;
}

const REQUIRED_VARS = [
  'LOADTEST_TENANT_ID',
  'LOADTEST_CLIENT_ID',
  'LOADTEST_API_SCOPE',
  'LOADTEST_USERNAME',
  'LOADTEST_PASSWORD',
] as const;

export function validateEnvVars(env: NodeJS.ProcessEnv): LoadTestCredentials {
  const missing = REQUIRED_VARS.filter((name) => !env[name]);

  if (missing.length > 0) {
    throw new Error(`Missing required environment variable(s): ${missing.join(', ')}`);
  }

  return {
    tenantId: env['LOADTEST_TENANT_ID'] as string,
    clientId: env['LOADTEST_CLIENT_ID'] as string,
    apiScope: env['LOADTEST_API_SCOPE'] as string,
    username: env['LOADTEST_USERNAME'] as string,
    password: env['LOADTEST_PASSWORD'] as string,
  };
}

export async function acquireToken(
  creds: LoadTestCredentials,
  pcaFactory: (config: Configuration) => PublicClientApplication = (config) =>
    new PublicClientApplication(config),
): Promise<string> {
  const pca = pcaFactory({
    auth: {
      clientId: creds.clientId,
      authority: `https://login.microsoftonline.com/${creds.tenantId}`,
    },
  });

  const result = await pca.acquireTokenByUsernamePassword({
    scopes: [creds.apiScope],
    username: creds.username,
    password: creds.password,
  });

  if (!result || !result.accessToken) {
    throw new Error('MSAL returned no access token for the LOADTEST_* credentials.');
  }

  return result.accessToken;
}

async function main(): Promise<void> {
  let creds: LoadTestCredentials;
  try {
    creds = validateEnvVars(process.env);
  } catch (err) {
    console.error((err as Error).message);
    process.exit(1);
  }

  try {
    const token = await acquireToken(creds);
    process.stdout.write(token);
  } catch (error) {
    console.error(error);
    process.exit(1);
  }
}

const isCliEntrypoint = process.argv[1]?.endsWith('get-token.ts');
if (isCliEntrypoint) {
  void main();
}
