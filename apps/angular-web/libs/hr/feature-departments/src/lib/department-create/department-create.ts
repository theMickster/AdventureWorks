import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { DepartmentCreate } from '@adventureworks-web/hr/data-access';
import type { Department } from '@adventureworks-web/shared/data-access';
import { InputFieldComponent, SelectFieldComponent } from '@adventureworks-web/shared/ui';
import { ApiValidationError, NotificationService } from '@adventureworks-web/shared/util';
import { extractGroupNames } from '../extract-group-names';

/**
 * Create form for a new department at `hr/departments/new`. Direct `HrApiService` call — no
 * NgRx store — mirroring `StoreCreateComponent`.
 *
 * Group name is a constrained dropdown, not free text: the ADO AC requires "group name from
 * allowed values." Options are derived at runtime from the distinct `groupName` values already
 * present in the department list — loaded once in `ngOnInit` for this sole purpose.
 *
 * Duplicate-name handling: the API rejects a duplicate name with a 400 whose FluentValidation
 * `errorCode` is `Rule-05` (see `WriteDepartmentController.PostAsync`). We branch on `errorCode`
 * first, not `propertyName`, because the analogous update-path error (`Rule-06`) reports an
 * empty `propertyName` — a naive property-keyed lookup would silently swallow that case.
 */
@Component({
  selector: 'aw-department-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputFieldComponent, SelectFieldComponent],
  templateUrl: './department-create.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartmentCreateComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly isSaving = signal(false);
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

  ngOnInit(): void {
    this.hrApi
      .getDepartments()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (departments: Department[]) => {
          this.groupNames.set(extractGroupNames(departments));
        },
        error: () => {
          this.notificationService.error('Failed to load group names. Please refresh and try again.');
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

    const model: DepartmentCreate = {
      name: this.form.value.name ?? '',
      groupName: this.form.value.groupName ?? '',
    };

    this.isSaving.set(true);
    this.hrApi
      .createDepartment(model)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (department) => {
          this.notificationService.success('Department created successfully.');
          void this.router.navigate(['/hr/departments', department.id]);
        },
        error: (err: unknown) => {
          this.isSaving.set(false);
          if (err instanceof ApiValidationError) {
            const duplicateNameError = err.errors.find((ve) => ve.errorCode === 'Rule-05');
            if (duplicateNameError) {
              this.form.controls.name.setErrors({ server: duplicateNameError.errorMessage });
              this.form.controls.name.markAsTouched();
              return;
            }
          }
          this.notificationService.error('Failed to create department. Please try again.');
        },
      });
  }
}
