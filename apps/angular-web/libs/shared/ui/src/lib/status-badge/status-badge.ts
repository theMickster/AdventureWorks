import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

const DEFAULT_STATUS_MAP: Record<string, string> = {
  active: 'badge-success',
  inactive: 'badge-secondary',
  pending: 'badge-warning',
  error: 'badge-error',
  complete: 'badge-success',
  cancelled: 'badge-secondary',
  draft: 'badge-info',
};

@Component({
  selector: 'aw-status-badge',
  imports: [TranslatePipe],
  template: `<span [id]="fieldId()" [class]="badgeClass()">{{ status() | translate }}</span>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/** Inline badge that maps status strings to DaisyUI badge variants. Supports custom status-to-class mapping. */
export class StatusBadgeComponent {
  readonly fieldId = input('aw-status-badge');
  readonly status = input.required<string>();
  readonly statusMap = input<Record<string, string>>({});

  protected readonly badgeClass = computed(() => {
    const status = this.status();
    const customMap = this.statusMap();
    const variant = customMap[status] ?? DEFAULT_STATUS_MAP[status] ?? 'badge-outline';
    return `badge ${variant}`;
  });
}
