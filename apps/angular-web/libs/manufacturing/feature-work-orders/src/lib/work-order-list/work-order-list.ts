import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { WorkOrderStore } from '@adventureworks-web/manufacturing/data-access';
import { ColumnDefDirective, DataTableComponent, StatusBadgeComponent } from '@adventureworks-web/shared/ui';
import type { ColumnConfig } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';
import { WORK_ORDER_STATUS_BADGE_MAP } from '../work-order-status-badge';

const PAGE_SIZE = 25;
const VALID_SORT_COLUMNS = ['workOrderId', 'startDate', 'dueDate'] as const;
type SortColumn = (typeof VALID_SORT_COLUMNS)[number];

const DEFAULT_ORDER_BY: SortColumn = 'startDate';
const DEFAULT_SORT_ORDER: 'asc' | 'desc' = 'desc';

/** Mutable filter accumulator built from the filter-bar form before merging into WorkOrderParams. */
interface WorkOrderFilters {
  productId?: number;
  scrapReasonId?: number;
  startDate?: string;
  endDate?: string;
  hasScrapped?: boolean;
}

@Component({
  selector: 'aw-work-order-list',
  standalone: true,
  imports: [ReactiveFormsModule, DataTableComponent, ColumnDefDirective, StatusBadgeComponent],
  templateUrl: './work-order-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Paginated, filterable production work-order list at `/manufacturing/work-orders`.
 *
 * URL-param sync uses a reactive `route.queryParams` subscription, matching `OrderListComponent`:
 * every URL change (including in-place browser back/forward while the component is mounted)
 * re-fires `restoreFiltersFromUrl` and `loadFromUrl`. Action methods (`onApplyFilters`,
 * `onResetFilters`, `onPageChange`, `onSortChange`) only write to the URL via `router.navigate`;
 * the subscription is the sole driver of data loading.
 *
 * `productId` and `scrapReasonId` are plain numeric filter inputs, not dropdowns — no lookup
 * endpoint exists for either. `startDate`/`endDate` are native `<input type="date">` range filters
 * and `hasScrapped` is an All / With Scrap / No Scrap `<select>` — both map directly onto
 * `WorkOrderParams`, which the API already supports. Default sort is `startDate` desc, matching
 * the API's own default, so "newest first" requires no extra client-side work.
 */
export class WorkOrderListComponent implements OnInit {
  private readonly workOrderStore = inject(WorkOrderStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly sortColumn = signal('');
  protected readonly sortDirection = signal<'asc' | 'desc'>(DEFAULT_SORT_ORDER);

  protected readonly isLoading = this.workOrderStore.isLoading;
  protected readonly pageNumber = this.workOrderStore.pageNumber;
  protected readonly pageSize = this.workOrderStore.pageSize;
  protected readonly totalPages = this.workOrderStore.totalPages;
  protected readonly totalRecords = this.workOrderStore.totalRecords;

  /**
   * Filter-bar form. `productId`/`scrapReasonId` are plain numeric inputs since no lookup endpoint
   * exists for either field. `startDate`/`endDate` are `<input type="date">` (empty string when
   * unset). `hasScrapped` is a tri-state select: `''` (All), `'true'` (With Scrap), `'false'` (No Scrap).
   */
  protected readonly filterForm = this.fb.group({
    productId: [''],
    scrapReasonId: [''],
    startDate: [''],
    endDate: [''],
    hasScrapped: [''],
  });

  protected readonly columns: ColumnConfig[] = [
    { key: 'productName', label: 'Product', sortable: false },
    { key: 'orderedQty', label: 'Ordered', sortable: false, headerClass: 'text-right', cellClass: 'text-right' },
    { key: 'stockedQty', label: 'Stocked', sortable: false, headerClass: 'text-right', cellClass: 'text-right' },
    { key: 'scrappedQty', label: 'Scrapped', sortable: false, headerClass: 'text-right', cellClass: 'text-right' },
    { key: 'yieldRate', label: 'Yield %', sortable: false, headerClass: 'text-right', cellClass: 'text-right' },
    { key: 'startDate', label: 'Start Date', sortable: true },
    { key: 'endDate', label: 'End Date', sortable: false },
    { key: 'statusKey', label: 'Status', sortable: false, headerClass: 'min-w-28' },
    { key: 'view', label: '', sortable: false, cellClass: 'text-right' },
  ];

  protected readonly statusBadgeMap = WORK_ORDER_STATUS_BADGE_MAP;

  /**
   * Flattens work-order entities to flat key-value rows for DataTableComponent. Projects
   * startDate/endDate to display date strings (endDate blank when still in progress), and
   * `statusKey` to `'completed late'` only when `isCompletedLate` — an on-time order gets no
   * badge at all, rather than an "On Time" label.
   */
  protected readonly rows = computed(() =>
    this.workOrderStore.entities().map((workOrder): Record<string, unknown> => ({
      workOrderId: workOrder.workOrderId,
      productName: workOrder.productName,
      orderedQty: workOrder.orderedQty,
      stockedQty: workOrder.stockedQty,
      scrappedQty: workOrder.scrappedQty,
      yieldRate: workOrder.yieldRate,
      startDate: workOrder.startDate.slice(0, 10),
      endDate: workOrder.endDate ? workOrder.endDate.slice(0, 10) : '',
      statusKey: workOrder.isCompletedLate ? 'completed late' : '',
    })),
  );

  /** Serialized snapshot of the last-applied filters; distinguishes filter changes from page/sort-only changes. */
  private _lastFilterHash = '';

  constructor() {
    effect(() => {
      if (this.workOrderStore.hasError()) {
        this.notificationService.error('Failed to load work orders. Please try again.');
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

  /** Clears all filters and sort, returning to the default startDate-desc baseline. */
  protected onResetFilters(): void {
    this.filterForm.reset({ productId: '', scrapReasonId: '', startDate: '', endDate: '', hasScrapped: '' });
    this.sortColumn.set('');
    this.sortDirection.set(DEFAULT_SORT_ORDER);
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        productId: null,
        scrapReasonId: null,
        startDate: null,
        endDate: null,
        hasScrapped: null,
        pageNumber: null,
        orderBy: null,
        sortOrder: null,
      },
      queryParamsHandling: 'merge',
    });
  }

  /** Writes the requested pageNumber to the URL; the merge preserves current sort/filters and the subscription reloads. */
  protected onPageChange(page: number): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { pageNumber: page },
      queryParamsHandling: 'merge',
    });
  }

  /** Writes the sort plus pageNumber=1 to the URL (merge preserves filters); the subscription reloads. */
  protected onSortChange(event: { column: string; direction: 'asc' | 'desc' }): void {
    if (!(VALID_SORT_COLUMNS as readonly string[]).includes(event.column)) {
      return;
    }
    this.sortColumn.set(event.column);
    this.sortDirection.set(event.direction);
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { orderBy: event.column, sortOrder: event.direction, pageNumber: 1 },
      queryParamsHandling: 'merge',
    });
  }

  /** Routes to the work-order detail view; a per-row "View" button, not a row-click handler (DataTableComponent has no row-click output). */
  protected onViewClick(row: Record<string, unknown>): void {
    void this.router.navigate(['/manufacturing/work-orders', row['workOrderId']]);
  }

  /** Restores filter-bar form values and sort signals from the emitted URL params. */
  private restoreFiltersFromUrl(params: Record<string, string>): void {
    this.filterForm.setValue({
      productId: params['productId'] ?? '',
      scrapReasonId: params['scrapReasonId'] ?? '',
      startDate: params['startDate'] ?? '',
      endDate: params['endDate'] ?? '',
      hasScrapped: params['hasScrapped'] === 'true' || params['hasScrapped'] === 'false' ? params['hasScrapped'] : '',
    });

    const orderBy = this.parseOrderBy(params['orderBy']);
    if (orderBy) {
      this.sortColumn.set(orderBy);
      this.sortDirection.set(params['sortOrder'] === 'asc' ? 'asc' : 'desc');
    } else {
      this.sortColumn.set('');
      this.sortDirection.set(DEFAULT_SORT_ORDER);
    }
  }

  /**
   * Issues a load using URL state. When filters have changed since the last load, calls
   * `applyFilters` (resets to page 1); when only the page or sort changed, calls `loadPage` directly.
   */
  private loadFromUrl(params: Record<string, string>): void {
    const pageNumber = Math.max(1, Math.trunc(Number(params['pageNumber'])) || 1);
    const orderBy = this.parseOrderBy(params['orderBy']);
    const sortOrder = params['sortOrder'] === 'asc' ? 'asc' : 'desc';
    const sortParams = orderBy
      ? { orderBy, sortOrder: sortOrder as 'asc' | 'desc' }
      : { orderBy: DEFAULT_ORDER_BY, sortOrder: DEFAULT_SORT_ORDER };

    const filters = this.parseFilterParams(params);
    const filterHash = JSON.stringify(filters);
    const filtersChanged = filterHash !== this._lastFilterHash;

    if (filtersChanged) {
      this._lastFilterHash = filterHash;
      this.workOrderStore.applyFilters({ pageNumber, pageSize: PAGE_SIZE, ...sortParams, ...filters });
    } else {
      this.workOrderStore.loadPage({ pageNumber, pageSize: PAGE_SIZE, ...sortParams, ...filters });
    }
  }

  /** Allowlist-validates an orderBy URL value; junk resolves to undefined and never reaches the API. */
  private parseOrderBy(raw: string | undefined): SortColumn | undefined {
    return (VALID_SORT_COLUMNS as readonly string[]).includes(raw ?? '') ? (raw as SortColumn) : undefined;
  }

  /**
   * Builds the filter accumulator from either URL params (always strings) or the filter form's raw
   * value. The form source is NOT reliably `string`: Angular's `NumberValueAccessor` (bound via
   * `input[type=number][formControlName]`) parses the DOM value and emits `number | null`, not the
   * `string` its `FormControl<string | null>` generic implies — a known Reactive Forms/template
   * type mismatch. `Number.isFinite` guards numeric fields so a junk URL value (or `NaN` from an
   * already-numeric form value) fails the check and is silently dropped rather than forwarded.
   */
  private parseFilterParams(src: Record<string, string | number | null | undefined>): WorkOrderFilters {
    const filters: WorkOrderFilters = {};
    if (src['productId'] && Number.isFinite(Number(src['productId']))) { filters.productId = Number(src['productId']); }
    if (src['scrapReasonId'] && Number.isFinite(Number(src['scrapReasonId']))) { filters.scrapReasonId = Number(src['scrapReasonId']); }
    if (src['startDate']) { filters.startDate = String(src['startDate']); }
    if (src['endDate']) { filters.endDate = String(src['endDate']); }
    if (src['hasScrapped'] === 'true') { filters.hasScrapped = true; }
    if (src['hasScrapped'] === 'false') { filters.hasScrapped = false; }
    return filters;
  }

  /** Reads the filter form, parsing string/number inputs to numbers and omitting cleared (empty) fields. */
  private readFilters(): WorkOrderFilters {
    return this.parseFilterParams(this.filterForm.getRawValue());
  }

  /** Maps applied filters to URL query params, nulling any cleared field so it is removed on merge. */
  private filterUrlParams(filters: WorkOrderFilters): Record<string, number | string | null> {
    return {
      productId: filters.productId ?? null,
      scrapReasonId: filters.scrapReasonId ?? null,
      startDate: filters.startDate ?? null,
      endDate: filters.endDate ?? null,
      hasScrapped: filters.hasScrapped === undefined ? null : String(filters.hasScrapped),
    };
  }
}
