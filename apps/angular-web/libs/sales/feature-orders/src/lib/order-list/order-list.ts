import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { SalesApiService, SalesOrderStore, SALES_ORDER_STATUSES } from '@adventureworks-web/sales/data-access';
import type { SalesOrderParams } from '@adventureworks-web/sales/data-access';
import { LookupApiService } from '@adventureworks-web/shared/data-access';
import { ColumnDefDirective, DataTableComponent, SelectFieldComponent, StatusBadgeComponent } from '@adventureworks-web/shared/ui';
import type { ColumnConfig } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';

const PAGE_SIZE = 25;
const VALID_SORT_COLUMNS = ['orderDate', 'totalDue', 'salesOrderNumber', 'salesOrderId'] as const;
type SortColumn = (typeof VALID_SORT_COLUMNS)[number];

const DEFAULT_ORDER_BY: SortColumn = 'orderDate';
const DEFAULT_SORT_ORDER: 'asc' | 'desc' = 'desc';

/** Mutable filter accumulator built from the filter-bar form before merging into SalesOrderParams. */
interface SalesOrderFilters {
  orderDateFrom?: string;
  orderDateTo?: string;
  status?: number;
  salesPersonId?: number;
  territoryId?: number;
}

// DaisyUI badge variant per lowercased server statusDescription. The default status-badge map
// does not cover these sales-order labels, so an explicit map is supplied.
const STATUS_BADGE_MAP: Record<string, string> = {
  'in process': 'badge-info',
  approved: 'badge-success',
  backordered: 'badge-warning',
  rejected: 'badge-error',
  shipped: 'badge-success',
  cancelled: 'badge-error',
};

@Component({
  selector: 'aw-order-list',
  standalone: true,
  imports: [CurrencyPipe, ReactiveFormsModule, DataTableComponent, ColumnDefDirective, SelectFieldComponent, StatusBadgeComponent],
  templateUrl: './order-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Paginated, filterable sales-order list at `/sales/orders`.
 *
 * Mirrors StoreListComponent's URL-param sync, sort allowlist, and pagination invariants, replacing the
 * single search box with a five-field filter bar (date-from, date-to, status, sales person, territory).
 * Filter state lives in a small FormGroup so the SelectFieldComponent CVAs (which emit string values)
 * can drive it; values are parsed back to numbers before reaching the SalesOrderParams.
 *
 * Date format: `<input type="date">` yields `YYYY-MM-DD`, which binds to the API's `DateTime?` filter
 * parameters. The API documents the range as inclusive on both ends; for the AdventureWorks dataset
 * OrderDate is always midnight, so a date-only `orderDateTo` matches same-day orders. A time component
 * is intentionally NOT appended (see US-736 brief gotcha #3).
 */
export class OrderListComponent implements OnInit {
  private readonly salesOrderStore = inject(SalesOrderStore);
  private readonly salesApi = inject(SalesApiService);
  private readonly lookupApi = inject(LookupApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly sortColumn = signal('');
  protected readonly sortDirection = signal<'asc' | 'desc'>(DEFAULT_SORT_ORDER);

  protected readonly isLoading = this.salesOrderStore.isLoading;
  protected readonly pageNumber = this.salesOrderStore.pageNumber;
  protected readonly pageSize = this.salesOrderStore.pageSize;
  protected readonly totalPages = this.salesOrderStore.totalPages;
  protected readonly totalRecords = this.salesOrderStore.totalRecords;

  protected readonly statusOptions = SALES_ORDER_STATUSES.map((s) => ({ value: s.value, label: s.label }));
  protected readonly salesPersonOptions = signal<{ value: number; label: string }[]>([]);
  protected readonly territoryOptions = signal<{ value: number; label: string }[]>([]);

  /** Filter-bar form. SelectFieldComponent CVAs emit strings; values are parsed to number on apply. */
  protected readonly filterForm = this.fb.group({
    orderDateFrom: [''],
    orderDateTo: [''],
    status: [''],
    salesPersonId: [''],
    territoryId: [''],
  });

  protected readonly columns: ColumnConfig[] = [
    { key: 'salesOrderNumber', label: 'Order #', sortable: true },
    { key: 'orderDate', label: 'Order Date', sortable: true },
    { key: 'customerName', label: 'Customer', sortable: false },
    { key: 'salesPersonName', label: 'Sales Person', sortable: false },
    { key: 'status', label: 'Status', sortable: false },
    { key: 'totalDue', label: 'Total Due', sortable: true, cellClass: 'text-right' },
    { key: 'view', label: '', sortable: false, cellClass: 'text-right' },
  ];

  protected readonly statusBadgeMap = STATUS_BADGE_MAP;

  /**
   * Flattens sales-order entities to flat key-value rows for DataTableComponent. Projects orderDate to a
   * display date string, totalDue to a currency-ready scalar, and a lowercased status key for the badge.
   * A null salesPersonName renders as an em dash.
   */
  protected readonly rows = computed(() =>
    this.salesOrderStore.entities().map((order): Record<string, unknown> => ({
      salesOrderId: order.salesOrderId,
      salesOrderNumber: order.salesOrderNumber,
      orderDate: order.orderDate.slice(0, 10),
      customerName: order.customerName,
      salesPersonName: order.salesPersonName ?? '—',
      status: order.statusDescription,
      statusKey: order.statusDescription.toLowerCase(),
      totalDue: order.totalDue,
    })),
  );

  constructor() {
    // effect() drives the error toast as a side effect outside the render cycle; a template
    // expression cannot safely call the imperative notification service.
    effect(() => {
      if (this.salesOrderStore.hasError()) {
        this.notificationService.error('Failed to load sales orders. Please try again.');
      }
    });
  }

  ngOnInit(): void {
    // Dropdown reference data loads independently of the grid — a slow/failed lookup must not
    // block or break the list. The list load is kicked off first.
    const params = this.route.snapshot.queryParams;
    this.restoreFiltersFromUrl(params);
    this.loadFromUrl(params);

    forkJoin({
      salesPersons: this.salesApi.getSalesPersons({ pageNumber: 1, pageSize: 100 }),
      territories: this.lookupApi.getTerritories(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.salesPersonOptions.set(
            (result.salesPersons.results ?? []).map((sp) => ({ value: sp.id, label: `${sp.firstName} ${sp.lastName}` })),
          );
          this.territoryOptions.set(result.territories.map((t) => ({ value: t.id, label: t.name })));
        },
        error: () => {
          this.notificationService.error('Failed to load filter options. Filtering may be unavailable.');
        },
      });
  }

  /** Applies the filter bar: loads page 1 with the current filters and mirrors them to the URL (merge). */
  protected onApplyFilters(): void {
    const filters = this.readFilters();
    const sortParams = this.activeSortParams();
    this.salesOrderStore.loadPage({ pageNumber: 1, pageSize: PAGE_SIZE, ...sortParams, ...filters });
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { ...this.filterUrlParams(filters), pageNumber: 1 },
      queryParamsHandling: 'merge',
    });
  }

  /**
   * Clears all filters and sort, returning to the default OrderDate-desc baseline. URL filter/sort/page
   * params are nulled so the merge strips them; the list reloads at page 1 with only the default sort.
   */
  protected onResetFilters(): void {
    this.filterForm.reset({ orderDateFrom: '', orderDateTo: '', status: '', salesPersonId: '', territoryId: '' });
    this.sortColumn.set('');
    this.sortDirection.set(DEFAULT_SORT_ORDER);
    this.salesOrderStore.loadPage({ pageNumber: 1, pageSize: PAGE_SIZE, orderBy: DEFAULT_ORDER_BY, sortOrder: DEFAULT_SORT_ORDER });
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        orderDateFrom: null,
        orderDateTo: null,
        status: null,
        salesPersonId: null,
        territoryId: null,
        pageNumber: null,
        orderBy: null,
        sortOrder: null,
      },
      queryParamsHandling: 'merge',
    });
  }

  /** Loads the requested page, carrying current sort and filters forward so neither is lost on paging. */
  protected onPageChange(page: number): void {
    const sortParams = this.activeSortParams();
    const filters = this.readFilters();
    this.salesOrderStore.loadPage({ pageNumber: page, pageSize: PAGE_SIZE, ...sortParams, ...filters });
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { pageNumber: page },
      queryParamsHandling: 'merge',
    });
  }

  /** Applies a column sort, carrying current filters forward. Non-allowlisted columns are ignored (see guard below). */
  protected onSortChange(event: { column: string; direction: 'asc' | 'desc' }): void {
    this.sortColumn.set(event.column);
    this.sortDirection.set(event.direction);
    // Runtime allowlist guard: DataTableComponent emits sort events for any column key; the
    // sortable flag is a UI hint only. VALID_SORT_COLUMNS keeps arbitrary keys off the API call.
    if (!(VALID_SORT_COLUMNS as readonly string[]).includes(event.column)) {
      return;
    }
    const orderBy = event.column as SortColumn;
    const filters = this.readFilters();
    this.salesOrderStore.loadPage({ pageNumber: this.pageNumber(), pageSize: PAGE_SIZE, orderBy, sortOrder: event.direction, ...filters });
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { orderBy: event.column, sortOrder: event.direction },
      queryParamsHandling: 'merge',
    });
  }

  /** Routes a clicked row to the order detail view at /sales/orders/:id (US-737). */
  protected onRowClick(row: Record<string, unknown>): void {
    void this.router.navigate(['/sales/orders', row['salesOrderId']]);
  }

  /** Restores filter-bar form values and sort signals from the URL snapshot. */
  private restoreFiltersFromUrl(params: Record<string, string>): void {
    this.filterForm.setValue({
      orderDateFrom: params['orderDateFrom'] ?? '',
      orderDateTo: params['orderDateTo'] ?? '',
      status: params['status'] ?? '',
      salesPersonId: params['salesPersonId'] ?? '',
      territoryId: params['territoryId'] ?? '',
    });

    const orderBy = this.parseOrderBy(params['orderBy']);
    if (orderBy) {
      this.sortColumn.set(orderBy);
      this.sortDirection.set(params['sortOrder'] === 'asc' ? 'asc' : 'desc');
    }
  }

  /** Issues the initial list load using restored URL state, defaulting to OrderDate desc. */
  private loadFromUrl(params: Record<string, string>): void {
    const pageNumber = Math.max(1, Math.trunc(Number(params['pageNumber'])) || 1);
    const orderBy = this.parseOrderBy(params['orderBy']);
    const sortOrder = params['sortOrder'] === 'asc' ? 'asc' : 'desc';
    const sortParams = orderBy
      ? { orderBy, sortOrder: sortOrder as 'asc' | 'desc' }
      : { orderBy: DEFAULT_ORDER_BY, sortOrder: DEFAULT_SORT_ORDER };
    this.salesOrderStore.loadPage({ pageNumber, pageSize: PAGE_SIZE, ...sortParams, ...this.readFilters() });
  }

  /** Allowlist-validates an orderBy URL value; junk resolves to undefined and never reaches the API. */
  private parseOrderBy(raw: string | undefined): SortColumn | undefined {
    return (VALID_SORT_COLUMNS as readonly string[]).includes(raw ?? '') ? (raw as SortColumn) : undefined;
  }

  /** Active sort params for the current signal state, or empty when no sort is set. */
  private activeSortParams(): Pick<SalesOrderParams, 'orderBy' | 'sortOrder'> {
    const col = this.sortColumn();
    return (VALID_SORT_COLUMNS as readonly string[]).includes(col)
      ? { orderBy: col as SortColumn, sortOrder: this.sortDirection() }
      : {};
  }

  /** Reads the filter form, parsing select strings to numbers and omitting cleared (empty) fields. */
  private readFilters(): SalesOrderFilters {
    const value = this.filterForm.getRawValue();
    const filters: SalesOrderFilters = {};
    if (value.orderDateFrom) {
      filters.orderDateFrom = value.orderDateFrom;
    }
    if (value.orderDateTo) {
      filters.orderDateTo = value.orderDateTo;
    }
    if (value.status) {
      filters.status = Number(value.status);
    }
    if (value.salesPersonId) {
      filters.salesPersonId = Number(value.salesPersonId);
    }
    if (value.territoryId) {
      filters.territoryId = Number(value.territoryId);
    }
    return filters;
  }

  /** Maps applied filters to URL query params, nulling any cleared field so it is removed on merge. */
  private filterUrlParams(filters: SalesOrderFilters): Record<string, string | number | null> {
    return {
      orderDateFrom: filters.orderDateFrom ?? null,
      orderDateTo: filters.orderDateTo ?? null,
      status: filters.status ?? null,
      salesPersonId: filters.salesPersonId ?? null,
      territoryId: filters.territoryId ?? null,
    };
  }
}
