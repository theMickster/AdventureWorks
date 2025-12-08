import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { DashboardStore } from '@adventureworks-web/sales/data-access';
import { SkeletonComponent } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';
import { TrendChartComponent } from '@adventureworks-web/sales/ui-trend-chart';
import { TopPerformersComponent } from '../top-performers/top-performers';
import { TerritoryBreakdownComponent } from '../territory-breakdown/territory-breakdown';

@Component({
  selector: 'aw-dashboard',
  standalone: true,
  imports: [CurrencyPipe, DecimalPipe, SkeletonComponent, TrendChartComponent, TopPerformersComponent, TerritoryBreakdownComponent],
  templateUrl: './dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent implements OnInit {
  private readonly dashboardStore = inject(DashboardStore);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly isLoading = this.dashboardStore.isLoading;
  protected readonly dashboard = this.dashboardStore.dashboard;
  protected readonly hasError = this.dashboardStore.hasError;

  constructor() {
    // Constructor placement ensures Angular evaluates this effect after ngOnInit's setLoading() clears any stale error, preventing a spurious toast on revisit.
    effect(() => {
      if (this.hasError()) {
        this.notificationService.error('Failed to load dashboard KPIs. Please try again.');
      }
    });
  }

  ngOnInit(): void {
    this.dashboardStore.load();
  }

  /** Navigates to the orders list pre-filtered to the full calendar month of the clicked trend point. */
  protected onTrendChartClick({ year, month }: { year: number; month: number }): void {
    const pad = (n: number) => String(n).padStart(2, '0');
    const orderDateFrom = `${year}-${pad(month)}-01`;
    const daysInMonth = new Date(year, month, 0).getDate();
    const orderDateTo = `${year}-${pad(month)}-${pad(daysInMonth)}`;
    this.router.navigate(['/sales/orders'], { queryParams: { orderDateFrom, orderDateTo } });
  }
}
