import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { DepartmentUpdate } from '@adventureworks-web/hr/data-access';
import type { Department } from '@adventureworks-web/shared/data-access';
import { InputFieldComponent, SelectFieldComponent, SkeletonComponent } from '@adventureworks-web/shared/ui';
import { ApiValidationError, extractListNavParams, NotificationService } from '@adventureworks-web/shared/util';
import { extractGroupNames } from '../extract-group-names';

/**
 * Edit form for an existing department at `hr/departments/:id/edit`. Direct `HrApiService`
 * calls — no NgRx store — mirroring `StoreEditComponent`.
 *
 * `isLoading` (initial `forkJoin` load) and `isSaving` (update call in flight) are separate
 * signals — do not merge them.
 *
 * Duplicate-name handling: the update endpoint's FluentValidation error is `errorCode: "Rule-06"`
 * with an EMPTY `propertyName` (a whole-model rule, not a per-field one) — see
 * `WriteDepartmentController.PutAsync`. We branch on `errorCode` first; a lookup keyed on
 * `propertyName` would never match this case.
 */
@Component({
  selector: 'aw-department-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputFieldComponent, SelectFieldComponent, SkeletonComponent],
  templateUrl: './department-edit.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartmentEditComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly departmentId: number = (() => {
    const rawId = this.route.snapshot.paramMap.get('id');
    return Math.trunc(Number(rawId));
  })();
  protected readonly groupNames = signal<string[]>([]);
  // Flipped to true on first submit attempt; drives field errors to show even before touch
  protected readonly submitted = signal(false);

  protected readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    groupName: ['', Validators.required],
  });

  // Bridge form status changes into the signal graph so computed() re-evaluates on validation state changes
  private readonly _formStatus = toSignal(this.form.statusChanges, { initialValue: this.form.status });

  protected readonly groupNameOptions = computed(() =>
    this.groupNames().map((name) => ({ value: name, label: name })),
  );

  protected readonly nameErrors = computed((): Record<string, string> | null => {
    this._formStatus();
    const ctrl = this.form.controls.name;
    if (!ctrl.errors) {
      return null;
    }
    if (!this.submitted() && !ctrl.touched) {
      return null;
    }
    const errors: Record<string, string> = {};
    if (ctrl.errors['required']) {
      errors['required'] = 'Name is required.';
    }
    if (ctrl.errors['maxlength']) {
      errors['maxlength'] = 'Name cannot exceed 50 characters.';
    }
    if (ctrl.errors['server']) {
      errors['server'] = ctrl.errors['server'] as string;
    }
    return Object.keys(errors).length ? errors : null;
  });

  protected readonly groupNameErrors = computed((): Record<string, string> | null => {
    this._formStatus();
    const ctrl = this.form.controls.groupName;
    if (!ctrl.errors) {
      return null;
    }
    if (!this.submitted() && !ctrl.touched) {
      return null;
    }
    const errors: Record<string, string> = {};
    if (ctrl.errors['required']) {
      errors['required'] = 'Group name is required.';
    }
    return Object.keys(errors).length ? errors : null;
  });

  protected readonly backQueryParams = computed(() =>
    // snapshot read is intentional: captures list nav state once at navigation time
    extractListNavParams(this.route.snapshot.queryParams),
  );

  ngOnInit(): void {
    if (!this.departmentId || this.departmentId <= 0) {
      void this.router.navigate(['/hr/departments']);
      return;
    }

    forkJoin({
      department: this.hrApi.getDepartment(this.departmentId),
      departments: this.hrApi.getDepartments(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ department, departments }: { department: Department; departments: Department[] }) => {
          this.groupNames.set(extractGroupNames(departments));
          this.form.patchValue({
            name: department.name,
            groupName: department.groupName,
          });
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.notificationService.error('Failed to load department. Please try again.');
        },
      });
  }

  protected onSubmit(): void {
    if (this.isSaving()) {
      return;
    }
    this.form.controls.name.setValue((this.form.value.name ?? '').trim());
    this.submitted.set(true);
    if (this.form.invalid) {
      return;
    }

    const model: DepartmentUpdate = {
      id: this.departmentId,
      name: this.form.value.name ?? '',
      groupName: this.form.value.groupName ?? '',
    };

    this.isSaving.set(true);
    this.hrApi
      .updateDepartment(this.departmentId, model)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (department) => {
          this.notificationService.success('Department updated successfully.');
          void this.router.navigate(['/hr/departments', department.id]);
        },
        error: (err: unknown) => {
          this.isSaving.set(false);
          if (err instanceof ApiValidationError) {
            const duplicateNameError = err.errors.find((ve) => ve.errorCode === 'Rule-06');
            if (duplicateNameError) {
              this.form.controls.name.setErrors({ server: duplicateNameError.errorMessage });
              this.form.controls.name.markAsTouched();
              return;
            }
          }
          this.notificationService.error('Failed to update department. Please try again.');
        },
      });
  }
}
