import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { Department } from '@adventureworks-web/shared/data-access';
import { ColumnDefDirective, DataTableComponent } from '@adventureworks-web/shared/ui';
import type { ColumnConfig } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';
import { extractGroupNames } from '../extract-group-names';

/**
 * Department list at `/hr/departments`. Direct `HrApiService` call, no NgRx store — the
 * department count is small (16 rows) and bounded, mirroring `SalesPersonListComponent`'s
 * client-side-only justification. No sort is offered; the only interaction is the
 * group-name filter dropdown.
 *
 * The group-name filter options are derived at runtime from the distinct `groupName` values
 * in the loaded department list, not hardcoded — department data already has a backing
 * endpoint, so a second source of truth for group names would risk drift.
 */
@Component({
  selector: 'aw-department-list',
  standalone: true,
  imports: [RouterLink, ColumnDefDirective, DataTableComponent],
  templateUrl: './department-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartmentListComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly isLoading = signal(false);
  protected readonly hasError = signal(false);
  protected readonly departments = signal<Department[]>([]);
  protected readonly groupNameFilter = signal('');

  constructor() {
    // effect() is used instead of a template binding because error notifications
    // are side effects (toast dispatch) that must fire outside the render cycle.
    effect(() => {
      if (this.hasError()) {
        this.notificationService.error('Failed to load departments. Please try again.');
      }
    });
  }

  protected readonly columns: ColumnConfig[] = [
    { key: 'name', label: 'Name' },
    { key: 'groupName', label: 'Group Name' },
    { key: 'modifiedDate', label: 'Modified Date' },
    { key: 'view', label: '', cellClass: 'text-right' },
  ];

  protected readonly groupNameOptions = computed(() => extractGroupNames(this.departments()));

  protected readonly filteredDepartments = computed(() => {
    const filter = this.groupNameFilter();
    const items = this.departments();
    return filter ? items.filter((d) => d.groupName === filter) : items;
  });

  protected readonly rows = computed((): Record<string, unknown>[] =>
    this.filteredDepartments().map((d) => ({
      id: d.id,
      name: d.name,
      groupName: d.groupName,
      modifiedDate: new Date(d.modifiedDate).toLocaleDateString('en-US'),
    })),
  );

  ngOnInit(): void {
    this.isLoading.set(true);
    this.hrApi
      .getDepartments()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (departments) => {
          this.departments.set(departments);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.hasError.set(true);
        },
      });
  }

  protected onGroupNameFilterChange(value: string): void {
    this.groupNameFilter.set(value);
  }
}
