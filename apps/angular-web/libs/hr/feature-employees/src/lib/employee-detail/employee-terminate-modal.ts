import { ChangeDetectionStrategy, Component, computed, effect, inject, input, model, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmployeeStore, TERMINATION_TYPE_OPTIONS } from '@adventureworks-web/hr/data-access';
import type { EmployeeTerminate } from '@adventureworks-web/hr/data-access';
import { ModalComponent, SelectFieldComponent, TextareaFieldComponent, ToggleFieldComponent } from '@adventureworks-web/shared/ui';
import { maxFutureDateValidator, NotificationService } from '@adventureworks-web/shared/util';
import { errorsToList as sharedErrorsToList, watchLifecycleActionCompletion, WizardFormErrors } from './wizard-form-helpers';

type TerminateWizardStep = 1 | 2 | 3;

@Component({
  selector: 'aw-employee-terminate-modal',
  standalone: true,
  imports: [ReactiveFormsModule, ModalComponent, SelectFieldComponent, TextareaFieldComponent, ToggleFieldComponent],
  templateUrl: './employee-terminate-modal.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/** Terminate wizard modal — only rendered for an `Active` employee (see `EmployeeDetailComponent`). */
export class EmployeeTerminateModalComponent {
  private readonly employeeStore = inject(EmployeeStore);
  private readonly notificationService = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly employeeId = input.required<number>();
  readonly vacationHoursBalance = input.required<number>();
  readonly open = model(false);
  readonly terminated = output<void>();

  protected readonly terminationTypeOptions = TERMINATION_TYPE_OPTIONS.map((t) => ({ value: t, label: t }));
  protected readonly hasVacationBalance = computed(() => this.vacationHoursBalance() > 0);

  protected readonly currentStep = signal<TerminateWizardStep>(1);
  protected readonly isSubmitting = signal(false);

  protected readonly form = this.fb.group({
    details: this.fb.group({
      terminationDate: ['', [Validators.required, maxFutureDateValidator(90)]],
      terminationType: ['' as '' | EmployeeTerminate['terminationType'], Validators.required],
      reason: ['', [Validators.required, Validators.maxLength(500)]],
    }),
    options: this.fb.group({
      eligibleForRehire: [true],
      payoutPto: [false],
      notes: [null as string | null, Validators.maxLength(1000)],
    }),
  });

  private readonly formErrors = new WizardFormErrors(this.form);

  protected readonly detailsValid = computed(() => this.formErrors.groupValid(this.form.controls.details));
  protected readonly optionsValid = computed(() => this.formErrors.groupValid(this.form.controls.options));

  protected readonly terminationDateErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.details.controls.terminationDate, {
      required: 'Termination date is required.',
      maxFutureDate: 'Termination date cannot be more than 90 days in the future.',
    }),
  );
  protected readonly terminationTypeErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.details.controls.terminationType, { required: 'Termination type is required.' }),
  );
  protected readonly reasonErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.details.controls.reason, {
      required: 'Reason is required.',
      maxlength: 'Reason cannot exceed 500 characters.',
    }),
  );
  protected readonly notesErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.options.controls.notes, { maxlength: 'Notes cannot exceed 1000 characters.' }),
  );

  protected readonly errorsToList = sharedErrorsToList;

  /**
   * The termination-date `<input>` is a raw, hand-rolled date field (unlike `aw-input-field`), so
   * it doesn't get `aria-describedby` wiring for free — build it manually so assistive tech
   * announces the error text, not just the visual `input-error` styling.
   */
  protected readonly terminationDateAriaDescribedBy = computed(() => {
    const errors = this.terminationDateErrors();
    return errors ? Object.keys(errors).map((key) => `aw-employee-terminate-date-error-${key}`).join(' ') : null;
  });

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
      errorMessage: 'Failed to terminate employee. Please try again.',
      onSuccess: () => {
        this.open.set(false);
        this.terminated.emit();
      },
    });
  }

  private resetForm(): void {
    this.currentStep.set(1);
    this.formErrors.submitted.set(false);
    this.form.reset({
      details: { terminationDate: '', terminationType: '', reason: '' },
      options: { eligibleForRehire: true, payoutPto: false, notes: null },
    });
  }

  protected onNext(): void {
    const group = this.currentStep() === 1 ? this.form.controls.details : this.form.controls.options;
    group.markAllAsTouched();
    this.formErrors.bumpTouchTick();
    if (group.invalid) {
      return;
    }
    this.currentStep.update((step) => (step < 3 ? ((step + 1) as TerminateWizardStep) : step));
  }

  protected onBack(): void {
    this.currentStep.update((step) => (step > 1 ? ((step - 1) as TerminateWizardStep) : step));
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

    const details = this.form.controls.details.value;
    const options = this.form.controls.options.value;

    const model: EmployeeTerminate = {
      employeeId: this.employeeId(),
      terminationDate: details.terminationDate ?? '',
      terminationType: details.terminationType as EmployeeTerminate['terminationType'],
      reason: details.reason ?? '',
      eligibleForRehire: options.eligibleForRehire ?? true,
      payoutPto: this.hasVacationBalance() ? (options.payoutPto ?? false) : false,
      notes: options.notes || null,
    };

    this.isSubmitting.set(true);
    this.employeeStore.terminateEmployee({ id: this.employeeId(), model });
  }
}
