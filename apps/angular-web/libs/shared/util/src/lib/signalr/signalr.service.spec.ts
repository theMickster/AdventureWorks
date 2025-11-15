import { DestroyRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MsalService } from '@azure/msal-angular';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { Observable, of, throwError } from 'rxjs';
import { Environment } from '../environment/environment.model';
import { ENVIRONMENT } from '../environment/environment.token';
import { AppInsightsService } from '../telemetry/app-insights.service';
import { SignalrService } from './signalr.service';

class MockHubConnection {
  state: HubConnectionState = HubConnectionState.Disconnected;
  readonly start: ReturnType<typeof vi.fn>;
  readonly stop: ReturnType<typeof vi.fn>;
  readonly on: ReturnType<typeof vi.fn>;
  readonly invoke: ReturnType<typeof vi.fn>;

  private reconnectingHandler?: () => void;
  private reconnectedHandler?: () => void;
  private closeHandler?: () => void;
  private readonly handlers = new Map<string, (...args: unknown[]) => void>();

  constructor(private readonly accessTokenFactory?: () => Promise<string | undefined>) {
    this.on = vi.fn((eventName: string, handler: (...args: unknown[]) => void) => {
      this.handlers.set(eventName, handler);
    });

    this.invoke = vi.fn().mockResolvedValue(undefined);

    this.start = vi.fn(async () => {
      this.state = HubConnectionState.Connecting;
      await this.accessTokenFactory?.();
      this.state = HubConnectionState.Connected;
    });

    this.stop = vi.fn(async () => {
      this.state = HubConnectionState.Disconnected;
    });
  }

  onreconnecting(handler: () => void): void {
    this.reconnectingHandler = handler;
  }

  onreconnected(handler: () => void): void {
    this.reconnectedHandler = handler;
  }

  onclose(handler: () => void): void {
    this.closeHandler = handler;
  }

  triggerReconnecting(): void {
    this.state = HubConnectionState.Reconnecting;
    this.reconnectingHandler?.();
  }

  triggerReconnected(): void {
    this.state = HubConnectionState.Connected;
    this.reconnectedHandler?.();
  }

  triggerClose(): void {
    this.state = HubConnectionState.Disconnected;
    this.closeHandler?.();
  }

  off(eventName: string, handler: (...args: unknown[]) => void): void {
    if (this.handlers.get(eventName) === handler) {
      this.handlers.delete(eventName);
    }
  }

  emit<T>(eventName: string, payload: T): void {
    this.handlers.get(eventName)?.(payload);
  }
}

const baseEnvironment: Environment = {
  production: false,
  defaultLocale: 'en',
  api: { primary: { baseUrl: 'https://localhost:5001/api', name: 'Primary' } },
  auth: {
    authority: 'https://login.microsoftonline.com/tenant-id',
    clientId: 'client-id',
    redirectUri: 'http://localhost:4200',
    postLogoutRedirectUri: 'http://localhost:4200',
    scopes: ['api://auth/access'],
  },
  signalr: {
    hubUrl: 'https://localhost:5001/hubs/notifications',
    scopes: ['api://signalr/access'],
    reconnectDelaysMs: [250, 500, 1000],
  },
};

function setup({
  environment = baseEnvironment,
  activeAccount = { username: 'test@adventureworks.com' },
  allAccounts = [{ username: 'test@adventureworks.com' }],
  acquireTokenSilentResult = of({ accessToken: 'token-123' }),
}: {
  environment?: Environment;
  activeAccount?: unknown | null;
  allAccounts?: unknown[];
  acquireTokenSilentResult?: Observable<{ accessToken: string }>;
} = {}): {
  service: SignalrService;
  acquireTokenSilent: ReturnType<typeof vi.fn>;
  getActiveAccount: ReturnType<typeof vi.fn>;
  setActiveAccount: ReturnType<typeof vi.fn>;
  appInsightsService: { trackException: ReturnType<typeof vi.fn> };
} {
  const getActiveAccount = vi.fn(() => activeAccount);
  const setActiveAccount = vi.fn();
  const acquireTokenSilent = vi.fn(() => acquireTokenSilentResult);
  const appInsightsService = { trackException: vi.fn() };

  TestBed.configureTestingModule({
    providers: [
      SignalrService,
      { provide: ENVIRONMENT, useValue: environment },
      { provide: AppInsightsService, useValue: appInsightsService },
      {
        provide: MsalService,
        useValue: {
          instance: {
            getActiveAccount,
            getAllAccounts: vi.fn(() => allAccounts),
            setActiveAccount,
          },
          acquireTokenSilent,
        } as unknown as MsalService,
      },
    ],
  });

  return {
    service: TestBed.inject(SignalrService),
    acquireTokenSilent,
    getActiveAccount,
    setActiveAccount,
    appInsightsService,
  };
}

describe('SignalrService', () => {
  let lastHubUrl: string | null;
  let lastReconnectDelays: number[] | null;
  let currentTokenFactory: (() => Promise<string | undefined>) | undefined;
  let currentConnection: MockHubConnection | null;

  beforeEach(() => {
    lastHubUrl = null;
    lastReconnectDelays = null;
    currentTokenFactory = undefined;
    currentConnection = null;

    vi.spyOn(HubConnectionBuilder.prototype, 'withUrl').mockImplementation(function (
      this: HubConnectionBuilder,
      url: string,
      options: { accessTokenFactory?: () => string | Promise<string> },
    ): HubConnectionBuilder {
      lastHubUrl = url;
      currentTokenFactory = options.accessTokenFactory
        ? async () => await options.accessTokenFactory?.()
        : undefined;
      return this;
    });

    const reconnectPrototype = HubConnectionBuilder.prototype as unknown as {
      withAutomaticReconnect: (reconnectPolicy: unknown) => HubConnectionBuilder;
    };
    vi.spyOn(reconnectPrototype, 'withAutomaticReconnect').mockImplementation(function (
      this: HubConnectionBuilder,
      reconnectPolicy: unknown,
    ): HubConnectionBuilder {
      if (Array.isArray(reconnectPolicy)) {
        lastReconnectDelays = reconnectPolicy;
      }
      return this;
    });

    vi.spyOn(HubConnectionBuilder.prototype, 'build').mockImplementation((): HubConnection => {
      currentConnection = new MockHubConnection(currentTokenFactory);
      return currentConnection as unknown as HubConnection;
    });
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('connects and disconnects while updating status', async () => {
    const { service } = setup();

    expect(service.connectionStatus()).toBe('disconnected');
    await service.connect();
    expect(service.connectionStatus()).toBe('connected');
    expect(currentConnection?.start).toHaveBeenCalledOnce();

    await service.disconnect();
    expect(currentConnection?.stop).toHaveBeenCalledOnce();
    expect(service.connectionStatus()).toBe('disconnected');
  });

  it('does not start a second connection when one is already active', async () => {
    const { service } = setup();
    await service.connect();
    const firstConnection = currentConnection;

    await service.connect();

    expect(currentConnection).toBe(firstConnection);
    expect(firstConnection?.start).toHaveBeenCalledOnce();
  });

  it('registers event handlers via on<T>() after connect', async () => {
    const { service } = setup();
    const handler = vi.fn<(payload: { id: string }) => void>();

    expect(() => service.on('OrderUpdated', handler)).toThrowError(/Call connect\(\)/);

    await service.connect();
    service.on('OrderUpdated', handler);
    currentConnection?.emit('OrderUpdated', { id: '42' });

    expect(handler).toHaveBeenCalledWith({ id: '42' });
  });

  it('invoke() rejects when called before connect', async () => {
    const { service } = setup();
    await expect(service.invoke('SomeHubMethod')).rejects.toThrowError(/Call connect\(\)/);
  });

  it('invoke() delegates to the hub connection', async () => {
    const { service } = setup();
    await service.connect();

    await service.invoke('SomeHubMethod');

    expect(currentConnection?.invoke).toHaveBeenCalledWith('SomeHubMethod');
  });

  it('updates connection status on reconnect lifecycle callbacks', async () => {
    const { service } = setup();
    await service.connect();

    currentConnection?.triggerReconnecting();
    expect(service.connectionStatus()).toBe('reconnecting');

    currentConnection?.triggerReconnected();
    expect(service.connectionStatus()).toBe('connected');

    currentConnection?.triggerClose();
    expect(service.connectionStatus()).toBe('disconnected');
  });

  it('manualReconnect() stops the current connection and creates a new one', async () => {
    const { service } = setup();
    await service.connect();
    const firstConnection = currentConnection;

    await service.manualReconnect();

    expect(firstConnection?.stop).toHaveBeenCalledOnce();
    expect(service.connectionStatus()).toBe('connected');
    expect(currentConnection).not.toBe(firstConnection);
    expect(currentConnection?.start).toHaveBeenCalledOnce();
  });

  it('manualReconnect() works when no prior connection exists', async () => {
    const { service } = setup();

    await service.manualReconnect();

    expect(service.connectionStatus()).toBe('connected');
    expect(currentConnection?.start).toHaveBeenCalledOnce();
  });

  it('fails connect when no authenticated account is available', async () => {
    const { service, appInsightsService } = setup({ activeAccount: null, allAccounts: [] });

    await service.connect();
    expect(service.connectionStatus()).toBe('disconnected');
    expect(appInsightsService.trackException).toHaveBeenCalledWith(
      expect.objectContaining({ message: expect.stringContaining('no authenticated account') }),
    );
  });

  it('fails connect when token acquisition fails', async () => {
    const tokenError = new Error('token acquisition failed');
    const { service, appInsightsService } = setup({
      acquireTokenSilentResult: throwError(() => tokenError) as Observable<{
        accessToken: string;
      }>,
    });

    await service.connect();
    expect(service.connectionStatus()).toBe('disconnected');
    expect(appInsightsService.trackException).toHaveBeenCalledWith(tokenError);
  });

  it('handles expired session gracefully without throwing', async () => {
    const interactionRequiredError = new Error('interaction_required: AADSTS65001');
    const { service, appInsightsService } = setup({
      acquireTokenSilentResult: throwError(() => interactionRequiredError) as Observable<{
        accessToken: string;
      }>,
    });

    await service.connect();
    expect(service.connectionStatus()).toBe('disconnected');
    expect(appInsightsService.trackException).toHaveBeenCalledOnce();
    expect(appInsightsService.trackException).toHaveBeenCalledWith(interactionRequiredError);
  });

  it('accessTokenFactory acquires a fresh token each time it is invoked', async () => {
    const { service, acquireTokenSilent } = setup();

    await service.connect();
    const callCountAfterConnect = acquireTokenSilent.mock.calls.length;

    await currentTokenFactory?.();

    expect(acquireTokenSilent.mock.calls.length).toBeGreaterThan(callCountAfterConnect);
  });

  it('uses custom reconnect delays and hub url from environment', async () => {
    const { service } = setup();
    await service.connect();

    expect(lastHubUrl).toBe('https://localhost:5001/hubs/notifications');
    expect(lastReconnectDelays).toEqual([250, 500, 1000]);
  });

  it('uses default reconnect delays when none are configured', async () => {
    const { service } = setup({
      environment: {
        ...baseEnvironment,
        signalr: {
          hubUrl: 'https://localhost:5001/hubs/notifications',
          scopes: ['api://signalr/access'],
        },
      },
    });

    await service.connect();

    expect(lastReconnectDelays).toEqual([0, 2000, 5000, 10000, 30000]);
  });

  it('bounds reconnect delays to non-negative integers, max delay, and max attempts', async () => {
    const { service } = setup({
      environment: {
        ...baseEnvironment,
        signalr: {
          hubUrl: 'https://localhost:5001/hubs/notifications',
          scopes: ['api://signalr/access'],
          reconnectDelaysMs: [-1, 0, 1.5, 1000, 30000, 30001, 2000, 3000, 4000, 5000, 6000, 7000],
        },
      },
    });

    await service.connect();

    expect(lastReconnectDelays).toEqual([0, 1000, 30000, 2000, 3000, 4000, 5000, 6000]);
  });

  it('sets first available account as active when active account is missing', async () => {
    const fallbackAccount = { username: 'fallback@adventureworks.com' };
    const { service, setActiveAccount, acquireTokenSilent, getActiveAccount } = setup({
      activeAccount: null,
      allAccounts: [fallbackAccount],
    });

    await service.connect();

    expect(getActiveAccount).toHaveBeenCalled();
    expect(setActiveAccount).toHaveBeenCalledWith(fallbackAccount);
    expect(acquireTokenSilent).toHaveBeenCalledWith(
      expect.objectContaining({
        account: fallbackAccount,
        scopes: ['api://signalr/access'],
      }),
    );
  });

  it('on<T>() returns a teardown that removes the handler', async () => {
    const { service } = setup();
    await service.connect();

    const handler = vi.fn();
    const teardown = service.on('EntityChanged', handler);

    currentConnection?.emit('EntityChanged', { entityType: 'Order', entityId: 1, action: 'Created', userName: 'test', timestamp: '2024-01-01T00:00:00Z' });
    expect(handler).toHaveBeenCalledOnce();

    teardown();
    currentConnection?.emit('EntityChanged', { entityType: 'Order', entityId: 1, action: 'Created', userName: 'test', timestamp: '2024-01-01T00:00:00Z' });
    expect(handler).toHaveBeenCalledOnce();
  });

  it('on<T>() teardown is safe to call when connection is already null', async () => {
    const { service } = setup();
    await service.connect();

    const teardown = service.on('EntityChanged', vi.fn());
    await service.disconnect();

    expect(() => teardown()).not.toThrow();
  });

  it('on<T>() with DestroyRef registers onDestroy callback', async () => {
    const { service } = setup();
    await service.connect();

    const mockDestroyRef = { onDestroy: vi.fn<[() => void], void>() };
    const handler = vi.fn();

    service.on('EntityChanged', handler, mockDestroyRef as unknown as DestroyRef);

    expect(mockDestroyRef.onDestroy).toHaveBeenCalledOnce();
    expect(mockDestroyRef.onDestroy).toHaveBeenCalledWith(expect.any(Function));
  });

  it('on<T>() with DestroyRef removes handler when destroy fires', async () => {
    const { service } = setup();
    await service.connect();

    const mockDestroyRef = { onDestroy: vi.fn<[() => void], void>() };
    const handler = vi.fn();

    service.on('EntityChanged', handler, mockDestroyRef as unknown as DestroyRef);

    mockDestroyRef.onDestroy.mock.calls[0][0]();
    currentConnection?.emit('EntityChanged', { entityType: 'Order', entityId: 1, action: 'Created', userName: 'test', timestamp: '2024-01-01T00:00:00Z' });

    expect(handler).not.toHaveBeenCalled();
  });
});
