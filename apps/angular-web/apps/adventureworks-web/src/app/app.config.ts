import {
  ApplicationConfig,
  effect,
  EffectRef,
  ErrorHandler,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import {
  HTTP_INTERCEPTORS,
  provideHttpClient,
  withFetch,
  withInterceptors,
  withInterceptorsFromDi,
} from '@angular/common/http';
import { provideRouter, TitleStrategy, withComponentInputBinding } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import {
  MSAL_GUARD_CONFIG,
  MSAL_INSTANCE,
  MSAL_INTERCEPTOR_CONFIG,
  MsalBroadcastService,
  MsalGuard,
  MsalInterceptor,
  MsalService,
} from '@azure/msal-angular';
import {
  AppInsightsErrorHandler,
  AppInsightsService,
  AuthService,
  AwTitleStrategy,
  correlationIdInterceptor,
  ENVIRONMENT,
  errorInterceptor,
  loadingInterceptor,
  msalGuardConfigFactory,
  msalInstanceFactory,
  msalInterceptorConfigFactory,
  EntityChangedEvent,
  SignalRConnectionStatus,
  SignalrService,
} from '@adventureworks-web/shared/util';
import { EmployeeStore } from '@adventureworks-web/hr/data-access';
import { SalesPersonStore, StoreStore } from '@adventureworks-web/sales/data-access';
import { environment } from '../environments/environment';
import { appRoutes } from './app.routes';

export async function runSignalrLifecycleStep(
  isAuthenticated: boolean,
  signalrService: Pick<SignalrService, 'connect' | 'disconnect' | 'connectionStatus'>,
  appInsightsService: Pick<AppInsightsService, 'trackException'>,
): Promise<void> {
  if (isAuthenticated) {
    await signalrService.connect();
    // Retry once after a short delay to recover from transient MSAL errors (e.g. block_iframe_reload)
    // that can occur when acquireTokenSilent is called while the auth state is still settling on page load.
    if (signalrService.connectionStatus() === 'disconnected') {
      await new Promise<void>((resolve) => setTimeout(resolve, 2000));
      await signalrService.connect();
    }
    return;
  }

  await signalrService.disconnect().catch((error: unknown) => {
    appInsightsService.trackException(error instanceof Error ? error : new Error(String(error)));
  });
}

/** Starts or stops the SignalR connection in response to the current auth state. */
export function initializeSignalrLifecycle(
  authService: Pick<AuthService, 'isAuthenticated'> = inject(AuthService),
  signalrService: Pick<SignalrService, 'connect' | 'disconnect' | 'connectionStatus'> = inject(SignalrService),
  appInsightsService: Pick<AppInsightsService, 'trackException'> = inject(AppInsightsService),
): EffectRef {
  return effect(() => {
    void runSignalrLifecycleStep(authService.isAuthenticated(), signalrService, appInsightsService);
  });
}

export function runStoreRefreshOnReconnectStep(
  previousStatus: SignalRConnectionStatus | undefined,
  currentStatus: SignalRConnectionStatus,
  stores: {
    storeStore: { refresh: () => void };
    salesPersonStore: { refresh: () => void };
    employeeStore: { refresh: () => void };
  },
): SignalRConnectionStatus {
  if (previousStatus === 'reconnecting' && currentStatus === 'connected') {
    stores.storeStore.refresh();
    stores.salesPersonStore.refresh();
    stores.employeeStore.refresh();
  }

  return currentStatus;
}

export function runSignalrEventSubscriptionStep(
  handlersRegistered: boolean,
  currentStatus: SignalRConnectionStatus,
  signalrService: Pick<SignalrService, 'on'>,
  stores: {
    storeStore: { refresh: () => void };
    employeeStore: { handleSignalrEmployeeLifecycleEvent: () => void };
  },
  appInsightsService: Pick<AppInsightsService, 'trackException'>,
): boolean {
  if (currentStatus === 'disconnected') {
    return false;
  }

  if (currentStatus !== 'connected' || handlersRegistered) {
    return handlersRegistered;
  }

  try {
    // EntityChanged carries a notification (entityType, entityId) — not store data.
    // Each handler re-fetches; do not attempt to patch from the event payload.
    // Group subscription is handled server-side in DashboardHub.OnConnectedAsync.
    signalrService.on<EntityChangedEvent>('EntityChanged', (payload) => {
      if (payload.entityType === 'Store') {
        stores.storeStore.refresh();
      } else if (payload.entityType === 'Employee') {
        stores.employeeStore.handleSignalrEmployeeLifecycleEvent();
      }
    });
    return true;
  } catch (error: unknown) {
    appInsightsService.trackException(error instanceof Error ? error : new Error(String(error)));
    return false;
  }
}

export function initializeSignalrReconnectRefresh(
  signalrService: Pick<SignalrService, 'connectionStatus'> = inject(SignalrService),
  storeStore: { refresh: () => void } = inject(StoreStore),
  salesPersonStore: { refresh: () => void } = inject(SalesPersonStore),
  employeeStore: { refresh: () => void } = inject(EmployeeStore),
): EffectRef {
  let previousStatus: SignalRConnectionStatus | undefined;

  return effect(() => {
    previousStatus = runStoreRefreshOnReconnectStep(previousStatus, signalrService.connectionStatus(), {
      storeStore,
      salesPersonStore,
      employeeStore,
    });
  });
}

/** Subscribes stores to SignalR events so live updates can flow into signal stores. */
export function initializeSignalrEventSubscriptions(
  signalrService: Pick<SignalrService, 'connectionStatus' | 'on'> = inject(SignalrService),
  storeStore: { refresh: () => void } = inject(StoreStore),
  employeeStore: { handleSignalrEmployeeLifecycleEvent: () => void } = inject(EmployeeStore),
  appInsightsService: Pick<AppInsightsService, 'trackException'> = inject(AppInsightsService),
): EffectRef {
  let handlersRegistered = false;

  return effect(() => {
    handlersRegistered = runSignalrEventSubscriptionStep(
      handlersRegistered,
      signalrService.connectionStatus(),
      signalrService,
      { storeStore, employeeStore },
      appInsightsService,
    );
  });
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    { provide: ErrorHandler, useClass: AppInsightsErrorHandler },
    provideHttpClient(
      withFetch(),
      withInterceptors([correlationIdInterceptor, loadingInterceptor, errorInterceptor]),
      withInterceptorsFromDi(),
    ),
    { provide: ENVIRONMENT, useValue: environment },
    provideRouter(appRoutes, withComponentInputBinding()),
    { provide: TitleStrategy, useClass: AwTitleStrategy },
    provideTranslateService({
      fallbackLang: 'en',
      loader: provideTranslateHttpLoader({ prefix: './i18n/' }),
    }),
    { provide: MSAL_INSTANCE, useFactory: msalInstanceFactory, deps: [ENVIRONMENT] },
    { provide: MSAL_GUARD_CONFIG, useFactory: msalGuardConfigFactory, deps: [ENVIRONMENT] },
    { provide: MSAL_INTERCEPTOR_CONFIG, useFactory: msalInterceptorConfigFactory, deps: [ENVIRONMENT] },
    { provide: HTTP_INTERCEPTORS, useClass: MsalInterceptor, multi: true },
    MsalService,
    MsalGuard,
    MsalBroadcastService,
    provideAppInitializer(() => inject(AuthService).initialize()),
    provideAppInitializer(() => {
      initializeSignalrLifecycle();
      initializeSignalrReconnectRefresh();
      initializeSignalrEventSubscriptions();
    }),
    provideAppInitializer(() => inject(AppInsightsService).initialize()),
  ],
};
