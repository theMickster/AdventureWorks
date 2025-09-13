import { TestBed } from '@angular/core/testing';
import { MsalService } from '@azure/msal-angular';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { Observable, of, throwError } from 'rxjs';
import { Environment } from '../environment/environment.model';
import { ENVIRONMENT } from '../environment/environment.token';
import { SignalrService } from './signalr.service';

class MockHubConnection {
  state: HubConnectionState = HubConnectionState.Disconnected;
  readonly start: ReturnType<typeof vi.fn>;
  readonly stop: ReturnType<typeof vi.fn>;
  readonly on: ReturnType<typeof vi.fn>;

  private reconnectingHandler?: () => void;
  private reconnectedHandler?: () => void;
  private closeHandler?: () => void;
  private readonly handlers = new Map<string, (...args: unknown[]) => void>();

  constructor(private readonly accessTokenFactory?: () => Promise<string | undefined>) {
    this.on = vi.fn((eventName: string, handler: (...args: unknown[]) => void) => {
      this.handlers.set(eventName, handler);
    });

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
} {
  const getActiveAccount = vi.fn(() => activeAccount);
  const setActiveAccount = vi.fn();
  const acquireTokenSilent = vi.fn(() => acquireTokenSilentResult);

  TestBed.configureTestingModule({
    providers: [
      SignalrService,
      { provide: ENVIRONMENT, useValue: environment },
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

  it('fails connect when no authenticated account is available', async () => {
    const { service } = setup({ activeAccount: null, allAccounts: [] });

    await expect(service.connect()).rejects.toThrowError('Failed to establish SignalR connection.');
    expect(service.connectionStatus()).toBe('disconnected');
  });

  it('fails connect when token acquisition fails', async () => {
    const { service } = setup({
      acquireTokenSilentResult: throwError(() => new Error('token acquisition failed')) as Observable<{
        accessToken: string;
      }>,
    });

    await expect(service.connect()).rejects.toThrowError('Failed to establish SignalR connection.');
    expect(service.connectionStatus()).toBe('disconnected');
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

    expect(lastReconnectDelays).toEqual([0, 2000, 5000, 10000]);
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
});
