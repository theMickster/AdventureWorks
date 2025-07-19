import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** Content placeholder block using DaisyUI skeleton animation. */
@Component({
  selector: 'aw-skeleton',
  template: `<div
    [id]="id()"
    class="skeleton"
    [style.width]="width()"
    [style.height]="height()"
    aria-hidden="true"
  ></div>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SkeletonComponent {
  /** Element ID for testing and debugging. */
  readonly id = input('aw-skeleton');
  /** Width of the skeleton block (CSS value). */
  readonly width = input('100%');
  /** Height of the skeleton block (CSS value). */
  readonly height = input('1rem');
}
