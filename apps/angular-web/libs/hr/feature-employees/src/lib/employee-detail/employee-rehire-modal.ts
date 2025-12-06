import { ChangeDetectionStrategy, Component, computed, effect, inject, input, model, OnInit, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmployeeStore, SHIFT_OPTIONS, PAY_FREQUENCY_OPTIONS } from '@adventureworks-web/hr/data-access';
import type { EmployeeRehire } from '@adventureworks-web/hr/data-access';
import { InputFieldComponent, ModalComponent, SelectFieldComponent, TextareaFieldComponent, ToggleFieldComponent } from '@adventureworks-web/shared/ui';
import { minDateValidator, NotificationService } from '@adventureworks-web/shared/util';
import { createDepartmentOptionsLoader } from './department-options-loader';
import { errorsToList as sharedErrorsToList, toOptionalNumber, watchLifecycleActionCompletion, WizardFormErrors } from './wizard-form-helpers';

type RehireWizardStep = 1 | 2;

const REHIRE_ELIGIBILITY_DAYS = 90;

@Component({
  selector: 'aw-employee-rehire-modal',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ModalComponent,
    InputFieldComponent,
    SelectFieldComponent,
    TextareaFieldComponent,
    ToggleFieldComponent,
  ],
  templateUrl: './employee-rehire-modal.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/** Rehire wizard modal — only rendered for a `Terminated` employee (see `EmployeeDetailComponent`). */
export class EmployeeRehireModalComponent implements OnInit {
  private readonly employeeStore = inject(EmployeeStore);
  private readonly notificationService = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly employeeId = input.required<number>();
  readonly daysSinceTermination = input.required<number | null>();
  readonly open = model(false);
  readonly rehired = output<void>();

  protected readonly shiftOptions = SHIFT_OPTIONS.map((s) => ({ value: s.value, label: s.label }));
  protected readonly payFrequencyOptions = PAY_FREQUENCY_OPTIONS.map((p) => ({ value: p.value, label: p.label }));

  private readonly departmentsLoader = createDepartmentOptionsLoader();
  protected readonly departments = this.departmentsLoader.departments;
  protected readonly departmentOptions = this.departmentsLoader.departmentOptions;

  /**
   * 90-day rehire cooling-off period (US-758 AC: terminated 30 days ago -> "60 more days" warning,
   * Confirm disabled; eligible once `daysSinceTermination >= 90`). Matches
   * `EmployeeDetailComponent.isPastRehireWaitingPeriod` exactly — that read-only badge and this
   * modal's gate must agree since this modal is the only path to actually perform a rehire.
   * `null` (never terminated) is treated as not blocked.
   */
  protected readonly daysRemaining = computed(() => {
    const days = this.daysSinceTermination();
    return days === null ? 0 : Math.max(0, REHIRE_ELIGIBILITY_DAYS - days);
  });
  protected readonly isBlocked = computed(() => this.daysRemaining() > 0);

  protected readonly currentStep = signal<RehireWizardStep>(1);
  protected readonly isSubmitting = signal(false);

  protected readonly form = this.fb.group({
    assignment: this.fb.group({
      // minDateValidator is (re-)applied with a fresh `new Date()` in resetForm() on every open,
      // not here: this component instance is never destroyed/recreated between opens, so a
      // validator built once at construction time would freeze "today" for the component's
      // entire lifetime — e.g. a tab left open across midnight would silently accept an
      // already-past rehire date.
      rehireDate: ['', Validators.required],
      departmentId: [null as number | null, Validators.required],
      shiftId: [null as number | null, Validators.required],
      managerId: [null as number | null],
    }),
    compensation: this.fb.group({
      payRate: [null as number | null, [Validators.required, Validators.min(0.01), Validators.max(500)]],
      payFrequency: [null as number | null, Validators.required],
      restoreSeniority: [false],
      notes: [null as string | null, Validators.maxLength(500)],
    }),
  });

  private readonly formErrors = new WizardFormErrors(this.form);

  protected readonly assignmentValid = computed(() => this.formErrors.groupValid(this.form.controls.assignment));

  protected readonly rehireDateErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.assignment.controls.rehireDate, {
      required: 'Rehire date is required.',
      minDate: 'Rehire date cannot be in the past.',
    }),
  );
  protected readonly departmentIdErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.assignment.controls.departmentId, { required: 'Department is required.' }),
  );
  protected readonly shiftIdErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.assignment.controls.shiftId, { required: 'Shift is required.' }),
  );
  protected readonly payRateErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.compensation.controls.payRate, {
      required: 'Pay rate is required.',
      min: 'Pay rate must be at least 0.01.',
      max: 'Pay rate cannot exceed 500.',
    }),
  );
  protected readonly payFrequencyErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.compensation.controls.payFrequency, { required: 'Pay frequency is required.' }),
  );
  protected readonly notesErrors = computed(() =>
    this.formErrors.getErrors(this.form.controls.compensation.controls.notes, { maxlength: 'Notes cannot exceed 500 characters.' }),
  );

  protected readonly errorsToList = sharedErrorsToList;

  /**
   * The rehire-date `<input>` is a raw, hand-rolled date field (unlike `aw-input-field`), so it
   * doesn't get `aria-describedby` wiring for free — build it manually so assistive tech
   * announces the error text, not just the visual `input-error` styling.
   */
  protected readonly rehireDateAriaDescribedBy = computed(() => {
    const errors = this.rehireDateErrors();
    return errors ? Object.keys(errors).map((key) => `aw-employee-rehire-date-error-${key}`).join(' ') : null;
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
      errorMessage: 'Failed to rehire employee. Please try again.',
      onSuccess: () => {
        this.open.set(false);
        this.rehired.emit();
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
      assignment: { rehireDate: '', departmentId: null, shiftId: null, managerId: null },
      compensation: { payRate: null, payFrequency: null, restoreSeniority: false, notes: null },
    });
    // Recompute "today" fresh on every open — see the comment on the `rehireDate` control above.
    this.form.controls.assignment.controls.rehireDate.setValidators([Validators.required, minDateValidator(new Date())]);
    this.form.controls.assignment.controls.rehireDate.updateValueAndValidity();
  }

  protected onNext(): void {
    const group = this.form.controls.assignment;
    group.markAllAsTouched();
    this.formErrors.bumpTouchTick();
    if (group.invalid) {
      return;
    }
    this.currentStep.set(2);
  }

  protected onBack(): void {
    this.currentStep.set(1);
  }

  protected onCancel(): void {
    this.open.set(false);
  }

  protected onConfirm(): void {
    if (this.isSubmitting() || this.isBlocked()) {
      return;
    }
    this.formErrors.submitted.set(true);
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    const assignment = this.form.controls.assignment.value;
    const compensation = this.form.controls.compensation.value;

    const model: EmployeeRehire = {
      employeeId: this.employeeId(),
      rehireDate: assignment.rehireDate ?? '',
      departmentId: Number(assignment.departmentId),
      shiftId: Number(assignment.shiftId),
      managerId: toOptionalNumber(assignment.managerId) ?? null,
      payRate: Number(compensation.payRate),
      payFrequency: Number(compensation.payFrequency),
      restoreSeniority: compensation.restoreSeniority ?? false,
      notes: compensation.notes || null,
    };

    this.isSubmitting.set(true);
    this.employeeStore.rehireEmployee({ id: this.employeeId(), model });
  }
}
