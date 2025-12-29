import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PurchasingApiService } from '@adventureworks-web/purchasing/data-access';
import type { PurchaseOrderDetail as PurchaseOrderDetailModel } from '@adventureworks-web/purchasing/data-access';
import { CardComponent, EmptyStateComponent, SkeletonComponent, StatusBadgeComponent } from '@adventureworks-web/shared/ui';
import { PURCHASE_ORDER_STATUS_BADGE_MAP } from '../purchase-order-status-badge';

@Component({
  selector: 'aw-purchase-order-detail',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink, CardComponent, SkeletonComponent, EmptyStateComponent, StatusBadgeComponent],
  templateUrl: './purchase-order-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Full purchase order detail — header, spend metric tiles, and line items — at
 * `/purchasing/purchase-orders/:id`. Replaces the US-985-era PurchaseOrderDetailPlaceholderComponent.
 *
 * Follows VendorDetailComponent's established pattern: direct PurchasingApiService call, no NgRx
 * store, signals-based loading/error state, Skeleton while loading, EmptyState on 404, and a
 * separate error state for other failures.
 *
 * Line items render as a plain in-template `<table>` with a native `<tfoot>` totals row —
 * mirroring `OrderDetailComponent`'s precedent for a fixed-length, unpaginated line-item list —
 * rather than the shared `aw-data-table`, which has no footer/totals-row support and would put
 * the totals row in a second, independently-laid-out `<table>` that can't align its columns with
 * the one above it. The totals row renders `subTotal` from the API response rather than summing
 * `lineTotal` client-side, since the server already computes that exact figure.
 */
export class PurchaseOrderDetailComponent implements OnInit {
  private readonly purchasingApi = inject(PurchasingApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly purchaseOrder = signal<PurchaseOrderDetailModel | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly notFound = signal(false);
  protected readonly hasError = signal(false);

  protected readonly statusBadgeMap = PURCHASE_ORDER_STATUS_BADGE_MAP;

  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    const id = Math.trunc(Number(rawId));
    if (!id || id <= 0) {
      void this.router.navigate(['/purchasing/vendors']);
      return;
    }

    this.loadPurchaseOrder(id);
  }

  private loadPurchaseOrder(id: number): void {
    this.isLoading.set(true);
    this.purchasingApi
      .getPurchaseOrderDetail(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (detail) => {
          this.purchaseOrder.set(detail);
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
