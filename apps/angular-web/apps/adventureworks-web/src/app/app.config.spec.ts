import { describe, expect, it, vi } from 'vitest';
import { runSignalrLifecycleStep } from './app.config';

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
