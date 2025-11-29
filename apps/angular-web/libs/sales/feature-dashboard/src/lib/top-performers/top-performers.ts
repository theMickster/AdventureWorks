import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DashboardTopPerformer } from '@adventureworks-web/sales/data-access';

@Component({
  selector: 'aw-top-performers',
  standalone: true,
  imports: [RouterLink, CurrencyPipe],
  templateUrl: './top-performers.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TopPerformersComponent {
  readonly performers = input.required<DashboardTopPerformer[]>();
  /**
   * The API returns performers sorted descending by revenue, so index 0 is
   * always the leader. Using the leader's revenue as the 100% anchor means
   * every other bar is proportional to the top performer rather than to an
   * arbitrary absolute max.
   */
  protected readonly maxRevenue = computed(() => this.performers()[0]?.revenue ?? 0);

  /**
   * Returns a percentage width (0–100) for the DaisyUI progress bar.
   * The `maxRevenue() > 0` guard prevents division by zero when the
   * performers list is empty or all revenues are zero.
   */
  protected barWidth(revenue: number): number {
    return this.maxRevenue() > 0 ? (revenue / this.maxRevenue()) * 100 : 0;
  }
}
