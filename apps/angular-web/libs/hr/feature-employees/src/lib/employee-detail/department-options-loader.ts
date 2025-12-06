import { computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { Department } from '@adventureworks-web/shared/data-access';
import { NotificationService } from '@adventureworks-web/shared/util';

/**
 * Loads the department list once and exposes `{ value, label }` select options. Shared by the
 * Hire and Rehire lifecycle wizard modals (Terminate has no department field). Must be
 * constructed within an injection context (a component field initializer or constructor) since
 * it calls `inject()`.
 */
export function createDepartmentOptionsLoader() {
  const hrApi = inject(HrApiService);
  const notificationService = inject(NotificationService);
  const destroyRef = inject(DestroyRef);

  const departments = signal<Department[]>([]);
  const departmentOptions = computed(() => departments().map((d) => ({ value: d.id, label: d.name })));

  function load(): void {
    hrApi
      .getDepartments()
      .pipe(takeUntilDestroyed(destroyRef))
      .subscribe({
        next: (result) => departments.set(result),
        error: () => notificationService.error('Failed to load departments. Please try again.'),
      });
  }

  return { departments, departmentOptions, load };
}
