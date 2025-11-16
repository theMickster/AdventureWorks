import { ChangeDetectionStrategy, Component, computed, effect, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { SalesPersonStore } from '@adventureworks-web/sales/data-access';
import { DataTableComponent } from '@adventureworks-web/shared/ui';
import type { ColumnConfig } from '@adventureworks-web/shared/ui';
import type { SalesPerson } from '@adventureworks-web/sales/data-access';
import { NotificationService } from '@adventureworks-web/shared/util';

const VALID_SORT_COLUMNS = ['name', 'jobTitle', 'territory', 'salesYtd', 'bonus', 'commissionPct'] as const;
const PAGE_SIZE = 25;
type SortColumn = (typeof VALID_SORT_COLUMNS)[number];

/**
 * Returns a sortable value for the given column key.
 * Text columns return a lowercased string so the comparator sorts case-insensitively.
 * Numeric columns return the raw number so numeric ordering is preserved.
 */
function getRowValue(p: SalesPerson, col: string): string | number {
  switch (col) {
    case 'name': return `${p.firstName} ${p.lastName}`.toLowerCase();
    case 'jobTitle': return p.jobTitle.toLowerCase();
    case 'territory': return (p.territoryName ?? '').toLowerCase();
    case 'salesYtd': return p.salesYtd;
    case 'bonus': return p.bonus;
    case 'commissionPct': return p.commissionPct;
    default: return '';
  }
}

function compare(a: string | number, b: string | number): number {
  if (a < b) { return -1; }
  if (a > b) { return 1; }
  return 0;
}

@Component({
  selector: 'aw-sales-person-list',
  standalone: true,
  imports: [DataTableComponent],
  templateUrl: './sales-person-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesPersonListComponent implements OnInit {
  private readonly salesPersonStore = inject(SalesPersonStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notificationService = inject(NotificationService);

  protected readonly isLoading = this.salesPersonStore.isLoading;
  protected readonly searchTerm = signal('');
  protected readonly sortColumn = signal('');
  protected readonly sortDirection = signal<'asc' | 'desc'>('asc');

  constructor() {
    // effect() is used instead of a template binding because error notifications
    // are side effects (toast dispatch) that must fire outside the render cycle.
    // A template expression cannot call imperative service methods safely.
    effect(() => {
      if (this.salesPersonStore.hasError()) {
        this.notificationService.error('Failed to load sales persons. Please try again.');
      }
    });
  }

  protected readonly columns: ColumnConfig[] = [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'jobTitle', label: 'Job Title', sortable: true },
    { key: 'territory', label: 'Territory', sortable: true },
    { key: 'salesYtd', label: 'Sales YTD', sortable: true },
    { key: 'bonus', label: 'Bonus', sortable: true },
    { key: 'commissionPct', label: 'Commission', sortable: true },
  ];

  protected readonly rows = computed((): Record<string, unknown>[] => {
    const term = this.searchTerm().toLowerCase().trim();
    const col = this.sortColumn();
    const dir = this.sortDirection();

    let items = this.salesPersonStore.entities();

    if (term) {
      items = items.filter(
        (p) =>
          `${p.firstName} ${p.lastName}`.toLowerCase().includes(term) ||
          p.jobTitle.toLowerCase().includes(term),
      );
    }

    if (col && (VALID_SORT_COLUMNS as readonly string[]).includes(col)) {
      items = [...items].sort((a, b) => {
        const aVal = getRowValue(a, col);
        const bVal = getRowValue(b, col);
        return dir === 'asc' ? compare(aVal, bVal) : compare(bVal, aVal);
      });
    }

    return items.map((p) => ({
      id: p.id,
      name: `${p.firstName} ${p.lastName}`,
      jobTitle: p.jobTitle,
      territory: p.territoryName ?? '—',
      salesYtd: p.salesYtd.toLocaleString('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }),
      bonus: p.bonus.toLocaleString('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }),
      commissionPct: `${(p.commissionPct * 100).toFixed(2)}%`,
    }));
  });

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    const search = params['search'] as string | undefined;
    const rawOrderBy = params['orderBy'] as string | undefined;
    const sortOrder: 'asc' | 'desc' = params['sortOrder'] === 'desc' ? 'desc' : 'asc';

    if (search) {
      this.searchTerm.set(search);
    }

    const orderBy = (VALID_SORT_COLUMNS as readonly string[]).includes(rawOrderBy ?? '')
      ? (rawOrderBy as SortColumn)
      : undefined;

    if (orderBy) {
      this.sortColumn.set(orderBy);
      this.sortDirection.set(sortOrder);
    }

    this.salesPersonStore.loadPage({ pageNumber: 1, pageSize: PAGE_SIZE });
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
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { search: trimmed, orderBy: null, sortOrder: null },
      queryParamsHandling: 'merge',
    });
  }

  protected onClearSearch(): void {
    this.searchTerm.set('');
    this.sortColumn.set('');
    this.sortDirection.set('asc');
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { search: null, orderBy: null, sortOrder: null },
      queryParamsHandling: 'merge',
    });
  }

  /**
   * Handles live input events from the search field.
   * When the value is empty (keyboard clear), delegates to `onClearSearch()` rather than
   * just clearing `searchTerm` so the URL query params are also removed.
   */
  protected onInputChange(value: string): void {
    const trimmed = value.trim();
    if (!trimmed) {
      this.onClearSearch();
      return;
    }
    this.searchTerm.set(trimmed);
  }

  protected onSortChange(event: { column: string; direction: 'asc' | 'desc' }): void {
    if (!(VALID_SORT_COLUMNS as readonly string[]).includes(event.column)) {
      return;
    }
    this.sortColumn.set(event.column);
    this.sortDirection.set(event.direction);
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { orderBy: event.column, sortOrder: event.direction },
      queryParamsHandling: 'merge',
    });
  }
}
