import { afterNextRender, ChangeDetectionStrategy, Component, computed, ElementRef, input, OnDestroy, viewChild } from '@angular/core';
import { BarController, BarElement, CategoryScale, Chart, LinearScale, Tooltip } from 'chart.js';
import type { TenureDistribution } from '@adventureworks-web/hr/data-access';

// Legend plugin intentionally not registered — the DOM legend in tenure-chart.html replaces it (see bucketSummary()).
Chart.register(BarController, BarElement, LinearScale, CategoryScale, Tooltip);

/** Five tenure-bucket labels, in bucket order — shared between the chart legend, tooltip, and datasets. */
const TENURE_BUCKET_LABELS = ['0-1 yr', '1-3 yrs', '3-5 yrs', '5-10 yrs', '10+ yrs'] as const;

/**
 * Five brand-consistent, visually distinct colors (Alpine Circuit semantic hexes), one per tenure
 * bucket — matching `TrendChartComponent`/`HeadcountChartComponent`'s hardcoded-hex convention.
 */
const TENURE_BUCKET_COLORS = ['#0891b2', '#14b8a6', '#d97706', '#64748b', '#059669'] as const;

/**
 * Renders a single-row Chart.js horizontal stacked bar segmenting active employees into 5 tenure
 * buckets, with a legend.
 *
 * One-shot render, mirroring `TrendChartComponent`/`HeadcountChartComponent`: built in
 * `afterNextRender` from the `data` input at mount time; the host `@if`/`@else` block destroys
 * and recreates this component on every dashboard load/refresh.
 */
@Component({
  selector: 'aw-tenure-chart',
  standalone: true,
  imports: [],
  templateUrl: './tenure-chart.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TenureChartComponent implements OnDestroy {
  readonly data = input.required<TenureDistribution>();
  private readonly chartCanvas = viewChild.required<ElementRef<HTMLCanvasElement>>('chartCanvas');
  private chart: Chart | null = null;

  /**
   * Real DOM legend, rendered below the canvas — Chart.js's built-in legend plugin draws directly
   * onto canvas pixels, which is invisible to screen readers. This computed feeds both the visible
   * DOM legend markup and the canvas's `aria-label` text alternative, and is the single source of
   * truth for bucket label/color/value order.
   */
  protected readonly bucketSummary = computed(() => {
    const distribution = this.data();
    const values = [
      distribution.underOneYear,
      distribution.oneToThreeYears,
      distribution.threeToFiveYears,
      distribution.fiveToTenYears,
      distribution.tenPlusYears,
    ];

    return TENURE_BUCKET_LABELS.map((label, i) => ({
      label,
      color: TENURE_BUCKET_COLORS[i],
      value: values[i],
    }));
  });

  protected readonly ariaLabel = computed(() => {
    const summary = this.bucketSummary()
      .map((b) => `${b.label}: ${b.value}`)
      .join(', ');
    return `Stacked bar chart of active employee tenure distribution. ${summary}`;
  });

  constructor() {
    afterNextRender(() => {
      const buckets = this.bucketSummary();

      this.chart = new Chart(this.chartCanvas().nativeElement, {
        type: 'bar',
        data: {
          labels: ['Active employees'],
          datasets: buckets.map((bucket) => ({
            label: bucket.label,
            data: [bucket.value],
            backgroundColor: bucket.color,
          })),
        },
        options: {
          indexAxis: 'y',
          responsive: true,
          maintainAspectRatio: false,
          scales: {
            x: { stacked: true, beginAtZero: true, ticks: { precision: 0 } },
            y: { stacked: true },
          },
          plugins: {
            // Chart.js's built-in legend draws directly onto canvas pixels — invisible to screen
            // readers. Disabled here in favor of the real DOM legend in tenure-chart.html, driven
            // by the same bucketSummary() computed.
            legend: { display: false },
            tooltip: {
              callbacks: {
                label: (ctx) => `${ctx.dataset.label}: ${ctx.parsed.x} employee${ctx.parsed.x === 1 ? '' : 's'}`,
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
