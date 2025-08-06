import { ChangeDetectionStrategy, Component, input, booleanAttribute } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'aw-card',
  imports: [TranslatePipe],
  templateUrl: './card.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/** Card container with DaisyUI styling, optional title, content projection slots, and skeleton loading state. */
export class CardComponent {
  readonly fieldId = input('aw-card');
  readonly title = input('');
  readonly loading = input(false, { transform: booleanAttribute });
}
