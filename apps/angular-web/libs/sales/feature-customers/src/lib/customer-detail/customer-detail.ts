import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { CustomerDetail as CustomerDetailModel } from '@adventureworks-web/sales/data-access';
import { CardComponent, EmptyStateComponent, SkeletonComponent, StatusBadgeComponent } from '@adventureworks-web/shared/ui';

@Component({
  selector: 'aw-customer-detail',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, DecimalPipe, RouterLink, CardComponent, SkeletonComponent, EmptyStateComponent, StatusBadgeComponent],
  templateUrl: './customer-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Customer profile — LTV rank and lifetime-spend metric tiles — at `/sales/customers/:id`.
 *
 * Direct SalesApiService call; no NgRx store, mirroring PurchaseOrderDetailComponent's single-fetch
 * shape: signals-based loading/not-found/error state, aw-skeleton while loading, aw-empty-state on
 * 404 and on other failures. `totalCustomerCount` is server-supplied — never hardcoded or derived
 * client-side — since it is the denominator for the displayed "#N of totalCustomerCount" rank text.
 */
export class CustomerDetailComponent implements OnInit {
  private readonly salesApi = inject(SalesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly customer = signal<CustomerDetailModel | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly notFound = signal(false);
  protected readonly hasError = signal(false);

  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    const id = Math.trunc(Number(rawId));
    if (!id || id <= 0) {
      void this.router.navigate(['/sales/customers']);
      return;
    }

    this.loadCustomer(id);
  }

  private loadCustomer(id: number): void {
    this.isLoading.set(true);
    this.salesApi
      .getCustomerDetail(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (detail) => {
          this.customer.set(detail);
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
