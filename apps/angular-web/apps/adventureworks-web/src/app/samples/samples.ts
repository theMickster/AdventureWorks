import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { LookupStore } from '@adventureworks-web/shared/data-access';

@Component({
  selector: 'aw-samples',
  standalone: true,
  templateUrl: './samples.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SamplesComponent {
  protected readonly lookupStore = inject(LookupStore);

  constructor() {
    this.lookupStore.loadDepartments();
  }
}
