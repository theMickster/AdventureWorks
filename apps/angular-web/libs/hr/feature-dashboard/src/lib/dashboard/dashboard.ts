import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, OnInit, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { interval } from 'rxjs';
import { SkeletonComponent } from '@adventureworks-web/shared/ui';
import { HrDashboardStore } from '../stores/hr-dashboard.store';
import { HeadcountChartComponent } from '../headcount-chart/headcount-chart';
import { TenureChartComponent } from '../tenure-chart/tenure-chart';
import { formatRelativeTime } from '../utils/format-relative-time';

/** How often the "Last updated" label re-renders relative to `lastUpdated` while the dashboard stays open. */
const RELATIVE_TIME_TICK_MS = 30_000;

/**
 * HR dashboard at `/hr/dashboard`: headcount/status stat cards, a department headcount bar chart,
 * a tenure distribution chart, and a pay-band summary, with manual refresh (US-765/766/767).
 *
 * Deviates from the Sales dashboard in two deliberate ways:
 * - **Inline retry alert, not a toast** — a load failure renders an `alert alert-error` block with
 *   a Retry button, per US-765's AC. The Sales dashboard's toast-only error pattern isn't used.
 * - **Manual refresh** — `HrDashboardStore.load()` is callable repeatedly (US-767); the Sales
 *   `DashboardStore` has no refresh trigger by design.
 */
@Component({
  selector: 'aw-hr-dashboard',
  standalone: true,
  imports: [CurrencyPipe, DecimalPipe, SkeletonComponent, HeadcountChartComponent, TenureChartComponent],
  templateUrl: './dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HrDashboardComponent implements OnInit {
  private readonly store = inject(HrDashboardStore);
  // Ticks every RELATIVE_TIME_TICK_MS so the "Last updated" label stays accurate ("2 minutes ago"
  // becomes "3 minutes ago") without requiring a manual refresh or page reload.
  private readonly relativeTimeTick = toSignal(interval(RELATIVE_TIME_TICK_MS), { initialValue: 0 });

  protected readonly isLoading = this.store.isLoading;
  protected readonly hasError = this.store.hasError;
  protected readonly aggregates = this.store.aggregates;
  protected readonly lastUpdatedLabel = computed(() => {
    this.relativeTimeTick();
    const lastUpdated = this.store.lastUpdated();
    return lastUpdated ? formatRelativeTime(lastUpdated) : null;
  });

  ngOnInit(): void {
    this.store.load();
  }

  protected onRefreshClick(): void {
    this.store.load();
  }

  protected onRetryClick(): void {
    this.store.load();
  }
}
