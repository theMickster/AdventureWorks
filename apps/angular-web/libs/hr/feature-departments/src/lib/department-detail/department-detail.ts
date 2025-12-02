import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { DepartmentHeadcount, Employee } from '@adventureworks-web/hr/data-access';
import type { Department } from '@adventureworks-web/shared/data-access';
import { CardComponent, EmptyStateComponent, SkeletonComponent } from '@adventureworks-web/shared/ui';
import { extractListNavParams, NotificationService } from '@adventureworks-web/shared/util';

/**
 * Department detail at `/hr/departments/:id`. Uses `HrApiService` directly — no NgRx store —
 * mirroring `StoreDetailComponent`'s single-entity, no-shared-state pattern.
 *
 * Department info, headcount, and the employee roster are three independent reads, loaded in
 * parallel via `forkJoin`.
 */
@Component({
  selector: 'aw-department-detail',
  standalone: true,
  imports: [RouterLink, CardComponent, SkeletonComponent, EmptyStateComponent],
  templateUrl: './department-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartmentDetailComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly department = signal<Department | null>(null);
  protected readonly headcount = signal<DepartmentHeadcount | null>(null);
  protected readonly employees = signal<Employee[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly hasError = signal(false);

  protected readonly rosterRows = computed(() =>
    this.employees().map((e) => ({
      id: e.id,
      name: `${e.firstName} ${e.lastName}`,
      jobTitle: e.jobTitle,
    })),
  );

  protected readonly backQueryParams = computed(() =>
    // snapshot read is intentional: captures list nav state once at navigation time
    extractListNavParams(this.route.snapshot.queryParams),
  );

  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    const id = Math.trunc(Number(rawId));
    if (!id || id <= 0) {
      void this.router.navigate(['/hr/departments']);
      return;
    }

    this.isLoading.set(true);
    forkJoin({
      department: this.hrApi.getDepartment(id),
      headcount: this.hrApi.getDepartmentHeadcount(id),
      // pageSize 100 is the API's documented max (ReadDepartmentReportingController) — there's
      // no roster pagination UI, so this fetches the largest single page available.
      employees: this.hrApi.getDepartmentEmployees(id, { pageSize: 100 }),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.department.set(result.department);
          this.headcount.set(result.headcount);
          this.employees.set(result.employees);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.hasError.set(true);
          this.notificationService.error('Failed to load department. Please try again.');
        },
      });
  }
}
