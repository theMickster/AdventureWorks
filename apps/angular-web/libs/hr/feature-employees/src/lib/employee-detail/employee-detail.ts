import { CurrencyPipe, SlicePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { Employee, EmployeeLifecycleStatus, EmployeeUpdate } from '@adventureworks-web/hr/data-access';
import {
  CardComponent,
  EmptyStateComponent,
  InputFieldComponent,
  SelectFieldComponent,
  SkeletonComponent,
  StatusBadgeComponent,
} from '@adventureworks-web/shared/ui';
import { ApiValidationError, NotificationService } from '@adventureworks-web/shared/util';
import { EMPLOYEE_STATUS_BADGE_MAP } from '../employee-status-badge';
import { extractEmployeeListNavParams } from '../employee-list-nav-params';
import { EmployeeHireModalComponent } from './employee-hire-modal';
import { EmployeeTerminateModalComponent } from './employee-terminate-modal';
import { EmployeeRehireModalComponent } from './employee-rehire-modal';

/**
 * Maps a server `propertyName` to its form control. `ExceptionHandlerMiddleware`'s `CamelCaseOptions`
 * only renames the JSON key (`PropertyName` -> `propertyName`) — the string *value* is FluentValidation's
 * raw C# property name from the `RuleFor` lambda, e.g. `"JobTitle"`, and stays PascalCase. Matches the
 * proven convention in `EmployeeCreateComponent`'s `API_PROPERTY_TO_FORM_PATH`.
 */
const SERVER_ERROR_FIELD_MAP: Record<string, string> = {
  FirstName: 'firstName',
  MiddleName: 'middleName',
  LastName: 'lastName',
  JobTitle: 'jobTitle',
  MaritalStatus: 'maritalStatus',
  Gender: 'gender',
};

/**
 * Employee detail at `/hr/employees/:id`. Mirrors `DepartmentDetailComponent` — direct `HrApiService`
 * calls, no NgRx store. Employee and lifecycle status are two independent reads, loaded in parallel
 * via `forkJoin`.
 *
 * The Hire/Terminate/Rehire actions (US-758) open modal wizards (`EmployeeHireModalComponent`,
 * `EmployeeTerminateModalComponent`, `EmployeeRehireModalComponent`) that each inject `EmployeeStore`
 * directly and call its `hireEmployee`/`terminateEmployee`/`rehireEmployee` rxMethods — a deliberate,
 * narrow exception to this component's "no NgRx store" convention, scoped to those three lifecycle
 * actions only. This component itself still reads via `HrApiService` (`getEmployee`,
 * `getLifecycleStatus`) and saves Personal Info via `HrApiService.updateEmployee` — not a precedent
 * for further store migration.
 *
 * US-757: the Personal Info card toggles between a read-only view and an in-place edit form via
 * `isEditing` — a deliberate deviation from the routed `/edit` page convention used elsewhere in HR.
 */
@Component({
  selector: 'aw-employee-detail',
  standalone: true,
  imports: [
    RouterLink,
    CurrencyPipe,
    SlicePipe,
    ReactiveFormsModule,
    CardComponent,
    SkeletonComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
    InputFieldComponent,
    SelectFieldComponent,
    EmployeeHireModalComponent,
    EmployeeTerminateModalComponent,
    EmployeeRehireModalComponent,
  ],
  templateUrl: './employee-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmployeeDetailComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly employee = signal<Employee | null>(null);
  protected readonly lifecycle = signal<EmployeeLifecycleStatus | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly hasError = signal(false);
  protected readonly isEditing = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly hireModalOpen = signal(false);
  protected readonly terminateModalOpen = signal(false);
  protected readonly rehireModalOpen = signal(false);
  // Flipped to true on first save attempt; drives field errors to show even before touch
  protected readonly submitted = signal(false);

  protected readonly personalInfoForm = this.fb.group({
    firstName: ['', [Validators.required, Validators.maxLength(50)]],
    middleName: ['', Validators.maxLength(50)],
    lastName: ['', [Validators.required, Validators.maxLength(50)]],
    jobTitle: ['', [Validators.required, Validators.maxLength(50)]],
    maritalStatus: ['', Validators.required],
    gender: ['', Validators.required],
  });

  // Bridge form status changes into the signal graph so computed() re-evaluates on validation state changes
  private readonly _formStatus = toSignal(this.personalInfoForm.statusChanges, {
    initialValue: this.personalInfoForm.status,
  });

  protected readonly statusBadgeMap = EMPLOYEE_STATUS_BADGE_MAP;

  protected readonly statusKey = computed(() => this.lifecycle()?.employmentStatus.toLowerCase() ?? '');

  /**
   * Days elapsed since termination. The API's `eligibleForRehire` field (see
   * `ReadEmployeeLifecycleStatusQueryHandler.EligibleForRehire = !CurrentFlag && terminationCount > 0`)
   * only means "was terminated at least once" — it does not implement any time-based rule. The 90-day
   * cooling-off period is a UI-only computation from `terminationDate`.
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

  /**
   * True once the employee has passed the 90-day rehire cooling-off period (US-758 AC: terminated
   * 30 days ago -> blocked with "60 more days" warning; eligible once `daysSinceTermination >= 90`).
   * Matches `EmployeeRehireModalComponent.isBlocked`'s day-90 boundary exactly — that modal is the
   * only path to actually perform a rehire, so this read-only badge must agree with its gate.
   */
  protected readonly isPastRehireWaitingPeriod = computed(() => {
    const days = this.daysSinceTermination();
    return days !== null && days >= 90;
  });

  protected readonly backQueryParams = computed(() =>
    // snapshot read is intentional: captures list nav state once at navigation time
    extractEmployeeListNavParams(this.route.snapshot.queryParams),
  );

  protected readonly firstNameErrors = computed((): Record<string, string> | null => {
    this._formStatus();
    const ctrl = this.personalInfoForm.controls.firstName;
    if (!ctrl.errors) {
      return null;
    }
    if (!this.submitted() && !ctrl.touched) {
      return null;
    }
    const errors: Record<string, string> = {};
    if (ctrl.errors['required']) {
      errors['required'] = 'First name is required.';
    }
    if (ctrl.errors['maxlength']) {
      errors['maxlength'] = 'First name cannot exceed 50 characters.';
    }
    if (ctrl.errors['server']) {
      errors['server'] = ctrl.errors['server'] as string;
    }
    return Object.keys(errors).length ? errors : null;
  });

  protected readonly middleNameErrors = computed((): Record<string, string> | null => {
    this._formStatus();
    const ctrl = this.personalInfoForm.controls.middleName;
    if (!ctrl.errors) {
      return null;
    }
    const errors: Record<string, string> = {};
    if (ctrl.errors['maxlength']) {
      errors['maxlength'] = 'Middle name cannot exceed 50 characters.';
    }
    if (ctrl.errors['server']) {
      errors['server'] = ctrl.errors['server'] as string;
    }
    return Object.keys(errors).length ? errors : null;
  });

  protected readonly lastNameErrors = computed((): Record<string, string> | null => {
    this._formStatus();
    const ctrl = this.personalInfoForm.controls.lastName;
    if (!ctrl.errors) {
      return null;
    }
    if (!this.submitted() && !ctrl.touched) {
      return null;
    }
    const errors: Record<string, string> = {};
    if (ctrl.errors['required']) {
      errors['required'] = 'Last name is required.';
    }
    if (ctrl.errors['maxlength']) {
      errors['maxlength'] = 'Last name cannot exceed 50 characters.';
    }
    if (ctrl.errors['server']) {
      errors['server'] = ctrl.errors['server'] as string;
    }
    return Object.keys(errors).length ? errors : null;
  });

  protected readonly jobTitleErrors = computed((): Record<string, string> | null => {
    this._formStatus();
    const ctrl = this.personalInfoForm.controls.jobTitle;
    if (!ctrl.errors) {
      return null;
    }
    if (!this.submitted() && !ctrl.touched) {
      return null;
    }
    const errors: Record<string, string> = {};
    if (ctrl.errors['required']) {
      errors['required'] = 'Job title is required.';
    }
    if (ctrl.errors['maxlength']) {
      errors['maxlength'] = 'Job title cannot exceed 50 characters.';
    }
    if (ctrl.errors['server']) {
      errors['server'] = ctrl.errors['server'] as string;
    }
    return Object.keys(errors).length ? errors : null;
  });

  protected readonly maritalStatusErrors = computed((): Record<string, string> | null => {
    this._formStatus();
    const ctrl = this.personalInfoForm.controls.maritalStatus;
    if (!ctrl.errors) {
      return null;
    }
    if (!this.submitted() && !ctrl.touched) {
      return null;
    }
    const errors: Record<string, string> = {};
    if (ctrl.errors['required']) {
      errors['required'] = 'Marital status is required.';
    }
    if (ctrl.errors['server']) {
      errors['server'] = ctrl.errors['server'] as string;
    }
    return Object.keys(errors).length ? errors : null;
  });

  protected readonly genderErrors = computed((): Record<string, string> | null => {
    this._formStatus();
    const ctrl = this.personalInfoForm.controls.gender;
    if (!ctrl.errors) {
      return null;
    }
    if (!this.submitted() && !ctrl.touched) {
      return null;
    }
    const errors: Record<string, string> = {};
    if (ctrl.errors['required']) {
      errors['required'] = 'Gender is required.';
    }
    if (ctrl.errors['server']) {
      errors['server'] = ctrl.errors['server'] as string;
    }
    return Object.keys(errors).length ? errors : null;
  });

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

  protected onHireClick(): void {
    this.hireModalOpen.set(true);
  }

  protected onTerminateClick(): void {
    this.terminateModalOpen.set(true);
  }

  protected onRehireClick(): void {
    this.rehireModalOpen.set(true);
  }

  /** Re-fetches lifecycle status after a successful lifecycle action and shows a success toast. */
  private refreshLifecycle(successMessage: string): void {
    const id = this.employee()?.id;
    if (!id) {
      return;
    }
    this.hrApi
      .getLifecycleStatus(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (lifecycle) => {
          this.lifecycle.set(lifecycle);
          this.notificationService.success(successMessage);
        },
        error: () => this.notificationService.error('Failed to refresh employee status. Please refresh the page.'),
      });
  }

  protected onHireConfirmed(): void {
    this.refreshLifecycle('Employee hired successfully.');
  }

  protected onTerminateConfirmed(): void {
    this.refreshLifecycle('Employee terminated successfully.');
  }

  protected onRehireConfirmed(): void {
    this.refreshLifecycle('Employee rehired successfully.');
  }

  /**
   * Populates the form from the already-loaded `employee()` signal — no extra API call needed.
   * Re-entering edit mode always rebuilds the form fresh, which is also how Cancel "reverts":
   * the read-only view renders from `employee()`, which is never mutated while editing.
   */
  protected onEditClick(): void {
    const e = this.employee();
    if (!e) {
      return;
    }
    this.personalInfoForm.reset({
      firstName: e.firstName,
      middleName: e.middleName ?? '',
      lastName: e.lastName,
      jobTitle: e.jobTitle,
      maritalStatus: e.maritalStatus,
      gender: e.gender,
    });
    this.submitted.set(false);
    this.isEditing.set(true);
  }

  protected onCancelClick(): void {
    this.isEditing.set(false);
    this.submitted.set(false);
  }

  protected onSaveClick(): void {
    if (this.isSaving()) {
      return;
    }
    this.submitted.set(true);
    if (this.personalInfoForm.invalid) {
      return;
    }

    const e = this.employee();
    if (!e) {
      return;
    }
    const { firstName, middleName, lastName, jobTitle, maritalStatus, gender } = this.personalInfoForm.value;
    const model: EmployeeUpdate = {
      id: e.id,
      firstName: firstName ?? '',
      middleName: middleName || null,
      lastName: lastName ?? '',
      // title/suffix are not editable in this form — pass through unchanged from the loaded
      // employee() so the PUT's full-replace semantics don't null out an existing honorific/suffix.
      title: e.title,
      suffix: e.suffix,
      jobTitle: jobTitle ?? '',
      maritalStatus: (maritalStatus ?? 'S') as 'M' | 'S',
      gender: (gender ?? 'M') as 'M' | 'F',
    };

    this.isSaving.set(true);
    this.hrApi
      .updateEmployee(e.id, model)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.employee.set(updated);
          this.isSaving.set(false);
          this.isEditing.set(false);
          this.notificationService.success('Employee updated successfully.');
        },
        error: (err: unknown) => {
          this.isSaving.set(false);
          if (err instanceof ApiValidationError) {
            let matched = false;
            for (const ve of err.errors) {
              const controlName = SERVER_ERROR_FIELD_MAP[ve.propertyName];
              if (!controlName) {
                continue;
              }
              const ctrl = this.personalInfoForm.get(controlName);
              ctrl?.setErrors({ server: ve.errorMessage });
              ctrl?.markAsTouched();
              matched = true;
            }
            if (matched) {
              return;
            }
          }
          this.notificationService.error('Failed to update employee. Please try again.');
        },
      });
  }
}
