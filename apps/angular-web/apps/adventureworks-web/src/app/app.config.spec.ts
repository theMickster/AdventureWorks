import { describe, expect, it, vi } from 'vitest';
import { runSignalrEventSubscriptionStep, runSignalrLifecycleStep, runStoreRefreshOnReconnectStep } from './app.config';

describe('runSignalrLifecycleStep', () => {
  it('disconnects when unauthenticated', async () => {
    const signalrService = {
      connect: vi.fn().mockResolvedValue(undefined),
      disconnect: vi.fn().mockResolvedValue(undefined),
    };
    const appInsightsService = {
      trackException: vi.fn(),
    };

    await runSignalrLifecycleStep(false, signalrService, appInsightsService);
    expect(signalrService.disconnect).toHaveBeenCalledTimes(1);
    expect(signalrService.connect).not.toHaveBeenCalled();
  });

  it('connects when authenticated', async () => {
    const signalrService = {
      connect: vi.fn().mockResolvedValue(undefined),
      disconnect: vi.fn().mockResolvedValue(undefined),
    };
    const appInsightsService = {
      trackException: vi.fn(),
    };
    await runSignalrLifecycleStep(true, signalrService, appInsightsService);
    expect(signalrService.connect).toHaveBeenCalledTimes(1);
    expect(signalrService.disconnect).not.toHaveBeenCalled();
  });

  it('tracks exceptions from connect failures', async () => {
    const signalrService = {
      connect: vi.fn().mockRejectedValue(new Error('connect failed')),
      disconnect: vi.fn().mockResolvedValue(undefined),
    };
    const appInsightsService = {
      trackException: vi.fn(),
    };

    await runSignalrLifecycleStep(true, signalrService, appInsightsService);

    expect(signalrService.connect).toHaveBeenCalledTimes(1);
    expect(appInsightsService.trackException).toHaveBeenCalledTimes(1);
  });
});

describe('runStoreRefreshOnReconnectStep', () => {
  it('does not refresh stores on initial connect', () => {
    const stores = {
      storeStore: { refresh: vi.fn() },
      salesPersonStore: { refresh: vi.fn() },
      employeeStore: { refresh: vi.fn() },
    };

    const next = runStoreRefreshOnReconnectStep(undefined, 'connected', stores);

    expect(next).toBe('connected');
    expect(stores.storeStore.refresh).not.toHaveBeenCalled();
    expect(stores.salesPersonStore.refresh).not.toHaveBeenCalled();
    expect(stores.employeeStore.refresh).not.toHaveBeenCalled();
  });

  it('refreshes stores once when reconnecting transitions to connected', () => {
    const stores = {
      storeStore: { refresh: vi.fn() },
      salesPersonStore: { refresh: vi.fn() },
      employeeStore: { refresh: vi.fn() },
    };

    let previous = runStoreRefreshOnReconnectStep(undefined, 'connected', stores);
    previous = runStoreRefreshOnReconnectStep(previous, 'reconnecting', stores);
    previous = runStoreRefreshOnReconnectStep(previous, 'connected', stores);
    previous = runStoreRefreshOnReconnectStep(previous, 'connected', stores);

    expect(previous).toBe('connected');
    expect(stores.storeStore.refresh).toHaveBeenCalledTimes(1);
    expect(stores.salesPersonStore.refresh).toHaveBeenCalledTimes(1);
    expect(stores.employeeStore.refresh).toHaveBeenCalledTimes(1);
  });
});

describe('runSignalrEventSubscriptionStep', () => {
  it('registers handlers once when connection becomes connected', () => {
    const signalrService = { on: vi.fn() };
    const stores = {
      storeStore: { applySignalrStoreUpdated: vi.fn() },
      employeeStore: { handleSignalrEmployeeLifecycleEvent: vi.fn() },
    };
    const appInsightsService = { trackException: vi.fn() };

    let handlersRegistered = runSignalrEventSubscriptionStep(
      false,
      'connected',
      signalrService,
      stores,
      appInsightsService,
    );
    handlersRegistered = runSignalrEventSubscriptionStep(
      handlersRegistered,
      'connected',
      signalrService,
      stores,
      appInsightsService,
    );

    expect(handlersRegistered).toBe(true);
    expect(signalrService.on).toHaveBeenCalledTimes(3);
  });

  it('resets registration on disconnected status', () => {
    const signalrService = { on: vi.fn() };
    const stores = {
      storeStore: { applySignalrStoreUpdated: vi.fn() },
      employeeStore: { handleSignalrEmployeeLifecycleEvent: vi.fn() },
    };
    const appInsightsService = { trackException: vi.fn() };

    const handlersRegistered = runSignalrEventSubscriptionStep(true, 'disconnected', signalrService, stores, appInsightsService);

    expect(handlersRegistered).toBe(false);
    expect(signalrService.on).not.toHaveBeenCalled();
  });

  it('tracks exception when handler registration fails', () => {
    const signalrService = {
      on: vi.fn().mockImplementation(() => {
        throw new Error('registration failed');
      }),
    };
    const stores = {
      storeStore: { applySignalrStoreUpdated: vi.fn() },
      employeeStore: { handleSignalrEmployeeLifecycleEvent: vi.fn() },
    };
    const appInsightsService = { trackException: vi.fn() };

    const handlersRegistered = runSignalrEventSubscriptionStep(
      false,
      'connected',
      signalrService,
      stores,
      appInsightsService,
    );

    expect(handlersRegistered).toBe(false);
    expect(appInsightsService.trackException).toHaveBeenCalledTimes(1);
  });
});
