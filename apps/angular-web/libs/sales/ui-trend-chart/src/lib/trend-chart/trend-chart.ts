import { afterNextRender, ChangeDetectionStrategy, Component, ElementRef, input, OnDestroy, output, viewChild } from '@angular/core';
import { type ActiveElement, type ChartEvent, Chart, CategoryScale, LinearScale, LineController, LineElement, PointElement, Tooltip } from 'chart.js';

Chart.register(LineController, LineElement, PointElement, LinearScale, CategoryScale, Tooltip);

/** Structurally identical to `DashboardMonthlySalesTrend` (sales/data-access) — duplicated here so this `type:ui` library doesn't depend on `type:data-access`. */
export interface TrendChartDataPoint {
  readonly year: number;
  readonly month: number;
  readonly revenue: number;
  readonly orderCount: number;
}

/**
 * Renders a Chart.js line chart of monthly revenue for the trailing 24 months.
 *
 * The chart is a one-shot render driven by the `data` input at the time the
 * component first lands in the DOM. Changing the input after initial render
 * does not update the chart — the dashboard loads data once and never
 * re-triggers this component.
 */
@Component({
  selector: 'aw-trend-chart',
  standalone: true,
  imports: [],
  templateUrl: './trend-chart.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TrendChartComponent implements OnDestroy {
  readonly data = input.required<TrendChartDataPoint[]>();
  /** Emits the year and month of the clicked data point so the host can navigate to a filtered view. */
  readonly dataPointClick = output<{ year: number; month: number }>();
  private readonly chartCanvas = viewChild.required<ElementRef<HTMLCanvasElement>>('chartCanvas');
  /**
   * Stored so `ngOnDestroy` can call `chart.destroy()` and release the canvas
   * WebGL/2D context. Without explicit destroy, Chart.js holds a reference to
   * the canvas element and leaks GPU/memory resources when the component is
   * removed from the DOM.
   */
  private chart: Chart | null = null;

  constructor() {
    /**
     * `afterNextRender` fires once after the first DOM render in the current
     * scheduler tick — the earliest safe moment to read `viewChild` and
     * manipulate the canvas. Using `ngAfterViewInit` is unsafe in zoneless
     * apps because Angular does not guarantee change detection runs
     * synchronously after lifecycle hooks without Zone.js.
     */
    afterNextRender(() => {
      const points = this.data();
      const labels = points.map(
        (p) => new Date(p.year, p.month - 1).toLocaleDateString('en-US', { month: 'short', year: 'numeric' })
      );
      const revenues = points.map((p) => p.revenue);
      const formatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });

      this.chart = new Chart(this.chartCanvas().nativeElement, {
        type: 'line',
        data: {
          labels,
          datasets: [
            {
              data: revenues,
              borderColor: '#0891b2',
              backgroundColor: 'rgba(8, 145, 178, 0.1)',
              tension: 0.3,
              fill: true,
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          onClick: (_event: ChartEvent, elements: ActiveElement[]) => {
            if (elements.length === 0) { return; }
            const point = points[elements[0].index];
            this.dataPointClick.emit({ year: point.year, month: point.month });
          },
          // Chart.js does not auto-set cursor; onHover drives the pointer/default switch.
          onHover: (_event: ChartEvent, elements: ActiveElement[]) => {
            this.chartCanvas().nativeElement.style.cursor = elements.length > 0 ? 'pointer' : 'default';
          },
          plugins: {
            tooltip: {
              callbacks: {
                label: (ctx) => (ctx.parsed.y === null ? '' : formatter.format(ctx.parsed.y)),
              },
            },
          },
        },
      });
    });
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    this.chart = null;
  }
}
