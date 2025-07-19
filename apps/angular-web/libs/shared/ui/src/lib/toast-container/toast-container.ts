import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NotificationService, NotificationType } from '@adventureworks-web/shared/util';

/** Maps notification types to DaisyUI alert classes. */
const ALERT_CLASS_MAP: Record<NotificationType, string> = {
  success: 'alert-success',
  error: 'alert-error',
  warning: 'alert-warning',
  info: 'alert-info',
};

/** Maps notification types to Font Awesome icon classes. */
const ICON_CLASS_MAP: Record<NotificationType, string> = {
  success: 'fa-solid fa-circle-check',
  error: 'fa-solid fa-circle-xmark',
  warning: 'fa-solid fa-triangle-exclamation',
  info: 'fa-solid fa-circle-info',
};

/** Renders toast notifications from NotificationService in the bottom-right corner. */
@Component({
  selector: 'aw-toast-container',
  templateUrl: './toast-container.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastContainerComponent {
  protected readonly notificationService = inject(NotificationService);

  protected alertClass(type: NotificationType): string {
    return ALERT_CLASS_MAP[type];
  }

  protected iconClass(type: NotificationType): string {
    return ICON_CLASS_MAP[type];
  }
}
