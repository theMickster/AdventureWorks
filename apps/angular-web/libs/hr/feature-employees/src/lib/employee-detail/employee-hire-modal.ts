import { ChangeDetectionStrategy, Component, computed, effect, inject, input, model, OnInit, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmployeeStore, SHIFT_OPTIONS, PAY_FREQUENCY_OPTIONS } from '@adventureworks-web/hr/data-access';
import type { EmployeeHire } from '@adventureworks-web/hr/data-access';
import { InputFieldComponent, ModalComponent, SelectFieldComponent, TextareaFieldComponent } from '@adventureworks-web/shared/ui';
import { maxFutureDateValidator, NotificationService } from '@adventureworks-web/shared/util';
import { createDepartmentOptionsLoader } from './department-options-loader';
import { errorsToList as sharedErrorsToList, toOptionalNumber, watchLifecycleActionCompletion, WizardFormErrors } from './wizard-form-helpers';

type HireWizardStep = 1 | 2 | 3;

@Component({
  selector: 'aw-employee-hire-modal',
  standalone: true,
  imports: [ReactiveFormsModule, ModalComponent, InputFieldComponent, SelectFieldComponent, TextareaFieldComponent],
  templateUrl: './employee-hire-modal.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Hire wizard modal — only rendered for an employee whose lifecycle status is `OnLeave`
 * (see `EmployeeDetailComponent`). Calls `EmployeeStore.hireEmployee()`, never `HrApiService`
 * directly, so the resulting entity update flows through the store's cache.
 */
export class EmployeeHireModalComponent implements OnInit {
  private readonly employeeStore = inject(EmployeeStore);
  private readonly notificationService = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly employeeId = input.required<number>();
  readonly open = model(false);
  readonly hired = output<void>();

  protected readonly shiftOptions = SHIFT_OPTIONS.map((s) => ({ value: s.value, label: s.label }));
  protected readonly payFrequencyOptions = PAY_FREQUENCY_OPTIONS.map((p) => ({ value: p.value, label: p.label }));

  private readonly departmentsLoader = createDepartmentOptionsLoader();
  protected readonly departments = this.departmentsLoader.departments;
  protected readonly departmentOptions = this.departmentsLoader.departmentOptions;

  protected readonly currentStep = signal<HireWizardStep>(1);
  protected readonly isSubmitting = signal(false);

  protected readonly form = this.fb.group({
    assignment: this.fb.group({
      departmentId: [null as number | null, Validators.required],
      shiftId: [null as number | null, Validators.required],
      hireDate: ['', [Validators.required, maxFutureDateValidator(30)]],
      managerId: [null as number | null],
    }),
    compensation: this.fb.group({
      initialPayRate: [null as number | null, [Validators.required, Validators.min(0.01), Validators.max(500)]],
      payFrequency: [null as number | null, Validators.required],
      initialVacationHours: [40 as number | null, [Validators.min(0), Validators.max(240)]],
      initialSickLeaveHours: [24 as number | null, [Validators.min(0), Validators.max(480)]],
      notes: [null as string | null, Validators.maxLength(500)],
    }),
  });

  private readonly formErrors = new WizardFormErrors(this.form);

  protected readonly assignmentValid = computed(() => this.formErrors.groupValid(this.form.controls.assignment));
  protected readonly compensationValid = computed(() => this.formErrors.groupValid(this.form.controls.compensation));

  protected readonly departmentIdErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.assignment.controls.departmentId, { required: 'Department is required.' }),
  );
  protected readonly shiftIdErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.assignment.controls.shiftId, { required: 'Shift is required.' }),
  );
  protected readonly hireDateErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.assignment.controls.hireDate, {
      required: 'Hire date is required.',
      maxFutureDate: 'Hire date cannot be more than 30 days in the future.',
    }),
  );

  protected readonly initialPayRateErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.compensation.controls.initialPayRate, {
      required: 'Pay rate is required.',
      min: 'Pay rate must be at least 0.01.',
      max: 'Pay rate cannot exceed 500.',
    }),
  );
  protected readonly payFrequencyErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.compensation.controls.payFrequency, { required: 'Pay frequency is required.' }),
  );
  protected readonly initialVacationHoursErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.compensation.controls.initialVacationHours, {
      min: 'Vacation hours cannot be negative.',
      max: 'Vacation hours cannot exceed 240.',
    }),
  );
  protected readonly initialSickLeaveHoursErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.compensation.controls.initialSickLeaveHours, {
      min: 'Sick leave hours cannot be negative.',
      max: 'Sick leave hours cannot exceed 480.',
    }),
  );
  protected readonly notesErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.compensation.controls.notes, { maxlength: 'Notes cannot exceed 500 characters.' }),
  );

  protected readonly errorsToList = sharedErrorsToList;

  /**
   * The hire-date `<input>` is a raw, hand-rolled date field (unlike `aw-input-field`), so it
   * doesn't get `aria-describedby` wiring for free — build it manually so assistive tech
   * announces the error text, not just the visual `input-error` styling.
   */
  protected readonly hireDateAriaDescribedBy = computed(() => {
    const errors = this.hireDateErrors();
    return errors ? Object.keys(errors).map((key) => `aw-employee-hire-date-error-${key}`).join(' ') : null;
  });

  /** Resolves a select option's label for the read-only Confirm step (Angular templates can't inline arrow functions). */
  protected findLabel(options: { value: number; label: string }[], value: number | null | undefined): string {
    return options.find((o) => o.value === value)?.label ?? '—';
  }

  constructor() {
    effect(() => {
      if (this.open()) {
        this.resetForm();
      }
    });

    watchLifecycleActionCompletion({
      isSubmitting: this.isSubmitting,
      isLoading: this.employeeStore.isLoading,
      hasError: this.employeeStore.hasError,
      notificationService: this.notificationService,
      errorMessage: 'Failed to hire employee. Please try again.',
      onSuccess: () => {
        this.open.set(false);
        this.hired.emit();
      },
    });
  }

  ngOnInit(): void {
    this.departmentsLoader.load();
  }

  private resetForm(): void {
    this.currentStep.set(1);
    this.formErrors.submitted.set(false);
    this.form.reset({
      assignment: { departmentId: null, shiftId: null, hireDate: '', managerId: null },
      compensation: { initialPayRate: null, payFrequency: null, initialVacationHours: 40, initialSickLeaveHours: 24, notes: null },
    });
  }

  protected onNext(): void {
    const group = this.currentStep() === 1 ? this.form.controls.assignment : this.form.controls.compensation;
    group.markAllAsTouched();
    this.formErrors.bumpTouchTick();
    if (group.invalid) {
      return;
    }
    this.currentStep.update((step) => (step < 3 ? ((step + 1) as HireWizardStep) : step));
  }

  protected onBack(): void {
    this.currentStep.update((step) => (step > 1 ? ((step - 1) as HireWizardStep) : step));
  }

  protected onCancel(): void {
    this.open.set(false);
  }

  protected onConfirm(): void {
    if (this.isSubmitting()) {
      return;
    }
    this.formErrors.submitted.set(true);
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    const assignment = this.form.controls.assignment.value;
    const compensation = this.form.controls.compensation.value;

    const model: EmployeeHire = {
      employeeId: this.employeeId(),
      hireDate: assignment.hireDate ?? '',
      departmentId: Number(assignment.departmentId),
      shiftId: Number(assignment.shiftId),
      managerId: toOptionalNumber(assignment.managerId) ?? null,
      initialPayRate: Number(compensation.initialPayRate),
      payFrequency: Number(compensation.payFrequency),
      initialVacationHours: toOptionalNumber(compensation.initialVacationHours),
      initialSickLeaveHours: toOptionalNumber(compensation.initialSickLeaveHours),
      notes: compensation.notes || null,
    };

    this.isSubmitting.set(true);
    this.employeeStore.hireEmployee({ id: this.employeeId(), model });
  }
}
