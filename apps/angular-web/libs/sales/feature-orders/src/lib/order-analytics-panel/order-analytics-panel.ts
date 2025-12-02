import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import type { SalesOrderAnalytics } from '@adventureworks-web/sales/data-access';
import { OrderAnalyticsTrendChartComponent } from './order-analytics-trend-chart';

/**
 * Displays aggregated analytics for the currently-filtered sales order slice.
 *
 * Renders three KPI tiles (total revenue, order count, % of total) plus a reactive
 * monthly trend chart. Handles four template states:
 * - **loading/idle**: skeleton placeholder
 * - **error**: error message, no data
 * - **empty**: "no data" message (zero orders or non-finite revenue)
 * - **data**: KPI tiles + `OrderAnalyticsTrendChartComponent`
 *
 * The trend chart is conditionally mounted — it is absent from the DOM when
 * `monthlyTrend` is empty, which also prevents a zero-point Chart.js render.
 */
@Component({
  selector: 'aw-order-analytics-panel',
  standalone: true,
  imports: [CurrencyPipe, DecimalPipe, OrderAnalyticsTrendChartComponent],
  templateUrl: './order-analytics-panel.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderAnalyticsPanelComponent {
  /** Aggregated analytics payload from the store. Null while loading or on error. */
  readonly analytics = input<SalesOrderAnalytics | null>();
  /** Request lifecycle status that drives template state selection. */
  readonly status = input<'idle' | 'loading' | 'loaded' | 'error'>();

  protected readonly isLoading = computed(() => this.status() === 'loading' || this.status() === 'idle');
  protected readonly isEmpty = computed(() => {
    const a = this.analytics();
    return !a || a.orderCount === 0 || !Number.isFinite(a.totalRevenue);
  });
  protected readonly hasError = computed(() => this.status() === 'error');
}
