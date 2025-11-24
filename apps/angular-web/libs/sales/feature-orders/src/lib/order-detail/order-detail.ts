import { CurrencyPipe, DatePipe, PercentPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { SalesOrderDetail } from '@adventureworks-web/sales/data-access';
import { EmptyStateComponent, SkeletonComponent, StatusBadgeComponent } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';
import { extractListNavParams } from '../list-nav-params';
import { STATUS_BADGE_MAP } from '../order-status-badge';

@Component({
  selector: 'aw-order-detail',
  standalone: true,
  imports: [RouterLink, CurrencyPipe, DatePipe, PercentPipe, SkeletonComponent, EmptyStateComponent, StatusBadgeComponent],
  templateUrl: './order-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Full sales-order detail view at `/sales/orders/:id`. Calls SalesApiService directly; no NgRx store.
 * Mirrors the StoreDetailComponent pattern: isLoading/notFound/hasError signals, backQueryParams
 * snapshot read is intentional.
 */
export class OrderDetailComponent implements OnInit {
  private readonly salesApi = inject(SalesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly order = signal<SalesOrderDetail | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly notFound = signal(false);
  protected readonly hasError = signal(false);

  protected readonly statusBadgeMap = STATUS_BADGE_MAP;

  protected readonly backQueryParams = computed(() =>
    // snapshot read is intentional: captures list nav state once at navigation time
    extractListNavParams(this.route.snapshot.queryParams),
  );

  constructor() {
    effect(() => {
      if (this.hasError()) {
        this.notificationService.error('Failed to load order. Please try again.');
      }
    });
  }

  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    const id = Math.trunc(Number(rawId));
    if (!id || id <= 0) {
      void this.router.navigate(['/sales/orders']);
      return;
    }
    this.isLoading.set(true);
    this.salesApi
      .getSalesOrder(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (detail) => {
          if (detail === null) {
            this.notFound.set(true);
          } else {
            this.order.set(detail);
          }
          this.isLoading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          if (err.status === 404) {
            this.notFound.set(true);
          } else {
            this.hasError.set(true);
          }
          this.isLoading.set(false);
        },
      });
  }
}
