import { CurrencyPipe, DecimalPipe, NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { Router } from '@angular/router';
import { DashboardTerritory } from '@adventureworks-web/sales/data-access';

@Component({
  selector: 'aw-territory-breakdown',
  standalone: true,
  imports: [CurrencyPipe, DecimalPipe, NgClass],
  templateUrl: './territory-breakdown.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TerritoryBreakdownComponent {
  private readonly router = inject(Router);
  readonly territories = input.required<DashboardTerritory[]>();

  /**
   * Groups territories by their `group` field (e.g. "North America", "Europe",
   * "Pacific") and sorts the groups alphabetically. Insertion order within
   * each group follows the API's revenue-descending order.
   *
   * Returned as `[groupName, territories[]]` pairs so the template can iterate
   * groups with a single `@for` without a secondary lookup.
   */
  protected readonly grouped = computed(() => {
    const map = new Map<string, DashboardTerritory[]>();
    for (const t of this.territories()) {
      const group = map.get(t.group) ?? [];
      group.push(t);
      map.set(t.group, group);
    }
    return Array.from(map.entries()).sort(([a], [b]) => a.localeCompare(b));
  });

  protected onTerritoryClick(territoryId: number): void {
    this.router.navigate(['/sales/orders'], { queryParams: { territoryId } });
  }
}
