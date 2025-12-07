import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomerStore } from '@adventureworks-web/sales/data-access';
import { ColumnDefDirective, DataTableComponent, StatusBadgeComponent } from '@adventureworks-web/shared/ui';
import type { ColumnConfig } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';
import { CurrencyPipe } from '@angular/common';

const PAGE_SIZE = 25;

@Component({
  selector: 'aw-customer-list',
  standalone: true,
  imports: [DataTableComponent, ColumnDefDirective, StatusBadgeComponent, CurrencyPipe],
  templateUrl: './customer-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerListComponent implements OnInit {
  private readonly customerStore = inject(CustomerStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly searchTerm = signal('');

  protected readonly isLoading = this.customerStore.isLoading;
  protected readonly pageNumber = this.customerStore.pageNumber;
  protected readonly pageSize = this.customerStore.pageSize;
  protected readonly totalPages = this.customerStore.totalPages;
  protected readonly totalRecords = this.customerStore.totalRecords;

  protected readonly columns: ColumnConfig[] = [
    { key: 'ltvRank', label: 'Rank', sortable: false },
    { key: 'displayName', label: 'Name / Store', sortable: false },
    { key: 'customerType', label: 'Type', sortable: false },
    { key: 'totalSpend', label: 'Total Spend', sortable: false, headerClass: 'text-right', cellClass: 'text-right' },
    { key: 'orderCount', label: 'Orders', sortable: false },
    { key: 'status', label: 'Status', sortable: false },
    { key: 'view', label: '', sortable: false, cellClass: 'text-right' },
  ];

  /** Flattens customer entities into flat rows for DataTableComponent, deriving a lowercase statusKey for the badge. */
  protected readonly rows = computed(() =>
    this.customerStore.entities().map((customer): Record<string, unknown> => ({
      customerId: customer.customerId,
      ltvRank: customer.ltvRank,
      displayName: customer.displayName,
      customerType: customer.customerType,
      totalSpend: customer.totalSpend,
      orderCount: customer.orderCount,
      statusKey: customer.isInactive ? 'inactive' : 'active',
    })),
  );

  constructor() {
    effect(() => {
      if (this.customerStore.hasError()) {
        this.notificationService.error('Failed to load customers. Please try again.');
      }
    });
  }

  ngOnInit(): void {
    this.route.queryParams
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const search = params['search'] as string | undefined;
        const pageNumber = Math.max(1, Math.trunc(Number(params['pageNumber'])) || 1);

        this.searchTerm.set(search ?? '');

        this.customerStore.loadPage({ pageNumber, pageSize: PAGE_SIZE, ...(search ? { search } : {}) });
      });
  }

  protected onSearch(term: string): void {
    const trimmed = term.trim();
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { search: trimmed || null, pageNumber: 1 },
      queryParamsHandling: 'merge',
    });
  }

  protected onClearSearch(): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { search: null, pageNumber: null },
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

  /** Routes a clicked row to the customer detail view at /sales/customers/:customerId (detail page not yet built). */
  protected onRowClick(row: Record<string, unknown>): void {
    void this.router.navigate(['/sales/customers', row['customerId']]);
  }
}
