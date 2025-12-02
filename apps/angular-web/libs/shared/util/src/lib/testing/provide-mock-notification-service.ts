import { Provider, signal, WritableSignal } from '@angular/core';
import { Notification, NotificationService } from '../notification/notification.service';

/** Shape of the mock returned by {@link provideMockNotificationService}. */
export interface MockNotificationService {
  notifications: WritableSignal<Notification[]>;
  success: (message: string) => void;
  error: (message: string) => void;
  warning: (message: string) => void;
  info: (message: string) => void;
  dismiss: (id: string) => void;
}

/** Test-only mock provider for {@link NotificationService}. */
export function provideMockNotificationService(overrides?: Partial<MockNotificationService>): Provider {
  const mock: MockNotificationService = {
    notifications: signal<Notification[]>([]),
    success: () => undefined,
    error: () => undefined,
    warning: () => undefined,
    info: () => undefined,
    dismiss: () => undefined,
    ...overrides,
  };

  return { provide: NotificationService, useValue: mock };
}
