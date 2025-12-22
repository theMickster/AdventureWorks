import { afterNextRender, ChangeDetectionStrategy, Component, computed, ElementRef, input, OnDestroy, viewChild } from '@angular/core';
import { BarController, BarElement, CategoryScale, Chart, LinearScale, Tooltip } from 'chart.js';
import type { DepartmentHeadcountSummary } from '@adventureworks-web/hr/data-access';

Chart.register(BarController, BarElement, LinearScale, CategoryScale, Tooltip);

/** Brand primary (Alpine Circuit `--ac-primary` / DaisyUI `primary`), matching `TrendChartComponent`'s hardcoded hex convention. */
const BAR_COLOR = '#0891b2';

/**
 * Renders a Chart.js horizontal bar chart of active headcount per department.
 *
 * One-shot render, mirroring `TrendChartComponent`: the chart is built in
 * `afterNextRender` from the `data` input at mount time and is not reactive
 * after that — the host `@if`/`@else` block destroys and recreates this
 * component on every dashboard load/refresh, which is how new data reaches it.
 */
@Component({
  selector: 'aw-headcount-chart',
  standalone: true,
  imports: [],
  templateUrl: './headcount-chart.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeadcountChartComponent implements OnDestroy {
  readonly data = input.required<DepartmentHeadcountSummary[]>();
  /** Container height scales with department count so each horizontal bar keeps a readable row height. */
  protected readonly chartHeight = computed(() => Math.max(320, this.data().length * 32));
  /**
   * Text alternative for screen reader users — Chart.js renders to a bare `<canvas>` with no
   * accessible content of its own. Summarizes the same data the bars encode.
   */
  protected readonly ariaLabel = computed(() => {
    const departments = this.data();
    const summary = departments.map((d) => `${d.departmentName}: ${d.activeEmployeeCount}`).join(', ');
    return `Bar chart of active employee headcount by department. ${summary}`;
  });
  private readonly chartCanvas = viewChild.required<ElementRef<HTMLCanvasElement>>('chartCanvas');
  private chart: Chart | null = null;

  constructor() {
    afterNextRender(() => {
      const departments = this.data();
      const labels = departments.map((d) => d.departmentName);
      const counts = departments.map((d) => d.activeEmployeeCount);

      this.chart = new Chart(this.chartCanvas().nativeElement, {
        type: 'bar',
        data: {
          labels,
          datasets: [
            {
              label: 'Active employees',
              data: counts,
              backgroundColor: BAR_COLOR,
            },
          ],
        },
        options: {
          indexAxis: 'y',
          responsive: true,
          maintainAspectRatio: false,
          scales: {
            x: {
              beginAtZero: true,
              ticks: { precision: 0 },
            },
          },
          plugins: {
            legend: { display: false },
            tooltip: {
              callbacks: {
                label: (ctx) => `${ctx.parsed.x} active employee${ctx.parsed.x === 1 ? '' : 's'}`,
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
