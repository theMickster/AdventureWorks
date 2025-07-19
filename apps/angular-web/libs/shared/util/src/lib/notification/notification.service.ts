import { Injectable, signal } from '@angular/core';

/** Visual style of a notification toast. */
export type NotificationType = 'success' | 'error' | 'warning' | 'info';

/** A single notification in the toast queue. */
export interface Notification {
  id: string;
  type: NotificationType;
  message: string;
  autoDismiss: boolean;
}

const AUTO_DISMISS_MS = 5000;

/** Signal-based notification service. Manages a queue of toast messages displayed by ToastContainerComponent. */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  /** Current notifications in the queue. */
  readonly notifications = signal<Notification[]>([]);

  /** Show a success toast (auto-dismisses after 5s). */
  success(message: string): void {
    this.add('success', message, true);
  }

  /** Show an error toast (stays until manually dismissed). */
  error(message: string): void {
    this.add('error', message, false);
  }

  /** Show a warning toast (auto-dismisses after 5s). */
  warning(message: string): void {
    this.add('warning', message, true);
  }

  /** Show an info toast (auto-dismisses after 5s). */
  info(message: string): void {
    this.add('info', message, true);
  }

  /** Remove a notification by ID. */
  dismiss(id: string): void {
    this.notifications.update((list) => list.filter((n) => n.id !== id));
  }

  private add(type: NotificationType, message: string, autoDismiss: boolean): void {
    const id = crypto.randomUUID();
    this.notifications.update((list) => [...list, { id, type, message, autoDismiss }]);

    if (autoDismiss) {
      setTimeout(() => this.dismiss(id), AUTO_DISMISS_MS);
    }
  }
}
