import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'aw-shared-ui',
  imports: [],
  templateUrl: './shared-ui.html',
  styleUrl: './shared-ui.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SharedUiComponent {}
