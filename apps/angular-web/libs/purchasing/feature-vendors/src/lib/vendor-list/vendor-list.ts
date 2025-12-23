import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { VendorStore } from '@adventureworks-web/purchasing/data-access';
import { ColumnDefDirective, DataTableComponent, SelectFieldComponent, StatusBadgeComponent, ToggleFieldComponent } from '@adventureworks-web/shared/ui';
import type { ColumnConfig } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';

const PAGE_SIZE = 25;
const MIN_CREDIT_RATING = 1;
const MAX_CREDIT_RATING = 5;

/** 1=Superior, 2=Excellent, 3=Above Average, 4=Average, 5=Below Average — matches VendorParameter.CreditRating on the server. */
const CREDIT_RATING_OPTIONS = [
  { value: 1, label: 'Superior' },
  { value: 2, label: 'Excellent' },
  { value: 3, label: 'Above Average' },
  { value: 4, label: 'Average' },
  { value: 5, label: 'Below Average' },
];

/** Mutable filter accumulator built from the filter-bar form before merging into VendorListParams. */
interface VendorFilters {
  creditRating?: number;
  preferredVendorStatus?: boolean;
  activeFlag?: boolean;
}

@Component({
  selector: 'aw-vendor-list',
  standalone: true,
  imports: [CurrencyPipe, ReactiveFormsModule, DataTableComponent, ColumnDefDirective, SelectFieldComponent, ToggleFieldComponent, StatusBadgeComponent],
  templateUrl: './vendor-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Risk-ranked, filterable vendor list at `/purchasing/vendors`. Server always sorts by total spend
 * descending — there is no client-facing sort column, unlike OrderListComponent/StoreListComponent.
 *
 * URL-param sync uses a reactive `route.queryParams` subscription (mirroring OrderListComponent) so
 * browser back/forward navigation re-fires the load while the component stays mounted. Action methods
 * only write to the URL; the subscription is the sole driver of data loading.
 *
 * The preferred/active toggles are "show only" filters: unchecked omits the param entirely (no
 * server-side filtering on that field), checked sends `true`. The API also supports filtering to
 * `false` explicitly, but that isn't exposed here — a single toggle can only represent two UI states.
 */
export class VendorListComponent implements OnInit {
  private readonly vendorStore = inject(VendorStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly isLoading = this.vendorStore.isLoading;
  protected readonly pageNumber = this.vendorStore.pageNumber;
  protected readonly pageSize = this.vendorStore.pageSize;
  protected readonly totalPages = this.vendorStore.totalPages;
  protected readonly totalRecords = this.vendorStore.totalRecords;

  protected readonly creditRatingOptions = CREDIT_RATING_OPTIONS;

  /** Status column: success badge for preferred vendors, outline otherwise. */
  protected readonly statusBadgeMap: Record<string, string> = {
    preferred: 'badge-success',
    standard: 'badge-outline',
  };

  /** Risk column: danger badge for high-risk vendors (CreditRating >= 4 in the top-half spend rank), success otherwise. */
  protected readonly riskBadgeMap: Record<string, string> = {
    'high-risk': 'badge-error',
    'low-risk': 'badge-success',
  };

  /** Filter-bar form. SelectFieldComponent CVA emits strings; ToggleFieldComponent CVAs emit booleans. */
  protected readonly filterForm = this.fb.group({
    creditRating: [''],
    preferredVendorStatus: [false],
    activeFlag: [false],
  });

  protected readonly columns: ColumnConfig[] = [
    { key: 'name', label: 'Vendor Name', sortable: false },
    { key: 'creditRatingLabel', label: 'Credit Rating', sortable: false },
    { key: 'status', label: 'Status', sortable: false },
    { key: 'totalSpend', label: 'Total Spend', sortable: false, headerClass: 'text-right', cellClass: 'text-right' },
    { key: 'poCount', label: 'PO Count', sortable: false, headerClass: 'text-right', cellClass: 'text-right' },
    { key: 'risk', label: 'Risk', sortable: false },
    { key: 'view', label: '', sortable: false },
  ];

  /** Flattens vendor entities to flat key-value rows for DataTableComponent. */
  protected readonly rows = computed(() =>
    this.vendorStore.entities().map((vendor): Record<string, unknown> => ({
      vendorId: vendor.vendorId,
      name: vendor.name,
      accountNumber: vendor.accountNumber,
      creditRatingLabel: vendor.creditRatingLabel,
      preferredVendorStatus: vendor.preferredVendorStatus,
      statusKey: vendor.preferredVendorStatus ? 'preferred' : 'standard',
      activeFlag: vendor.activeFlag,
      totalSpend: vendor.totalSpend,
      poCount: vendor.poCount,
      isHighRisk: vendor.isHighRisk,
      riskKey: vendor.isHighRisk ? 'high-risk' : 'low-risk',
    })),
  );

  constructor() {
    effect(() => {
      if (this.vendorStore.hasError()) {
        this.notificationService.error('Failed to load vendors. Please try again.');
      }
    });
  }

  /** Subscribes to `route.queryParams` so that back/forward navigation re-fires the load while the component stays mounted. */
  ngOnInit(): void {
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      this.restoreFiltersFromUrl(params as Record<string, string>);
      this.loadFromUrl(params as Record<string, string>);
    });
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
    this.filterForm.reset({ creditRating: '', preferredVendorStatus: false, activeFlag: false });
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { creditRating: null, preferredVendorStatus: null, activeFlag: null, pageNumber: null },
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

  /** Navigates to the vendor detail page — mirrors WorkOrderListComponent's View-button row navigation. */
  protected onViewClick(row: Record<string, unknown>): void {
    void this.router.navigate(['/purchasing/vendors', row['vendorId']]);
  }

  /**
   * Restores filter-bar form values from the emitted URL params. `creditRating` is range-validated
   * (see `parseCreditRating`) so an out-of-range or junk bookmarked value (e.g. `creditRating=9`)
   * never lands on the `<select>` as a value with no matching `<option>` — it resets to "All ratings".
   */
  private restoreFiltersFromUrl(params: Record<string, string>): void {
    const creditRating = this.parseCreditRating(params['creditRating']);
    this.filterForm.setValue({
      creditRating: creditRating !== undefined ? String(creditRating) : '',
      preferredVendorStatus: params['preferredVendorStatus'] === 'true',
      activeFlag: params['activeFlag'] === 'true',
    });
  }

  /** Issues a load using the current URL state. */
  private loadFromUrl(params: Record<string, string>): void {
    const pageNumber = Math.max(1, Math.trunc(Number(params['pageNumber'])) || 1);
    const filters = this.parseFilterParams(params);
    this.vendorStore.loadPage({ pageNumber, pageSize: PAGE_SIZE, ...filters });
  }

  /**
   * Builds the filter accumulator from a string-keyed source (URL params or form values), coercing
   * numerics and omitting empty/false-by-default fields. `creditRating` is parsed and range-validated
   * by `parseCreditRating` so a junk (`creditRating=abc`) or out-of-range (`creditRating=9`) URL value
   * is silently dropped rather than forwarded to the API — the server 400s on out-of-range values
   * (see `VendorParameter.CreditRating`, a `byte?` validated 1–5), and surfacing that as a raw error
   * toast for a value the user never actually chose (a stale/malicious bookmark) would be confusing.
   */
  private parseFilterParams(src: Record<string, string>): VendorFilters {
    const filters: VendorFilters = {};
    const creditRating = this.parseCreditRating(src['creditRating']);
    if (creditRating !== undefined) {
      filters.creditRating = creditRating;
    }
    if (src['preferredVendorStatus'] === 'true') {
      filters.preferredVendorStatus = true;
    }
    if (src['activeFlag'] === 'true') {
      filters.activeFlag = true;
    }
    return filters;
  }

  /** Parses and range-validates a `creditRating` string (1–5 inclusive); junk or out-of-range values resolve to `undefined`. */
  private parseCreditRating(raw: string | undefined): number | undefined {
    if (!raw) {
      return undefined;
    }
    const value = Number(raw);
    return Number.isInteger(value) && value >= MIN_CREDIT_RATING && value <= MAX_CREDIT_RATING ? value : undefined;
  }

  /** Reads the filter form, parsing the select string to a number and omitting unchecked toggles. */
  private readFilters(): VendorFilters {
    const raw = this.filterForm.getRawValue();
    return this.parseFilterParams({
      creditRating: raw.creditRating ?? '',
      preferredVendorStatus: String(raw.preferredVendorStatus ?? false),
      activeFlag: String(raw.activeFlag ?? false),
    });
  }

  /** Maps applied filters to URL query params, nulling any cleared field so it is removed on merge. */
  private filterUrlParams(filters: VendorFilters): Record<string, string | number | null> {
    return {
      creditRating: filters.creditRating ?? null,
      preferredVendorStatus: filters.preferredVendorStatus ? 'true' : null,
      activeFlag: filters.activeFlag ? 'true' : null,
    };
  }
}
