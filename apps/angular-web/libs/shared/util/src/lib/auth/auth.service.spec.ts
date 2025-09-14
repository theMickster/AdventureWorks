import { TestBed } from '@angular/core/testing';
import { MSAL_GUARD_CONFIG, MsalBroadcastService, MsalGuardConfiguration, MsalService } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';
import { Subject, of } from 'rxjs';
import { SignalrService } from '../signalr/signalr.service';
import { AppInsightsService } from '../telemetry/app-insights.service';
import { AuthService } from './auth.service';

function createDeferredPromise<T = void>(): {
  promise: Promise<T>;
  resolve: (value: T | PromiseLike<T>) => void;
  reject: (reason?: unknown) => void;
} {
  let resolvePromise!: (value: T | PromiseLike<T>) => void;
  let rejectPromise!: (reason?: unknown) => void;

  const promise = new Promise<T>((resolve, reject) => {
    resolvePromise = resolve;
    rejectPromise = reject;
  });

  return {
    promise,
    resolve: resolvePromise,
    reject: rejectPromise,
  };
}

function setup({
  disconnect = vi.fn(async () => undefined),
}: {
  disconnect?: ReturnType<typeof vi.fn>;
} = {}): {
  service: AuthService;
  logoutRedirect: ReturnType<typeof vi.fn>;
  disconnect: ReturnType<typeof vi.fn>;
} {
  const inProgress$ = new Subject<InteractionStatus>();
  const logoutRedirect = vi.fn(() => of(undefined));

  TestBed.configureTestingModule({
    providers: [
      AuthService,
      { provide: MSAL_GUARD_CONFIG, useValue: { authRequest: {} } as MsalGuardConfiguration },
      {
        provide: MsalBroadcastService,
        useValue: {
          inProgress$,
        },
      },
      {
        provide: MsalService,
        useValue: {
          handleRedirectObservable: vi.fn(() => of(undefined)),
          loginRedirect: vi.fn(() => of(undefined)),
          logoutRedirect,
          instance: {
            getActiveAccount: vi.fn(() => null),
            getAllAccounts: vi.fn(() => []),
            setActiveAccount: vi.fn(),
          },
        } as unknown as MsalService,
      },
      {
        provide: SignalrService,
        useValue: {
          disconnect,
        },
      },
      {
        provide: AppInsightsService,
        useValue: {
          trackException: vi.fn(),
        },
      },
    ],
  });

  return {
    service: TestBed.inject(AuthService),
    logoutRedirect,
    disconnect,
  };
}

describe('AuthService', () => {
  it('triggers SignalR disconnect before logoutRedirect', async () => {
    const deferredDisconnect = createDeferredPromise<void>();
    const disconnect = vi.fn(() => deferredDisconnect.promise);
    const { service, logoutRedirect } = setup({ disconnect });

    service.logout();
    await Promise.resolve();

    expect(disconnect).toHaveBeenCalledOnce();
    expect(logoutRedirect).not.toHaveBeenCalled();

    deferredDisconnect.resolve(undefined);
    await Promise.resolve();
    await Promise.resolve();

    expect(logoutRedirect).toHaveBeenCalledOnce();
  });

  it('is idempotent on rapid repeated logout calls', async () => {
    const deferredDisconnect = createDeferredPromise<void>();
    const disconnect = vi.fn(() => deferredDisconnect.promise);
    const { service, logoutRedirect } = setup({ disconnect });

    service.logout();
    service.logout();
    await Promise.resolve();

    expect(disconnect).toHaveBeenCalledOnce();
    expect(logoutRedirect).not.toHaveBeenCalled();

    deferredDisconnect.resolve(undefined);
    await Promise.resolve();
    await Promise.resolve();

    expect(logoutRedirect).toHaveBeenCalledOnce();
  });

  it('still calls logoutRedirect when disconnect fails', async () => {
    const disconnect = vi.fn(async () => await Promise.reject(new Error('disconnect failed')));
    const { service, logoutRedirect } = setup({ disconnect });

    service.logout();
    await Promise.resolve();
    await Promise.resolve();
    await Promise.resolve();

    expect(disconnect).toHaveBeenCalledOnce();
    expect(logoutRedirect).toHaveBeenCalledOnce();
  });
});
