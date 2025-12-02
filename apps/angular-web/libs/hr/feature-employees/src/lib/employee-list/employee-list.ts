import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeStore } from '@adventureworks-web/hr/data-access';
import type { EmployeeSearchBody } from '@adventureworks-web/hr/data-access';
import { ColumnDefDirective, DataTableComponent, SelectFieldComponent, StatusBadgeComponent } from '@adventureworks-web/shared/ui';
import type { ColumnConfig } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';
import { EMPLOYEE_STATUS_BADGE_MAP } from '../employee-status-badge';

const PAGE_SIZE = 10;

/** Mutable filter accumulator built from the filter-bar form before merging into the store request. */
interface EmployeeFilters {
  name?: string;
  currentFlag?: boolean;
}

/**
 * Paginated, filterable employee list at `/hr/employees`. Mirrors `OrderListComponent`'s server-side
 * reactive URL-param pattern — the employee dataset (290 rows) is large/unbounded, unlike
 * `DepartmentListComponent`'s small client-side-filtered dataset.
 *
 * The `route.queryParams` subscription in `ngOnInit` is the sole driver of `EmployeeStore.loadPage()`
 * (no filters active) or `EmployeeStore.search()` (name and/or status filter active). Action methods
 * only write to the URL via `router.navigate`.
 */
@Component({
  selector: 'aw-employee-list',
  standalone: true,
  imports: [ReactiveFormsModule, DataTableComponent, ColumnDefDirective, SelectFieldComponent, StatusBadgeComponent],
  templateUrl: './employee-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmployeeListComponent implements OnInit {
  private readonly employeeStore = inject(EmployeeStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly isLoading = this.employeeStore.isLoading;
  protected readonly pageNumber = this.employeeStore.pageNumber;
  protected readonly pageSize = this.employeeStore.pageSize;
  protected readonly totalPages = this.employeeStore.totalPages;
  protected readonly totalRecords = this.employeeStore.totalRecords;

  protected readonly statusOptions = [
    { value: 'active', label: 'Active' },
    { value: 'terminated', label: 'Terminated' },
  ];

  protected readonly filterForm = this.fb.group({
    name: [''],
    status: [''],
  });

  protected readonly columns: ColumnConfig[] = [
    { key: 'name', label: 'Name' },
    { key: 'jobTitle', label: 'Job Title' },
    { key: 'department', label: 'Department' },
    { key: 'status', label: 'Status' },
    { key: 'hireDate', label: 'Hire Date' },
    { key: 'view', label: '', cellClass: 'text-right' },
  ];

  protected readonly statusBadgeMap = EMPLOYEE_STATUS_BADGE_MAP;

  protected readonly rows = computed(() =>
    this.employeeStore.entities().map((employee): Record<string, unknown> => ({
      id: employee.id,
      name: `${employee.firstName} ${employee.lastName}`,
      jobTitle: employee.jobTitle,
      department: employee.currentDepartment ?? '—',
      status: employee.currentFlag ? 'Active' : 'Terminated',
      statusKey: employee.currentFlag ? 'active' : 'terminated',
      hireDate: employee.hireDate.slice(0, 10),
    })),
  );

  constructor() {
    effect(() => {
      if (this.employeeStore.hasError()) {
        this.notificationService.error('Failed to load employees. Please try again.');
      }
    });
  }

  ngOnInit(): void {
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const stringParams = params as Record<string, string>;
      this.restoreFiltersFromUrl(stringParams);
      this.loadFromUrl(stringParams);
    });
  }

  /** Writes the current name/status filters plus pageNumber=1 to the URL (merge); the subscription reloads. */
  protected onApplyFilters(): void {
    const filters = this.readFilters();
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { ...this.filterUrlParams(filters), pageNumber: 1 },
      queryParamsHandling: 'merge',
    });
  }

  /** Clears all filters, returning to the default unfiltered list at page 1. */
  protected onResetFilters(): void {
    this.filterForm.reset({ name: '', status: '' });
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { name: null, status: null, pageNumber: null },
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

  /** Navigates to the employee detail view at /hr/employees/:id. */
  protected onRowClick(row: Record<string, unknown>): void {
    void this.router.navigate(['/hr/employees', row['id']]);
  }

  /** Restores filter-bar form values from the emitted URL params. */
  private restoreFiltersFromUrl(params: Record<string, string>): void {
    this.filterForm.setValue({
      name: params['name'] ?? '',
      status: params['status'] ?? '',
    });
  }

  /** Issues a load using URL state: `loadPage` when no filters are active, `search` otherwise. */
  private loadFromUrl(params: Record<string, string>): void {
    const pageNumber = Math.max(1, Math.trunc(Number(params['pageNumber'])) || 1);
    const filters = this.parseFilterParams(params);

    if (!filters.name && filters.currentFlag === undefined) {
      this.employeeStore.loadPage({ pageNumber, pageSize: PAGE_SIZE });
      return;
    }

    let firstName: string | undefined;
    let lastName: string | undefined;
    if (filters.name) {
      const [first, ...rest] = filters.name.trim().split(/\s+/);
      firstName = first;
      if (rest.length > 0) {
        lastName = rest.join(' ');
      }
    }

    const body: EmployeeSearchBody = {
      ...(firstName ? { firstName } : {}),
      ...(lastName ? { lastName } : {}),
      ...(filters.currentFlag !== undefined ? { currentFlag: filters.currentFlag } : {}),
    };

    this.employeeStore.search({ params: { pageNumber, pageSize: PAGE_SIZE }, body });
  }

  /** Builds the filter accumulator from a string-keyed source (URL params or form values). */
  private parseFilterParams(src: Record<string, string>): EmployeeFilters {
    const filters: EmployeeFilters = {};
    const trimmedName = src['name']?.trim();
    if (trimmedName) {
      filters.name = trimmedName;
    }
    if (src['status'] === 'active') {
      filters.currentFlag = true;
    } else if (src['status'] === 'terminated') {
      filters.currentFlag = false;
    }
    return filters;
  }

  /** Reads the filter form, omitting cleared (empty) fields. */
  private readFilters(): EmployeeFilters {
    return this.parseFilterParams(this.filterForm.getRawValue() as Record<string, string>);
  }

  /** Maps applied filters to URL query params, nulling any cleared field so it is removed on merge. */
  private filterUrlParams(filters: EmployeeFilters): Record<string, string | null> {
    return {
      name: filters.name ?? null,
      status: filters.currentFlag === undefined ? null : filters.currentFlag ? 'active' : 'terminated',
    };
  }
}
