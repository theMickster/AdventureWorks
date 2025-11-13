import { describe, expect, it, vi } from 'vitest';
import { runSignalrEventSubscriptionStep, runSignalrLifecycleStep, runStoreRefreshOnReconnectStep } from './app.config';

describe('runSignalrLifecycleStep', () => {
  it('disconnects when unauthenticated', async () => {
    const signalrService = {
      connect: vi.fn().mockResolvedValue(undefined),
      disconnect: vi.fn().mockResolvedValue(undefined),
      connectionStatus: vi.fn().mockReturnValue('disconnected'),
    } as unknown as Parameters<typeof runSignalrLifecycleStep>[1];
    const appInsightsService = { trackException: vi.fn() };

    await runSignalrLifecycleStep(false, signalrService, appInsightsService);
    expect(signalrService.disconnect).toHaveBeenCalledTimes(1);
    expect(signalrService.connect).not.toHaveBeenCalled();
  });

  it('connects when authenticated and succeeds on the first attempt', async () => {
    const signalrService = {
      connect: vi.fn().mockResolvedValue(undefined),
      disconnect: vi.fn().mockResolvedValue(undefined),
      connectionStatus: vi.fn().mockReturnValue('connected'),
    } as unknown as Parameters<typeof runSignalrLifecycleStep>[1];
    const appInsightsService = { trackException: vi.fn() };

    await runSignalrLifecycleStep(true, signalrService, appInsightsService);
    expect(signalrService.connect).toHaveBeenCalledTimes(1);
    expect(signalrService.disconnect).not.toHaveBeenCalled();
  });

  it('retries connect once when the first attempt leaves status disconnected', async () => {
    vi.useFakeTimers();
    const signalrService = {
      connect: vi.fn().mockResolvedValue(undefined),
      disconnect: vi.fn().mockResolvedValue(undefined),
      // First check: still disconnected (first connect failed). Second check: connected (retry succeeded).
      connectionStatus: vi.fn().mockReturnValueOnce('disconnected').mockReturnValue('connected'),
    } as unknown as Parameters<typeof runSignalrLifecycleStep>[1];
    const appInsightsService = { trackException: vi.fn() };

    const step = runSignalrLifecycleStep(true, signalrService, appInsightsService);
    await vi.runAllTimersAsync();
    await step;

    expect(signalrService.connect).toHaveBeenCalledTimes(2);
    vi.useRealTimers();
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
      storeStore: { refresh: vi.fn() },
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
    expect(signalrService.on).toHaveBeenCalledTimes(1);
    expect(signalrService.on).toHaveBeenCalledWith('EntityChanged', expect.any(Function));
  });

  it('dispatches EntityChanged Store event to storeStore.refresh()', () => {
    const signalrService = { on: vi.fn() };
    const stores = {
      storeStore: { refresh: vi.fn() },
      employeeStore: { handleSignalrEmployeeLifecycleEvent: vi.fn() },
    };
    const appInsightsService = { trackException: vi.fn() };

    runSignalrEventSubscriptionStep(false, 'connected', signalrService, stores, appInsightsService);

    // Simulate the server pushing an EntityChanged event for a Store entity.
    const registeredHandler = signalrService.on.mock.calls[0][1] as (payload: unknown) => void;
    registeredHandler({ entityType: 'Store', entityId: 42, action: 'Updated', userName: 'test', timestamp: '2026-06-07T00:00:00Z' });

    expect(stores.storeStore.refresh).toHaveBeenCalledOnce();
    expect(stores.employeeStore.handleSignalrEmployeeLifecycleEvent).not.toHaveBeenCalled();
  });

  it('dispatches EntityChanged Employee event to employeeStore.handleSignalrEmployeeLifecycleEvent()', () => {
    const signalrService = { on: vi.fn() };
    const stores = {
      storeStore: { refresh: vi.fn() },
      employeeStore: { handleSignalrEmployeeLifecycleEvent: vi.fn() },
    };
    const appInsightsService = { trackException: vi.fn() };

    runSignalrEventSubscriptionStep(false, 'connected', signalrService, stores, appInsightsService);

    const registeredHandler = signalrService.on.mock.calls[0][1] as (payload: unknown) => void;
    registeredHandler({ entityType: 'Employee', entityId: 7, action: 'Created', userName: 'test', timestamp: '2026-06-07T00:00:00Z' });

    expect(stores.employeeStore.handleSignalrEmployeeLifecycleEvent).toHaveBeenCalledOnce();
    expect(stores.storeStore.refresh).not.toHaveBeenCalled();
  });

  it('resets registration on disconnected status', () => {
    const signalrService = { on: vi.fn() };
    const stores = {
      storeStore: { refresh: vi.fn() },
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
      storeStore: { refresh: vi.fn() },
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
