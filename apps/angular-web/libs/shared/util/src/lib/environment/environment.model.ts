/** Configuration for a single API backend endpoint. */
export interface ApiEndpoint {
  /** Base URL for API requests (e.g., 'https://localhost:5001/api' or '/api'). */
  baseUrl: string;
  /** Human-readable name for logging and diagnostics. */
  name: string;
}

/** Typed environment configuration consumed across the workspace. */
export interface Environment {
  /** Whether the app is running in production mode. */
  production: boolean;
  /** Default locale for i18n (e.g., 'en'). */
  defaultLocale: string;
  /** API endpoint registry. `primary` is required; additional endpoints are optional. */
  api: {
    primary: ApiEndpoint;
    [key: string]: ApiEndpoint;
  };
  /** Optional Microsoft Entra ID authentication settings. */
  auth?: {
    /** Authority URL (e.g., 'https://login.microsoftonline.com/{tenant}'). */
    authority: string;
    /** Application (client) ID registered in Entra ID. */
    clientId: string;
    /** Redirect URI after authentication. */
    redirectUri: string;
  };
}
