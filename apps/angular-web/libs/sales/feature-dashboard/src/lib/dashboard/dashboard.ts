import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, OnInit } from '@angular/core';
import { DashboardStore } from '@adventureworks-web/sales/data-access';
import { SkeletonComponent } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';

@Component({
  selector: 'aw-dashboard',
  standalone: true,
  imports: [CurrencyPipe, DecimalPipe, SkeletonComponent],
  templateUrl: './dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent implements OnInit {
  private readonly dashboardStore = inject(DashboardStore);
  private readonly notificationService = inject(NotificationService);

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
}
