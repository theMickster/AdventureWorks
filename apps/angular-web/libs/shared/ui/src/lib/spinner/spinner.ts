import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/** Inline loading spinner using DaisyUI loading classes. */
@Component({
  selector: 'aw-spinner',
  template: `<span [id]="id()" class="loading loading-spinner" [class]="sizeClass()" aria-label="Loading"></span>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpinnerComponent {
  /** Element ID for testing and debugging. */
  readonly id = input('aw-spinner');
  /** Spinner size variant. */
  readonly size = input<'xs' | 'sm' | 'md' | 'lg'>('md');

  protected readonly sizeClass = computed(() => 'loading-' + this.size());
}
