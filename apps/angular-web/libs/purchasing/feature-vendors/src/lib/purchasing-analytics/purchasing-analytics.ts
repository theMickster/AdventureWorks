import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { PurchasingApiService } from '@adventureworks-web/purchasing/data-access';
import type { PurchasingAnalyticsDto } from '@adventureworks-web/purchasing/data-access';
import { ParetoChartComponent } from '@adventureworks-web/purchasing/ui-pareto-chart';
import { CardComponent, EmptyStateComponent, SkeletonComponent } from '@adventureworks-web/shared/ui';

@Component({
  selector: 'aw-purchasing-analytics',
  standalone: true,
  imports: [CurrencyPipe, RouterLink, ParetoChartComponent, CardComponent, SkeletonComponent, EmptyStateComponent],
  templateUrl: './purchasing-analytics.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Purchasing analytics dashboard at `/purchasing/analytics` — a Pareto chart of vendor spend plus
 * four purchase order pipeline tiles.
 *
 * Follows `PurchaseOrderDetailComponent`'s one-shot-fetch pattern: direct `PurchasingApiService`
 * call in `ngOnInit`, no NgRx store, local loading/error signals. There is no `notFound` state —
 * the analytics endpoint is an aggregate with no id and therefore no 404 case.
 */
export class PurchasingAnalyticsComponent implements OnInit {
  private readonly purchasingApi = inject(PurchasingApiService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly analytics = signal<PurchasingAnalyticsDto | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly hasError = signal(false);

  ngOnInit(): void {
    this.loadAnalytics();
  }

  /** Navigates to the clicked vendor's detail page. */
  protected onVendorClick(vendorId: number): void {
    void this.router.navigate(['/purchasing/vendors', vendorId]);
  }

  /** Builds a stable element id suffix for a pipeline tile (e.g. "pending"). */
  protected tileId(statusLabel: string): string {
    return statusLabel.toLowerCase();
  }

  private loadAnalytics(): void {
    this.isLoading.set(true);
    this.purchasingApi
      .getPurchasingAnalytics()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (analytics) => {
          this.analytics.set(analytics);
          this.isLoading.set(false);
        },
        error: () => {
          this.hasError.set(true);
          this.isLoading.set(false);
        },
      });
  }
}
