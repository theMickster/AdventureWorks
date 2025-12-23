import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, EMPTY, switchMap, tap } from 'rxjs';
import { PurchasingApiService } from '@adventureworks-web/purchasing/data-access';
import type { PurchaseOrderSummary, VendorDetail } from '@adventureworks-web/purchasing/data-access';
import {
  CardComponent,
  ColumnDefDirective,
  DataTableComponent,
  EmptyStateComponent,
  SkeletonComponent,
  StatusBadgeComponent,
} from '@adventureworks-web/shared/ui';
import type { ColumnConfig } from '@adventureworks-web/shared/ui';

const PAGE_SIZE = 25;
const MIN_STATUS = 1;
const MAX_STATUS = 4;

/** 1=Pending, 2=Approved, 3=Rejected, 4=Complete — matches VendorPurchaseOrderParameter.Status on the server. */
const STATUS_OPTIONS = [
  { value: 1, label: 'Pending' },
  { value: 2, label: 'Approved' },
  { value: 3, label: 'Rejected' },
  { value: 4, label: 'Complete' },
];

/** Mutable filter accumulator built from the filter-bar form before merging into VendorPurchaseOrderParams. */
interface PurchaseOrderFilters {
  status?: number;
  startDate?: string;
  endDate?: string;
}

@Component({
  selector: 'aw-vendor-detail',
  standalone: true,
  imports: [
    CurrencyPipe,
    DatePipe,
    RouterLink,
    ReactiveFormsModule,
    CardComponent,
    SkeletonComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
    DataTableComponent,
    ColumnDefDirective,
  ],
  templateUrl: './vendor-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Vendor profile with spend metrics and filterable purchase order history at `/purchasing/vendors/:id`.
 * Calls PurchasingApiService directly; no NgRx store, mirroring OrderDetailComponent/StoreDetailComponent.
 *
 * Vendor detail and PO history load sequentially, not in parallel: the PO-history queryParams
 * subscription only starts once the vendor is confirmed to exist. Firing both requests in parallel
 * would risk a PO-history error/empty-state toast racing the "Vendor not found" EmptyState for a
 * vendor id that doesn't exist at all — the PO-history call would be entirely wasted in that case.
 *
 * Loading/error state is tracked as two independent signal groups (vendor vs. PO history) —
 * intentionally not merged, since the two requests fail independently and the template needs to
 * render "vendor not found" even while a PO-history request might still be in flight or retried.
 */
export class VendorDetailComponent implements OnInit {
  private readonly purchasingApi = inject(PurchasingApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly vendor = signal<VendorDetail | null>(null);
  protected readonly isLoadingVendor = signal(false);
  protected readonly vendorNotFound = signal(false);
  protected readonly vendorHasError = signal(false);

  protected readonly purchaseOrders = signal<PurchaseOrderSummary[]>([]);
  protected readonly isLoadingPurchaseOrders = signal(false);
  protected readonly purchaseOrdersHasError = signal(false);
  protected readonly poPageNumber = signal(1);
  protected readonly poPageSize = signal(PAGE_SIZE);
  protected readonly poTotalPages = signal(0);
  protected readonly poTotalRecords = signal(0);

  protected readonly statusOptions = STATUS_OPTIONS;

  /** Preferred/Standard badge — same map shape VendorListComponent uses for its Status column. */
  protected readonly preferredBadgeMap: Record<string, string> = {
    preferred: 'badge-success',
    standard: 'badge-outline',
  };

  /** Active/Inactive badge — a second, independent StatusBadgeComponent instance/map. */
  protected readonly activeBadgeMap: Record<string, string> = {
    active: 'badge-success',
    inactive: 'badge-secondary',
  };

  /** Purchase order status badge, keyed by the lowercased server-provided statusLabel. */
  protected readonly poStatusBadgeMap: Record<string, string> = {
    pending: 'badge-warning',
    approved: 'badge-info',
    rejected: 'badge-error',
    complete: 'badge-success',
  };

  protected readonly filterForm = this.fb.group({
    status: [''],
    startDate: [''],
    endDate: [''],
  });

  protected readonly columns: ColumnConfig[] = [
    { key: 'purchaseOrderId', label: 'PO Number' },
    { key: 'orderDate', label: 'Order Date' },
    { key: 'dueDate', label: 'Due Date' },
    { key: 'statusLabel', label: 'Status' },
    { key: 'totalDue', label: 'Total Due', headerClass: 'text-right', cellClass: 'text-right' },
  ];

  /** Flattens purchase order entities to flat key-value rows for DataTableComponent. */
  protected readonly rows = computed(() =>
    this.purchaseOrders().map((po): Record<string, unknown> => ({
      purchaseOrderId: po.purchaseOrderId,
      orderDate: po.orderDate,
      dueDate: po.dueDate,
      status: po.status,
      statusLabel: po.statusLabel,
      statusKey: po.statusLabel.toLowerCase(),
      totalDue: po.totalDue,
    })),
  );

  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    const id = Math.trunc(Number(rawId));
    if (!id || id <= 0) {
      void this.router.navigate(['/purchasing/vendors']);
      return;
    }

    this.loadVendor(id);
  }

  /** Writes the current filters plus pageNumber=1 to the URL (merge); the queryParams subscription reloads. */
  protected onApplyFilters(): void {
    const filters = this.readFilters();
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { ...this.filterUrlParams(filters), pageNumber: 1 },
      queryParamsHandling: 'merge',
    });
  }

  /** Clears all filters, returning to the default unfiltered view at page 1. */
  protected onResetFilters(): void {
    this.filterForm.reset({ status: '', startDate: '', endDate: '' });
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { status: null, startDate: null, endDate: null, pageNumber: null },
      queryParamsHandling: 'merge',
    });
  }

  /** Writes the requested pageNumber to the URL; the merge preserves current filters and the subscription reloads. */
  protected onPageChange(page: number): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { pageNumber: page },
      queryParamsHandling: 'merge',
    });
  }

  /** Loads the vendor's profile and spend metrics; only starts the PO-history sync once it succeeds. */
  private loadVendor(id: number): void {
    this.isLoadingVendor.set(true);
    this.purchasingApi
      .getVendorDetail(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (detail) => {
          this.vendor.set(detail);
          this.isLoadingVendor.set(false);
          this.startPurchaseOrderHistorySync(id);
        },
        error: (err: HttpErrorResponse) => {
          if (err.status === 404) {
            this.vendorNotFound.set(true);
          } else {
            this.vendorHasError.set(true);
          }
          this.isLoadingVendor.set(false);
        },
      });
  }

  /**
   * Subscribes to `route.queryParams` so back/forward navigation re-fires the PO-history load.
   * Uses `switchMap`, not a raw per-emission `subscribe()`, so a new URL change (e.g. the user
   * applies a filter, then immediately changes it again before the first response lands) cancels
   * the still-in-flight previous request — otherwise the two HTTP calls race and whichever
   * response resolves *last* would win regardless of which query is actually current. Mirrors
   * `VendorStore.loadPage`'s `switchMap`-based `rxMethod` cancellation guard.
   */
  private startPurchaseOrderHistorySync(vendorId: number): void {
    this.route.queryParams
      .pipe(
        tap((params) => this.restoreFiltersFromUrl(params as Record<string, string>)),
        switchMap((params) => this.loadPurchaseOrders(vendorId, params as Record<string, string>)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  /** Issues a PO-history load for the current URL state; returns an observable for `switchMap` to manage cancellation. */
  private loadPurchaseOrders(vendorId: number, params: Record<string, string>) {
    const pageNumber = Math.max(1, Math.trunc(Number(params['pageNumber'])) || 1);
    const filters = this.parseFilterParams(params);

    this.isLoadingPurchaseOrders.set(true);
    this.purchaseOrdersHasError.set(false);

    return this.purchasingApi.getVendorPurchaseOrders(vendorId, { pageNumber, pageSize: PAGE_SIZE, ...filters }).pipe(
      tap((result) => {
        this.purchaseOrders.set(result.results ?? []);
        this.poPageNumber.set(result.pageNumber);
        this.poPageSize.set(result.pageSize);
        this.poTotalPages.set(result.totalPages);
        this.poTotalRecords.set(result.totalRecords);
        this.isLoadingPurchaseOrders.set(false);
      }),
      catchError(() => {
        this.purchaseOrdersHasError.set(true);
        this.isLoadingPurchaseOrders.set(false);
        return EMPTY;
      }),
    );
  }

  /**
   * Restores filter-bar form values from the emitted URL params. `status` is range-validated
   * (see `parseStatus`) so an out-of-range or junk bookmarked value never lands on the `<select>`
   * as a value with no matching `<option>` — it resets to "All statuses", mirroring
   * VendorListComponent's `creditRating` handling.
   */
  private restoreFiltersFromUrl(params: Record<string, string>): void {
    const status = this.parseStatus(params['status']);
    this.filterForm.setValue({
      status: status !== undefined ? String(status) : '',
      startDate: params['startDate'] ?? '',
      endDate: params['endDate'] ?? '',
    });
  }

  /** Builds the filter accumulator from a string-keyed source (URL params or form values). */
  private parseFilterParams(src: Record<string, string>): PurchaseOrderFilters {
    const filters: PurchaseOrderFilters = {};
    const status = this.parseStatus(src['status']);
    if (status !== undefined) {
      filters.status = status;
    }
    if (src['startDate']) {
      filters.startDate = src['startDate'];
    }
    if (src['endDate']) {
      filters.endDate = src['endDate'];
    }
    return filters;
  }

  /** Parses and range-validates a `status` string (1-4 inclusive); junk or out-of-range values resolve to `undefined`. */
  private parseStatus(raw: string | undefined): number | undefined {
    if (!raw) {
      return undefined;
    }
    const value = Number(raw);
    return Number.isInteger(value) && value >= MIN_STATUS && value <= MAX_STATUS ? value : undefined;
  }

  /** Reads the filter form, parsing the select string to a number and omitting empty date fields. */
  private readFilters(): PurchaseOrderFilters {
    const raw = this.filterForm.getRawValue();
    return this.parseFilterParams({
      status: raw.status ?? '',
      startDate: raw.startDate ?? '',
      endDate: raw.endDate ?? '',
    });
  }

  /** Maps applied filters to URL query params, nulling any cleared field so it is removed on merge. */
  private filterUrlParams(filters: PurchaseOrderFilters): Record<string, string | number | null> {
    return {
      status: filters.status ?? null,
      startDate: filters.startDate ?? null,
      endDate: filters.endDate ?? null,
    };
  }
}
