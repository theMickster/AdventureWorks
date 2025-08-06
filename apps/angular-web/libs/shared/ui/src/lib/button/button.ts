import { ChangeDetectionStrategy, Component, computed, input, output, booleanAttribute } from '@angular/core';
@Component({
  selector: 'aw-button',
  templateUrl: './button.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/** Reusable button with DaisyUI variants, sizes, loading/disabled states, and content projection. */
export class ButtonComponent {
  readonly variant = input<'primary' | 'secondary' | 'accent' | 'error' | 'ghost'>('primary');
  readonly size = input<'sm' | 'md' | 'lg'>('md');
  readonly loading = input(false, { transform: booleanAttribute });
  readonly disabled = input(false, { transform: booleanAttribute });
  readonly type = input<'button' | 'submit' | 'reset'>('button');
  readonly clicked = output<MouseEvent>();

  protected readonly buttonClass = computed(() => {
    const classes = ['btn'];
    classes.push(`btn-${this.variant()}`);
    if (this.size() !== 'md') classes.push(`btn-${this.size()}`);
    return classes.join(' ');
  });

  protected readonly isDisabled = computed(() => this.disabled() || this.loading());
}
