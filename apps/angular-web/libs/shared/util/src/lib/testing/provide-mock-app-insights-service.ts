import { Provider } from '@angular/core';
import { AppInsightsService } from '../telemetry/app-insights.service';

/** Shape of the mock returned by {@link provideMockAppInsightsService}. */
export interface MockAppInsightsService {
  initialize: () => Promise<void>;
  trackEvent: (name: string, properties?: Record<string, string>) => void;
  trackException: (error: Error) => void;
}

/** Test-only mock provider for {@link AppInsightsService}. */
export function provideMockAppInsightsService(overrides?: Partial<MockAppInsightsService>): Provider {
  const mock: MockAppInsightsService = {
    initialize: async () => undefined,
    trackEvent: () => undefined,
    trackException: () => undefined,
    ...overrides,
  };

  return { provide: AppInsightsService, useValue: mock };
}
