import { Provider, signal, WritableSignal } from '@angular/core';
import { LoadingService } from '../loading/loading.service';

/** Shape of the mock returned by {@link provideMockLoadingService}. */
export interface MockLoadingService {
  isLoading: WritableSignal<boolean>;
  start: () => void;
  stop: () => void;
}

/** Test-only mock provider for {@link LoadingService}. */
export function provideMockLoadingService(overrides?: Partial<MockLoadingService>): Provider {
  const mock: MockLoadingService = {
    isLoading: signal(false),
    start: () => undefined,
    stop: () => undefined,
    ...overrides,
  };

  return { provide: LoadingService, useValue: mock };
}
