import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'aw-samples',
  standalone: true,
  templateUrl: './samples.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SamplesComponent {}
