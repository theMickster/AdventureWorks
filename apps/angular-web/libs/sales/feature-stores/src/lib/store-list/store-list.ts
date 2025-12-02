import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { StoreStore } from '@adventureworks-web/sales/data-access';
import { ColumnDefDirective, DataTableComponent } from '@adventureworks-web/shared/ui';
import type { ColumnConfig } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';

const PAGE_SIZE = 25;
const VALID_SORT_COLUMNS = ['name'] as const;

@Component({
  selector: 'aw-store-list',
  standalone: true,
  imports: [DataTableComponent, RouterLink, ColumnDefDirective],
  templateUrl: './store-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StoreListComponent implements OnInit {
  private readonly storeStore = inject(StoreStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

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
    { key: 'view', label: '', sortable: false, cellClass: 'text-right' },
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
    this.route.queryParams
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const search = params['search'] as string | undefined;
        const pageNumber = Math.max(1, Math.trunc(Number(params['pageNumber'])) || 1);
        const rawOrderBy = params['orderBy'] as string | undefined;
        const orderBy = rawOrderBy === 'name' ? ('name' as const) : undefined;
        const sortOrder = (params['sortOrder'] === 'desc' ? 'desc' : 'asc') as 'asc' | 'desc';

        this.sortColumn.set(orderBy ?? '');
        this.sortDirection.set(orderBy ? sortOrder : 'asc');
        this.searchTerm.set(search ?? '');

        const sortParams = orderBy ? { orderBy, sortOrder } : {};
        if (search) {
          this.storeStore.search({ params: { pageNumber, pageSize: PAGE_SIZE, ...sortParams }, body: { name: search } });
        } else {
          this.storeStore.loadPage({ pageNumber, pageSize: PAGE_SIZE, ...sortParams });
        }
      });
  }

  protected onSearch(term: string): void {
    const trimmed = term.trim();
    if (!trimmed) {
      this.onClearSearch();
      return;
    }
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { search: trimmed, pageNumber: 1, orderBy: null, sortOrder: null },
      queryParamsHandling: 'merge',
    });
  }

  protected onClearSearch(): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { search: null, pageNumber: null, orderBy: null, sortOrder: null },
      queryParamsHandling: 'merge',
    });
  }

  protected onPageChange(page: number): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { pageNumber: page },
      queryParamsHandling: 'merge',
    });
  }

  protected onSortChange(event: { column: string; direction: 'asc' | 'desc' }): void {
    if (!(VALID_SORT_COLUMNS as readonly string[]).includes(event.column)) { return; }
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { orderBy: event.column, sortOrder: event.direction },
      queryParamsHandling: 'merge',
    });
  }
}
