import { ChangeDetectionStrategy, Component, booleanAttribute, computed, input } from '@angular/core';

/**
 * Maps a DaisyUI semantic color token to its full `bg-*` class. Written as a literal lookup, not
 * string concatenation, so Tailwind's static-text scanner can see every generated class name.
 */
const NODE_VARIANT_CLASSES: Record<string, string> = {
  primary: 'bg-primary',
  secondary: 'bg-secondary',
  accent: 'bg-accent',
  info: 'bg-info',
  success: 'bg-success',
  warning: 'bg-warning',
  error: 'bg-error',
};

/**
 * Shared "vertical line + node + card" presentational primitive for `DepartmentTimelineComponent`
 * and `CombinedTimelineComponent`. Internal — not exported from the lib's `index.ts`.
 */
@Component({
  selector: 'aw-timeline-entry',
  standalone: true,
  templateUrl: './timeline-entry.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TimelineEntryComponent {
  readonly fieldId = input('aw-timeline-entry');
  readonly icon = input.required<string>();
  /** DaisyUI semantic color token for the node, e.g. 'primary', 'accent', 'success'. */
  readonly variant = input<string>('primary');
  readonly title = input.required<string>();
  readonly subtitle = input<string>('');
  readonly muted = input(false, { transform: booleanAttribute });
  /** Omits the connecting line below this entry — set true for the final entry in a list. */
  readonly isLast = input(false, { transform: booleanAttribute });

  protected readonly nodeClass = computed(() => NODE_VARIANT_CLASSES[this.variant()] ?? 'bg-primary');
}
