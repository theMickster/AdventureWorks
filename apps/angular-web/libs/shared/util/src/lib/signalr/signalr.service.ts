import { DestroyRef, inject, Injectable, signal } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { firstValueFrom } from 'rxjs';
import { Environment } from '../environment/environment.model';
import { ENVIRONMENT } from '../environment/environment.token';
import { AppInsightsService } from '../telemetry/app-insights.service';
import { SignalRConnectionStatus, SignalRHandler } from './signalr-connection.model';

// Delay schedule: 0, 2s, 5s, 10s, 30s (Feature 777 spec).
const DEFAULT_RECONNECT_DELAYS_MS = [0, 2000, 5000, 10000, 30000];
const MAX_RECONNECT_ATTEMPTS = 8;
const MAX_RECONNECT_DELAY_MS = 30_000;
const LOCAL_HOSTNAMES = new Set(['localhost', '127.0.0.1', '[::1]']);

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private readonly environment = inject<Environment>(ENVIRONMENT);
  private readonly msalService = inject(MsalService);
  private readonly appInsightsService = inject(AppInsightsService);
  private readonly status = signal<SignalRConnectionStatus>('disconnected');

  private connection: HubConnection | null = null;

  /** Read-only signal reflecting the current hub connection: `'connecting'` | `'connected'` | `'reconnecting'` | `'disconnected'`. */
  readonly connectionStatus = this.status.asReadonly();

  /** Starts a SignalR connection if one is not already active. */
  async connect(): Promise<void> {
    if (this.isConnectionActive()) {
      return;
    }
    this.status.set('connecting');
    try {
      const connection = this.createConnection();
      this.connection = connection;
      await connection.start();
      this.status.set('connected');
    } catch (error: unknown) {
      this.connection = null;
      this.status.set('disconnected');
      this.appInsightsService.trackException(
        error instanceof Error ? error : new Error(String(error)),
      );
      // intentionally no re-throw — connectionStatus signal is the source of truth for callers
    }
  }

  /** Stops the SignalR connection and resets status to disconnected. */
  async disconnect(): Promise<void> {
    if (!this.connection) {
      this.status.set('disconnected');
      return;
    }

    const connection = this.connection;
    this.connection = null;

    try {
      await connection.stop();
    } catch (error: unknown) {
      throw new Error(
        'Failed to stop SignalR connection cleanly.',
        { cause: error instanceof Error ? error : new Error(String(error)) },
      );
    } finally {
      this.status.set('disconnected');
    }
  }

  /**
   * Tears down the current connection and starts a fresh one.
   * Call this after the automatic retry schedule is exhausted (`connectionStatus === 'disconnected'`).
   * Status transitions through `'disconnected'` before moving to `'connecting'` / `'connected'`.
   */
  async manualReconnect(): Promise<void> {
    await this.disconnect();
    await this.connect();
  }

  /** Invokes a hub method on the server and returns the result. */
  async invoke<T>(methodName: string, ...args: unknown[]): Promise<T> {
    if (!this.connection) {
      throw new Error('SignalR connection is not initialized. Call connect() before invoking hub methods.');
    }
    return this.connection.invoke<T>(methodName, ...args);
  }

  /** Registers a strongly typed handler for a server event. Pass `destroyRef` to auto-wire teardown to component destruction; call the returned `() => void` manually otherwise. */
  on<T>(eventName: string, handler: SignalRHandler<T>, destroyRef?: DestroyRef): () => void {
    if (!this.connection) {
      throw new Error('SignalR connection is not initialized. Call connect() before subscribing to events.');
    }

    this.connection.on(eventName, handler as (...args: unknown[]) => void);

    // Capture the connection reference now so the teardown remains safe
    // even after disconnect() nulls this.connection.
    const capturedConnection = this.connection;
    const teardown = () => {
      capturedConnection.off(eventName, handler as (...args: unknown[]) => void);
    };

    if (destroyRef) {
      destroyRef.onDestroy(teardown);
    }

    return teardown;
  }

  private isConnectionActive(): boolean {
    if (!this.connection) {
      return false;
    }

    return [HubConnectionState.Connected, HubConnectionState.Connecting, HubConnectionState.Reconnecting].includes(
      this.connection.state,
    );
  }

  private createConnection(): HubConnection {
    const connection = new HubConnectionBuilder()
      .withUrl(this.getHubUrl(), { accessTokenFactory: () => this.acquireAccessToken() })
      .withAutomaticReconnect(this.getReconnectDelays())
      .build();

    connection.onreconnecting(() => this.status.set('reconnecting'));
    connection.onreconnected(() => this.status.set('connected'));
    connection.onclose(() => this.status.set('disconnected'));

    return connection;
  }

  private getHubUrl(): string {
    const hubUrl = this.environment.signalr?.hubUrl?.trim();
    // `__` is the deploy-time substitution sentinel — throwing fast prevents tokens from being sent to a wrong endpoint.
    if (!hubUrl || hubUrl.startsWith('__')) {
      throw new Error('SignalR hub URL is missing or has not been substituted by the deployment pipeline.');
    }

    const resolvedHubUrl = this.resolveUrl(hubUrl);
    const isHttps = resolvedHubUrl.protocol === 'https:';
    const isLocalHttp = resolvedHubUrl.protocol === 'http:' && LOCAL_HOSTNAMES.has(resolvedHubUrl.hostname);
    if (!isHttps && !isLocalHttp) {
      throw new Error('SignalR hub URL must use HTTPS (or HTTP for localhost development only).');
    }

    const trustedOrigins = this.getTrustedHubOrigins();
    if (!trustedOrigins.has(resolvedHubUrl.origin)) {
      throw new Error('SignalR hub URL origin is not trusted.');
    }

    return resolvedHubUrl.toString();
  }

  private getReconnectDelays(): number[] {
    const configuredDelays = this.environment.signalr?.reconnectDelaysMs ?? DEFAULT_RECONNECT_DELAYS_MS;
    const safeDelays = configuredDelays
      .filter((delay) => Number.isInteger(delay) && delay >= 0 && delay <= MAX_RECONNECT_DELAY_MS)
      .slice(0, MAX_RECONNECT_ATTEMPTS);

    if (safeDelays.length === 0) {
      return DEFAULT_RECONNECT_DELAYS_MS;
    }

    return safeDelays;
  }

  private getSignalRScopes(): string[] {
    const signalRScopes = this.environment.signalr?.scopes;
    if (signalRScopes && signalRScopes.length > 0) {
      return signalRScopes;
    }

    const authScopes = this.environment.auth?.scopes;
    if (authScopes && authScopes.length > 0) {
      return authScopes;
    }

    throw new Error('SignalR auth scopes are missing. Configure signalr.scopes or auth.scopes in environment settings.');
  }

  private async acquireAccessToken(): Promise<string> {
    const existingActive = this.msalService.instance.getActiveAccount();
    const activeAccount = existingActive ?? this.msalService.instance.getAllAccounts()[0];
    if (!activeAccount) {
      throw new Error('Cannot acquire SignalR access token because no authenticated account is available.');
    }

    if (!existingActive) {
      this.msalService.instance.setActiveAccount(activeAccount);
    }

    // No try/catch — raw MSAL errors (e.g. InteractionRequiredAuthError) propagate to connect()'s catch, which logs the real cause.
    const result = await firstValueFrom(
      this.msalService.acquireTokenSilent({
        account: activeAccount,
        scopes: this.getSignalRScopes(),
      }),
    );

    if (!result.accessToken) {
      throw new Error('SignalR access token acquisition returned an empty token.');
    }

    return result.accessToken;
  }

  private getTrustedHubOrigins(): Set<string> {
    // Both window origin and API origin are trusted by default — the hub may be co-hosted with the API.
    const trustedOrigins = new Set<string>();
    trustedOrigins.add(this.getCurrentOrigin());
    trustedOrigins.add(this.resolveUrl(this.environment.api.primary.baseUrl).origin);

    for (const configuredOrigin of this.environment.signalr?.trustedOrigins ?? []) {
      trustedOrigins.add(this.resolveUrl(configuredOrigin).origin);
    }

    return trustedOrigins;
  }

  private resolveUrl(url: string): URL {
    try {
      return new URL(url, this.getCurrentOrigin());
    } catch {
      throw new Error('SignalR hub URL is invalid.');
    }
  }

  private getCurrentOrigin(): string {
    if (typeof window === 'undefined') {
      throw new Error('SignalR requires a browser runtime.');
    }

    return window.location.origin;
  }
}
