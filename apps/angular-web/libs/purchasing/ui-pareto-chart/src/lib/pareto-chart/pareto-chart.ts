import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  input,
  OnDestroy,
  output,
  viewChild,
} from '@angular/core';
import {
  type ActiveElement,
  type ChartConfiguration,
  type ChartEvent,
  type Plugin,
  BarController,
  BarElement,
  CategoryScale,
  Chart,
  LinearScale,
  LineController,
  LineElement,
  PointElement,
  Tooltip,
} from 'chart.js';
import { EmptyStateComponent } from '@adventureworks-web/shared/ui';

/** The chart type union for the mixed bar (spend) + line (cumulative %) Pareto chart. */
type ParetoChartType = 'bar' | 'line';

/** The two cumulative-percent thresholds the reference lines are drawn at. */
const REFERENCE_LINE_VALUES = [80, 95] as const;

/**
 * Draws horizontal reference lines on the right-hand cumulative-percent axis (`y1`) at the 80%
 * and 95% thresholds — the classic Pareto read lines ("which vendors make up 80% of spend?").
 *
 * Hand-rolled as a plain canvas-2D plugin rather than pulling in `chartjs-plugin-annotation`:
 * two straight lines are not worth a new npm dependency.
 */
const paretoReferenceLinesPlugin: Plugin<ParetoChartType> = {
  id: 'paretoReferenceLines',
  afterDatasetsDraw(chart) {
    const scale = chart.scales['y1'];
    if (!scale) {
      return;
    }

    const { ctx, chartArea } = chart;
    ctx.save();
    ctx.lineWidth = 1;
    // Raw hex is unavoidable in a canvas context — DaisyUI/Tailwind classes don't apply here.
    // Matches the literal-color precedent set by TrendChartComponent's Chart.js config.
    ctx.strokeStyle = '#64748b';
    ctx.setLineDash([6, 4]);

    for (const value of REFERENCE_LINE_VALUES) {
      const y = scale.getPixelForValue(value);
      ctx.beginPath();
      ctx.moveTo(chartArea.left, y);
      ctx.lineTo(chartArea.right, y);
      ctx.stroke();
    }

    ctx.restore();
  },
};

Chart.register(BarController, BarElement, LineController, LineElement, PointElement, LinearScale, CategoryScale, Tooltip);

/** Structurally identical to `VendorSpendDto` (purchasing/data-access) — duplicated here so this `type:ui` library doesn't depend on `type:data-access`. */
export interface ParetoChartDataPoint {
  readonly vendorId: number;
  readonly vendorName: string;
  readonly totalSpend: number;
  readonly cumulativePercent: number;
}

/**
 * Renders a Pareto chart of vendor spend: a bar per vendor's total spend on the left axis, and a
 * cumulative-percent-of-total-spend line on the right axis, with 80% / 95% reference lines.
 *
 * The chart is a one-shot render driven by the `paretoData` input at the time the component first
 * lands in the DOM. Changing the input after initial render does not update the chart — the
 * analytics page loads data once in `ngOnInit` and never refetches.
 */
@Component({
  selector: 'aw-pareto-chart',
  standalone: true,
  imports: [EmptyStateComponent],
  templateUrl: './pareto-chart.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParetoChartComponent implements OnDestroy {
  /** Every vendor, spend-descending. Nullable so the host can bind straight from an unresolved fetch. */
  readonly paretoData = input<ParetoChartDataPoint[] | null>(null);
  /** Emits the clicked vendor's `vendorId` so the host can navigate to that vendor's detail page. */
  readonly vendorClick = output<number>();

  protected readonly hasData = computed(() => (this.paretoData()?.length ?? 0) > 0);

  private readonly chartCanvas = viewChild<ElementRef<HTMLCanvasElement>>('chartCanvas');
  /** Stored so `ngOnDestroy` can release the canvas 2D context Chart.js holds a reference to. */
  private chart: Chart<ParetoChartType, number[], string> | null = null;
  /**
   * Chart x-axis index -> `vendorId`, built once from the same array that produced the chart
   * labels. Resolving a click through this array decouples "which bar was clicked" from any
   * Chart.js internal dataset state.
   */
  private vendorIdsByIndex: number[] = [];

  constructor() {
    /**
     * `afterNextRender` fires once after the first DOM render — the earliest safe moment to read
     * `viewChild` and touch the canvas. `ngAfterViewInit` is unsafe in zoneless apps because
     * Angular does not guarantee change detection has run by then.
     */
    afterNextRender(() => {
      const vendors = this.paretoData();
      const canvas = this.chartCanvas()?.nativeElement;
      if (!vendors || vendors.length === 0 || !canvas) {
        return;
      }

      this.vendorIdsByIndex = vendors.map((v) => v.vendorId);
      const labels = vendors.map((v) => v.vendorName);
      const currencyFormatter = new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
        maximumFractionDigits: 0,
      });

      const config: ChartConfiguration<ParetoChartType, number[], string> = {
        type: 'bar',
        plugins: [paretoReferenceLinesPlugin],
        data: {
          labels,
          datasets: [
            {
              type: 'bar',
              label: 'Total Spend',
              data: vendors.map((v) => v.totalSpend),
              yAxisID: 'y',
              backgroundColor: 'rgba(8, 145, 178, 0.7)',
              borderColor: '#0891b2',
              order: 2,
            },
            {
              type: 'line',
              label: 'Cumulative %',
              data: vendors.map((v) => v.cumulativePercent),
              yAxisID: 'y1',
              borderColor: '#dc2626',
              backgroundColor: 'rgba(220, 38, 38, 0.1)',
              pointRadius: 0,
              tension: 0.2,
              order: 1,
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          onClick: (event, _elements, chart) => {
            this.handleChartClick(event, chart);
          },
          // Chart.js does not auto-set cursor; onHover drives the pointer/default switch.
          onHover: (_event: ChartEvent, elements: ActiveElement[]) => {
            canvas.style.cursor = elements.length > 0 ? 'pointer' : 'default';
          },
          scales: {
            x: {
              ticks: { autoSkip: true, maxRotation: 90, minRotation: 45 },
            },
            y: {
              type: 'linear',
              position: 'left',
              beginAtZero: true,
              ticks: {
                callback: (value) => currencyFormatter.format(Number(value)),
              },
            },
            y1: {
              type: 'linear',
              position: 'right',
              min: 0,
              max: 100,
              grid: { drawOnChartArea: false },
              ticks: {
                callback: (value) => `${Number(value)}%`,
              },
            },
          },
          plugins: {
            tooltip: {
              callbacks: {
                label: (ctx) => {
                  const raw = ctx.parsed.y;
                  if (raw === null) {
                    return '';
                  }
                  return ctx.dataset.yAxisID === 'y1'
                    ? `Cumulative: ${raw.toFixed(1)}%`
                    : `Spend: ${currencyFormatter.format(raw)}`;
                },
              },
            },
          },
        },
      };

      this.chart = new Chart(canvas, config);
    });
  }

  /**
   * Resolves the clicked chart element back to a vendor and emits it. Uses
   * `getElementsAtEventForMode` in `'index'` mode so a click anywhere in a category column hits
   * that vendor, then maps the element's index through `vendorIdsByIndex`.
   */
  protected handleChartClick(event: ChartEvent, chart: Chart): void {
    const nativeEvent = event.native;
    if (!nativeEvent) {
      return;
    }

    const elements = chart.getElementsAtEventForMode(nativeEvent, 'index', { intersect: true }, false);
    if (elements.length === 0) {
      return;
    }

    this.emitVendorAtIndex(elements[0].index);
  }

  /** Emits the `vendorId` at the given chart x-axis index; a no-op for an out-of-range index. */
  protected emitVendorAtIndex(index: number): void {
    const vendorId = this.vendorIdsByIndex[index];
    if (vendorId === undefined) {
      return;
    }

    this.vendorClick.emit(vendorId);
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    this.chart = null;
  }
}
