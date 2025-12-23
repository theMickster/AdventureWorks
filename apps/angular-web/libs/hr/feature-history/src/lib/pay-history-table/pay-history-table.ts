import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import type { EmployeePayHistory } from '@adventureworks-web/hr/data-access';
import { ColumnDefDirective, DataTableComponent, StatusBadgeComponent } from '@adventureworks-web/shared/ui';
import type { ColumnConfig } from '@adventureworks-web/shared/ui';
import { computePayDeltas } from '../combined-timeline/build-combined-timeline';

const COLUMNS: ColumnConfig[] = [
  { key: 'rateChangeDate', label: 'Effective Date' },
  { key: 'rate', label: 'Rate', headerClass: 'text-right', cellClass: 'text-right' },
  { key: 'delta', label: 'Change', headerClass: 'text-right', cellClass: 'text-right' },
  { key: 'payFrequencyLabel', label: 'Pay Frequency' },
];

const INITIAL_BADGE_MAP: Record<string, string> = { initial: 'badge-outline' };

/** Pay rate change history table (US-769). */
@Component({
  selector: 'aw-pay-history-table',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, DataTableComponent, ColumnDefDirective, StatusBadgeComponent],
  templateUrl: './pay-history-table.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PayHistoryTableComponent {
  readonly payHistory = input.required<EmployeePayHistory[]>();

  protected readonly columns = COLUMNS;
  protected readonly initialBadgeMap = INITIAL_BADGE_MAP;

  /** Rows, most-recent-first, each paired with its computed delta versus the prior entry. */
  protected readonly rows = computed(() => {
    const history = this.payHistory();
    const deltas = computePayDeltas(history);
    return history
      .map((entry, i) => ({ ...entry, delta: deltas[i] }))
      .sort((a, b) => b.rateChangeDate.localeCompare(a.rateChangeDate));
  });
}
