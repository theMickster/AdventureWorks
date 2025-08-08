import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'aw-placeholder',
  template: `<h1 class="text-2xl font-bold text-base-content">{{ breadcrumb() }}</h1>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaceholderComponent {
  readonly breadcrumb = input('Page');
}
