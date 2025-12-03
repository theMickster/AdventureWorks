import { CurrencyPipe, SlicePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { Employee, EmployeeLifecycleStatus } from '@adventureworks-web/hr/data-access';
import { CardComponent, EmptyStateComponent, SkeletonComponent, StatusBadgeComponent } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';
import { EMPLOYEE_STATUS_BADGE_MAP } from '../employee-status-badge';
import { extractEmployeeListNavParams } from '../employee-list-nav-params';

/**
 * Employee detail at `/hr/employees/:id`. Mirrors `DepartmentDetailComponent` — direct `HrApiService`
 * calls, no NgRx store. Employee and lifecycle status are two independent reads, loaded in parallel
 * via `forkJoin`.
 *
 * The Terminate/Rehire buttons are contextual on `lifecycle().employmentStatus` and currently only
 * show an informational notification — Stories 757-758 own the actual multi-step wizard forms.
 */
@Component({
  selector: 'aw-employee-detail',
  standalone: true,
  imports: [RouterLink, CurrencyPipe, SlicePipe, CardComponent, SkeletonComponent, EmptyStateComponent, StatusBadgeComponent],
  templateUrl: './employee-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmployeeDetailComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly employee = signal<Employee | null>(null);
  protected readonly lifecycle = signal<EmployeeLifecycleStatus | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly hasError = signal(false);

  protected readonly statusBadgeMap = EMPLOYEE_STATUS_BADGE_MAP;

  protected readonly statusKey = computed(() => this.lifecycle()?.employmentStatus.toLowerCase() ?? '');

  /**
   * Days elapsed since termination. The API's `eligibleForRehire` field (see
   * `ReadEmployeeLifecycleStatusQueryHandler.EligibleForRehire = !CurrentFlag && terminationCount > 0`)
   * only means "was terminated at least once" — it does not implement any time-based rule. The 90-day
   * window is a UI-only computation from `terminationDate`.
   *
   * `terminationDate` is a date-only string; both sides are normalized to UTC calendar midnight so the
   * day count is timezone-stable rather than drifting with the viewer's local offset.
   */
  protected readonly daysSinceTermination = computed(() => {
    const terminationDate = this.lifecycle()?.terminationDate;
    if (!terminationDate) {
      return null;
    }
    const today = new Date();
    const todayUtc = Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate());
    const elapsedMs = todayUtc - new Date(terminationDate).getTime();
    return Math.floor(elapsedMs / (1000 * 60 * 60 * 24));
  });

  /** True when the employee is within the 90-day rehire-eligibility window from their termination date. */
  protected readonly isEligibleForRehireWithin90Days = computed(() => {
    const days = this.daysSinceTermination();
    return days !== null && days <= 90;
  });

  protected readonly backQueryParams = computed(() =>
    // snapshot read is intentional: captures list nav state once at navigation time
    extractEmployeeListNavParams(this.route.snapshot.queryParams),
  );

  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    const id = Math.trunc(Number(rawId));
    if (!id || id <= 0) {
      void this.router.navigate(['/hr/employees']);
      return;
    }

    this.isLoading.set(true);
    forkJoin({
      employee: this.hrApi.getEmployee(id),
      lifecycle: this.hrApi.getLifecycleStatus(id),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.employee.set(result.employee);
          this.lifecycle.set(result.lifecycle);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.hasError.set(true);
          this.notificationService.error('Failed to load employee. Please try again.');
        },
      });
  }

  /** Stub handler — the Terminate wizard (US-757) is not yet built. */
  protected onTerminateClick(): void {
    this.notificationService.info('Terminate employee is not yet available.');
  }

  /** Stub handler — the Rehire wizard (US-758) is not yet built. */
  protected onRehireClick(): void {
    this.notificationService.info('Rehire employee is not yet available.');
  }
}
