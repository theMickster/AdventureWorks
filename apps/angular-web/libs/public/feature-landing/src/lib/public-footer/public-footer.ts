import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'aw-public-footer',
  imports: [],
  templateUrl: './public-footer.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicFooterComponent {
  protected readonly currentYear = new Date().getFullYear();
}
