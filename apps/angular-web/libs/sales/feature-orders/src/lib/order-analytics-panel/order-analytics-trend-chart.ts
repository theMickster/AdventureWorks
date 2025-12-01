import { afterNextRender, ChangeDetectionStrategy, Component, effect, ElementRef, input, OnDestroy, viewChild } from '@angular/core';
import { Chart, CategoryScale, LinearScale, LineController, LineElement, PointElement, Tooltip } from 'chart.js';
import type { SalesOrderMonthlyTrend } from '@adventureworks-web/sales/data-access';

Chart.register(LineController, LineElement, PointElement, LinearScale, CategoryScale, Tooltip);

/**
 * Reactive Chart.js line chart of monthly revenue for a filtered analytics slice.
 *
 * Unlike TrendChartComponent (dashboard, one-shot), this chart reacts to filter changes.
 * An effect() watching monthlyTrend() destroys and recreates the chart whenever the
 * input changes, so the panel stays in sync with the active filter.
 *
 * Partial months (isPartialMonth === true) use reduced point opacity to signal
 * that the revenue total represents an incomplete calendar month.
 */
@Component({
  selector: 'aw-order-analytics-trend-chart',
  standalone: true,
  imports: [],
  templateUrl: './order-analytics-trend-chart.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderAnalyticsTrendChartComponent implements OnDestroy {
  /**
   * Ordered array of monthly revenue data points. Each change triggers a full chart
   * destroy-and-recreate cycle. Points where `isPartialMonth` is true render at
   * reduced opacity and append "(Partial month)" to the tooltip label.
   */
  readonly monthlyTrend = input.required<SalesOrderMonthlyTrend[]>();
  private readonly chartCanvas = viewChild.required<ElementRef<HTMLCanvasElement>>('chartCanvas');
  private chart: Chart | null = null;
  private isRendered = false;

  private readonly formatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });

  constructor() {
    afterNextRender(() => {
      this.isRendered = true;
      this.buildChart(this.monthlyTrend());
    });

    effect(() => {
      const points = this.monthlyTrend();
      if (!this.isRendered) {
        return;
      }
      this.buildChart(points);
    });
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    this.chart = null;
  }

  private buildChart(points: SalesOrderMonthlyTrend[]): void {
    this.chart?.destroy();
    this.chart = null;

    const labels = points.map(
      (p) => new Date(p.year, p.month - 1).toLocaleDateString('en-US', { month: 'short', year: 'numeric' }),
    );
    const revenues = points.map((p) => p.revenue);
    const pointColors = points.map((p) => (p.isPartialMonth ? 'rgba(8,145,178,0.4)' : '#0891b2'));
    const formatter = this.formatter;

    this.chart = new Chart(this.chartCanvas().nativeElement, {
      type: 'line',
      data: {
        labels,
        datasets: [
          {
            data: revenues,
            borderColor: '#0891b2',
            backgroundColor: 'rgba(8, 145, 178, 0.1)',
            pointBackgroundColor: pointColors,
            tension: 0.3,
            fill: true,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          tooltip: {
            callbacks: {
              label: (ctx) => {
                if (ctx.parsed.y === null) {
                  return '';
                }
                const label = formatter.format(ctx.parsed.y);
                const point = points[ctx.dataIndex];
                return point?.isPartialMonth ? `${label} (Partial month)` : label;
              },
            },
          },
        },
      },
    });
  }
}
