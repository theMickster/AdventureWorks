import { Provider } from '@angular/core';
import { ENVIRONMENT } from '../environment/environment.token';
import { Environment } from '../environment/environment.model';

/** Test-only mock provider for {@link ENVIRONMENT}. Matches the literal duplicated across existing specs. */
export function provideMockEnvironment(overrides?: Partial<Environment>): Provider {
  const environment: Environment = {
    production: false,
    defaultLocale: 'en',
    api: {
      primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
    },
    ...overrides,
  };

  return { provide: ENVIRONMENT, useValue: environment };
}
