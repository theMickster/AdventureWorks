import { computed, Provider, signal, WritableSignal } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { AuthUser } from '../auth/auth.model';

/** Shape of the mock returned by {@link provideMockAuthService}, exposing writable signals for test control. */
export interface MockAuthService {
  isAuthenticated: WritableSignal<boolean>;
  user: WritableSignal<AuthUser | null>;
  displayName: ReturnType<typeof computed<string>>;
  userInitials: ReturnType<typeof computed<string>>;
  login: () => void;
  logout: () => void;
  initialize: () => void;
}

/**
 * Test-only mock provider for {@link AuthService}. `displayName`/`userInitials` are real `computed()`
 * signals derived from the mock's own `user` signal — mirroring the real service — so tests that mutate
 * `user` observe reactive updates rather than stale static values.
 */
export function provideMockAuthService(overrides?: Partial<MockAuthService>): Provider {
  const isAuthenticated = overrides?.isAuthenticated ?? signal(false);
  const user = overrides?.user ?? signal<AuthUser | null>(null);

  const mock: MockAuthService = {
    isAuthenticated,
    user,
    displayName: computed(() => user()?.name ?? ''),
    userInitials: computed(() => {
      const name = user()?.name;
      if (!name) return '';
      const parts = name.trim().split(/\s+/);
      const first = parts[0]?.[0] ?? '';
      const last = parts.length > 1 ? parts[parts.length - 1][0] : '';
      return (first + last).toUpperCase();
    }),
    login: () => undefined,
    logout: () => undefined,
    initialize: () => undefined,
    ...overrides,
  };

  return { provide: AuthService, useValue: mock };
}
