import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'aw-empty-state',
  imports: [TranslatePipe],
  templateUrl: './empty-state.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/** Centered placeholder for empty lists/tables with icon, title, description, and optional CTA via content projection. */
export class EmptyStateComponent {
  readonly fieldId = input('aw-empty-state');
  readonly icon = input('fa-solid fa-inbox');
  readonly title = input.required<string>();
  readonly description = input('');
}
