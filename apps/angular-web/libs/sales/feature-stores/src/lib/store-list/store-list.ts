import { ChangeDetectionStrategy, Component, computed, effect, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { StoreStore } from '@adventureworks-web/sales/data-access';
import { DataTableComponent } from '@adventureworks-web/shared/ui';
import type { ColumnConfig } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';

const PAGE_SIZE = 25;
const VALID_SORT_COLUMNS = ['name'] as const;
type SortColumn = (typeof VALID_SORT_COLUMNS)[number];

@Component({
  selector: 'aw-store-list',
  standalone: true,
  imports: [DataTableComponent],
  templateUrl: './store-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StoreListComponent implements OnInit {
  private readonly storeStore = inject(StoreStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);

  protected readonly searchTerm = signal('');
  protected readonly sortColumn = signal('');
  protected readonly sortDirection = signal<'asc' | 'desc'>('asc');

  protected readonly isLoading = this.storeStore.isLoading;
  protected readonly pageNumber = this.storeStore.pageNumber;
  protected readonly pageSize = this.storeStore.pageSize;
  protected readonly totalPages = this.storeStore.totalPages;
  protected readonly totalRecords = this.storeStore.totalRecords;

  protected readonly columns: ColumnConfig[] = [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'salesPerson', label: 'Sales Person', sortable: false },
    { key: 'city', label: 'City', sortable: false },
    { key: 'state', label: 'State', sortable: false },
  ];

  /**
   * Flattens store entities into plain key-value objects.
   * DataTableComponent requires a `Record<string, unknown>[]` row shape — it cannot
   * consume nested entity objects directly, so nested salesPerson and address data
   * is projected to scalar string fields here.
   */
  protected readonly rows = computed(() =>
    this.storeStore.entities().map((store): Record<string, unknown> => {
      const sp = store.salesPerson;
      const salesPerson = sp ? `${sp.firstName} ${sp.lastName}` : '—';
      const city = store.storeAddresses[0]?.city ?? '—';
      const state = store.storeAddresses[0]?.stateProvinceName ?? '—';
      return { id: store.id, name: store.name, salesPerson, city, state };
    }),
  );

  constructor() {
    // effect() is used instead of a template binding because error notifications
    // are side effects (toast dispatch) that must fire outside the render cycle.
    // A template expression cannot call imperative service methods safely.
    effect(() => {
      if (this.storeStore.hasError()) {
        this.notificationService.error('Failed to load stores. Please try again.');
      }
    });
  }

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    const search = params['search'] as string | undefined;
    const pageNumber = Math.max(1, Math.trunc(Number(params['pageNumber'])) || 1);
    const rawOrderBy = params['orderBy'] as string | undefined;
    // Accept only column keys that exist in VALID_SORT_COLUMNS — prevents a
    // bookmarked URL with an arbitrary orderBy value from reaching the API.
    const orderBy = rawOrderBy === 'name' ? ('name' as const) : undefined;
    const sortOrder = (params['sortOrder'] === 'desc' ? 'desc' : 'asc') as 'asc' | 'desc';

    if (orderBy) {
      this.sortColumn.set(orderBy);
      this.sortDirection.set(sortOrder);
    }

    if (search) {
      this.searchTerm.set(search);
      const sortParams = orderBy ? { orderBy, sortOrder } : {};
      this.storeStore.search({ params: { pageNumber, pageSize: PAGE_SIZE, ...sortParams }, body: { name: search } });
    } else {
      const sortParams = orderBy ? { orderBy, sortOrder } : {};
      this.storeStore.loadPage({ pageNumber, pageSize: PAGE_SIZE, ...sortParams });
    }
  }

  protected onSearch(term: string): void {
    const trimmed = term.trim();
    if (!trimmed) {
      this.onClearSearch();
      return;
    }
    this.searchTerm.set(trimmed);
    this.sortColumn.set('');
    this.sortDirection.set('asc');
    this.storeStore.search({ params: { pageNumber: 1, pageSize: PAGE_SIZE }, body: { name: trimmed } });
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { search: trimmed, pageNumber: 1, orderBy: null, sortOrder: null },
      queryParamsHandling: 'merge',
    });
  }

  protected onClearSearch(): void {
    this.searchTerm.set('');
    this.sortColumn.set('');
    this.sortDirection.set('asc');
    this.storeStore.loadPage({ pageNumber: 1, pageSize: PAGE_SIZE });
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { search: null, pageNumber: null, orderBy: null, sortOrder: null },
      queryParamsHandling: 'merge',
    });
  }

  protected onPageChange(page: number): void {
    const search = this.searchTerm();
    const col = this.sortColumn();
    const sortParams = col ? { orderBy: col as SortColumn, sortOrder: this.sortDirection() } : {};
    if (search) {
      this.storeStore.search({ params: { pageNumber: page, pageSize: PAGE_SIZE, ...sortParams }, body: { name: search } });
    } else {
      this.storeStore.loadPage({ pageNumber: page, pageSize: PAGE_SIZE, ...sortParams });
    }
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { pageNumber: page },
      queryParamsHandling: 'merge',
    });
  }

  protected onSortChange(event: { column: string; direction: 'asc' | 'desc' }): void {
    this.sortColumn.set(event.column);
    this.sortDirection.set(event.direction);
    const search = this.searchTerm();
    const page = this.pageNumber();
    // Runtime guard: DataTableComponent emits sort events for any column — the
    // sortable flag is a UI hint, not a contract. VALID_SORT_COLUMNS is the
    // authoritative allowlist that keeps arbitrary column keys off the API call.
    if (!(VALID_SORT_COLUMNS as readonly string[]).includes(event.column)) return;
    const orderBy = event.column as SortColumn;
    if (search) {
      this.storeStore.search({
        params: { pageNumber: page, pageSize: PAGE_SIZE, orderBy, sortOrder: event.direction },
        body: { name: search },
      });
    } else {
      this.storeStore.loadPage({ pageNumber: page, pageSize: PAGE_SIZE, orderBy, sortOrder: event.direction });
    }
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { orderBy: event.column, sortOrder: event.direction },
      queryParamsHandling: 'merge',
    });
  }
}
