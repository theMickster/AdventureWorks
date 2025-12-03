import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { EmployeeCreate } from '@adventureworks-web/hr/data-access';
import { LookupApiService } from '@adventureworks-web/shared/data-access';
import type { AddressType, PhoneNumberType, StateProvince } from '@adventureworks-web/shared/data-access';
import { InputFieldComponent, SelectFieldComponent, SkeletonComponent } from '@adventureworks-web/shared/ui';
import { ApiValidationError, ConflictError, minAgeValidator, NotificationService } from '@adventureworks-web/shared/util';

/** Sequential wizard step. 4 is the read-only review/submit step — it owns no `FormGroup` of its own. */
type WizardStep = 1 | 2 | 3 | 4;

// Maps API validation error `propertyName`s to their nested form control path and the wizard
// step that owns that control, so a server-side rejection can both set the inline error AND
// jump the user back to the step where the offending field lives (wizard steps aren't all
// visible at once, unlike the freely-clickable tabs in `sales-person-create.ts`).
const API_PROPERTY_TO_FORM_PATH: Record<string, { path: string[]; step: WizardStep }> = {
  FirstName:                 { path: ['personalInfo', 'firstName'], step: 1 },
  LastName:                  { path: ['personalInfo', 'lastName'], step: 1 },
  MiddleName:                { path: ['personalInfo', 'middleName'], step: 1 },
  Title:                     { path: ['personalInfo', 'title'], step: 1 },
  Suffix:                    { path: ['personalInfo', 'suffix'], step: 1 },
  JobTitle:                  { path: ['personalInfo', 'jobTitle'], step: 1 },
  NationalIdNumber:          { path: ['personalInfo', 'nationalIdNumber'], step: 1 },
  LoginId:                   { path: ['personalInfo', 'loginId'], step: 1 },
  BirthDate:                 { path: ['personalInfo', 'birthDate'], step: 1 },
  MaritalStatus:             { path: ['personalInfo', 'maritalStatus'], step: 1 },
  Gender:                    { path: ['personalInfo', 'gender'], step: 1 },
  EmailAddress:              { path: ['contact', 'emailAddress'], step: 2 },
  'Phone.PhoneNumber':       { path: ['contact', 'phone', 'phoneNumber'], step: 2 },
  'Phone.PhoneNumberTypeId': { path: ['contact', 'phone', 'phoneNumberTypeId'], step: 2 },
  AddressTypeId:             { path: ['address', 'addressTypeId'], step: 3 },
  'Address.AddressLine1':    { path: ['address', 'addressLine1'], step: 3 },
  'Address.AddressLine2':    { path: ['address', 'addressLine2'], step: 3 },
  'Address.City':            { path: ['address', 'city'], step: 3 },
  'Address.PostalCode':      { path: ['address', 'postalCode'], step: 3 },
  'Address.StateProvince':   { path: ['address', 'stateProvinceId'], step: 3 },
};

@Component({
  selector: 'aw-employee-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputFieldComponent, SelectFieldComponent, SkeletonComponent],
  templateUrl: './employee-create.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Guided, sequential 3-step wizard (Personal Info -> Contact -> Address) plus a review step for
 * creating a new employee at `hr/employees/new`.
 *
 * Unlike `SalesPersonCreateComponent`'s freely-clickable tabs, `currentStep` only advances via
 * `onNext()`, which marks the current step's group as touched and blocks advancing while it is
 * invalid (AC: "remain on current step"). `onBack()` and the review step's "Edit" links are not
 * gated — the form is a single `FormGroup` tree, so navigating between steps never loses data.
 *
 * Calls `HrApiService.createEmployee()` directly, no NgRx store, consistent with
 * `department-create.ts` and `sales-person-create.ts`.
 *
 * There is deliberately no `hireDate` field: `EmployeeCreateModel` has no such property — the
 * server hardcodes it on creation. `salariedFlag` is hardcoded to `false` and `organizationLevel`
 * to `null`; neither is part of this story's acceptance criteria.
 */
export class EmployeeCreateComponent implements OnInit {
  private readonly hrApi = inject(HrApiService);
  private readonly lookupApi = inject(LookupApiService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly submitted = signal(false);

  protected readonly addressTypes = signal<AddressType[]>([]);
  protected readonly stateProvinces = signal<StateProvince[]>([]);
  protected readonly phoneNumberTypes = signal<PhoneNumberType[]>([]);

  /** Current wizard step. 1 = Personal Info, 2 = Contact, 3 = Address, 4 = Review. */
  protected readonly currentStep = signal<WizardStep>(1);

  protected readonly maxBirthDate: string = (() => {
    const d = new Date();
    d.setFullYear(d.getFullYear() - 18);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  })();

  protected readonly form = this.fb.group({
    personalInfo: this.fb.group({
      firstName:        ['', [Validators.required, Validators.maxLength(50)]],
      lastName:         ['', [Validators.required, Validators.maxLength(50)]],
      middleName:       [null as string | null, Validators.maxLength(50)],
      title:            [null as string | null, Validators.maxLength(8)],
      suffix:           [null as string | null, Validators.maxLength(10)],
      jobTitle:         ['', [Validators.required, Validators.maxLength(50)]],
      nationalIdNumber: ['', [Validators.required, Validators.maxLength(15)]],
      loginId:          ['', [Validators.required, Validators.maxLength(256)]],
      birthDate:        ['', [Validators.required, minAgeValidator(18)]],
      maritalStatus:    ['' as 'M' | 'S' | '', Validators.required],
      gender:           ['' as 'M' | 'F' | '', Validators.required],
    }),
    contact: this.fb.group({
      emailAddress: ['', [Validators.required, Validators.email, Validators.maxLength(50)]],
      phone: this.fb.group({
        phoneNumber:       ['', [Validators.required, Validators.maxLength(25)]],
        phoneNumberTypeId: [null as number | null, Validators.required],
      }),
    }),
    address: this.fb.group({
      addressLine1:    ['', [Validators.required, Validators.maxLength(60)]],
      addressLine2:    [null as string | null, Validators.maxLength(60)],
      city:            ['', [Validators.required, Validators.maxLength(30)]],
      stateProvinceId: [null as number | null, Validators.required],
      postalCode:      ['', [Validators.required, Validators.maxLength(15)]],
      addressTypeId:   [null as number | null, Validators.required],
    }),
  });

  // Bridge form value changes into the signal graph so computed() re-evaluates whenever any field changes
  private readonly _formStatus = toSignal(this.form.valueChanges, { initialValue: this.form.value });

  // `markAllAsTouched()`/`markAsTouched()` mutate `AbstractControl.touched` directly without emitting
  // through `valueChanges` — a bare property read doesn't register as a computed() dependency, so
  // `getErrors()` would otherwise keep returning its stale (pre-touch) cached result. Bumping this
  // signal from every touch-marking call site forces the error computed()s to re-evaluate.
  private readonly _touchTick = signal(0);

  protected readonly addressTypeOptions = computed(() =>
    this.addressTypes().map((a) => ({ value: a.id, label: a.name })),
  );
  protected readonly stateProvinceOptions = computed(() =>
    this.stateProvinces().map((s) => ({ value: s.id, label: `${s.name} (${s.code})` })),
  );
  protected readonly phoneNumberTypeOptions = computed(() =>
    this.phoneNumberTypes().map((p) => ({ value: p.id, label: p.name })),
  );

  /** True once the Personal Info step's group is valid — gates the step-1 "Next" button. */
  protected readonly personalInfoValid = computed(() => {
    this._formStatus();
    return this.form.controls.personalInfo.valid;
  });
  /** True once the Contact step's group is valid — gates the step-2 "Next" button. */
  protected readonly contactValid = computed(() => {
    this._formStatus();
    return this.form.controls.contact.valid;
  });
  /** True once the Address step's group is valid — gates the step-3 "Next" button. */
  protected readonly addressValid = computed(() => {
    this._formStatus();
    return this.form.controls.address.valid;
  });

  // ── Personal Info field errors ─────────────────────────────────────────

  protected readonly firstNameErrors = computed(() =>
    this.getErrors(this.form.controls.personalInfo.controls.firstName, {
      required: 'First name is required.',
      maxlength: 'Cannot exceed 50 characters.',
    }),
  );
  protected readonly lastNameErrors = computed(() =>
    this.getErrors(this.form.controls.personalInfo.controls.lastName, {
      required: 'Last name is required.',
      maxlength: 'Cannot exceed 50 characters.',
    }),
  );
  protected readonly middleNameErrors = computed(() =>
    this.getErrors(this.form.controls.personalInfo.controls.middleName, {
      maxlength: 'Cannot exceed 50 characters.',
    }),
  );
  protected readonly titleErrors = computed(() =>
    this.getErrors(this.form.controls.personalInfo.controls.title, {
      maxlength: 'Cannot exceed 8 characters.',
    }),
  );
  protected readonly suffixErrors = computed(() =>
    this.getErrors(this.form.controls.personalInfo.controls.suffix, {
      maxlength: 'Cannot exceed 10 characters.',
    }),
  );
  protected readonly jobTitleErrors = computed(() =>
    this.getErrors(this.form.controls.personalInfo.controls.jobTitle, {
      required: 'Job title is required.',
      maxlength: 'Cannot exceed 50 characters.',
    }),
  );
  protected readonly nationalIdNumberErrors = computed(() =>
    this.getErrors(this.form.controls.personalInfo.controls.nationalIdNumber, {
      required: 'National ID number is required.',
      maxlength: 'Cannot exceed 15 characters.',
    }),
  );
  protected readonly loginIdErrors = computed(() =>
    this.getErrors(this.form.controls.personalInfo.controls.loginId, {
      required: 'Login ID is required.',
      maxlength: 'Cannot exceed 256 characters.',
    }),
  );
  protected readonly birthDateErrors = computed(() =>
    this.getErrors(this.form.controls.personalInfo.controls.birthDate, {
      required: 'Birth date is required.',
      minAge:   'Must be at least 18',
    }),
  );
  protected readonly maritalStatusErrors = computed(() =>
    this.getErrors(this.form.controls.personalInfo.controls.maritalStatus, {
      required: 'Marital status is required.',
    }),
  );
  protected readonly genderErrors = computed(() =>
    this.getErrors(this.form.controls.personalInfo.controls.gender, {
      required: 'Gender is required.',
    }),
  );

  // ── Contact field errors ───────────────────────────────────────────────

  protected readonly emailAddressErrors = computed(() =>
    this.getErrors(this.form.controls.contact.controls.emailAddress, {
      required:  'Email address is required.',
      email:     'Enter a valid email address.',
      maxlength: 'Cannot exceed 50 characters.',
    }),
  );
  protected readonly phoneNumberErrors = computed(() =>
    this.getErrors(this.form.controls.contact.controls.phone.controls.phoneNumber, {
      required:  'Phone number is required.',
      maxlength: 'Cannot exceed 25 characters.',
    }),
  );
  protected readonly phoneNumberTypeErrors = computed(() =>
    this.getErrors(this.form.controls.contact.controls.phone.controls.phoneNumberTypeId, {
      required: 'Phone type is required.',
    }),
  );

  // ── Address field errors ───────────────────────────────────────────────

  protected readonly addressLine1Errors = computed(() =>
    this.getErrors(this.form.controls.address.controls.addressLine1, {
      required:  'Address line 1 is required.',
      maxlength: 'Cannot exceed 60 characters.',
    }),
  );
  protected readonly addressLine2Errors = computed(() =>
    this.getErrors(this.form.controls.address.controls.addressLine2, {
      maxlength: 'Cannot exceed 60 characters.',
    }),
  );
  protected readonly cityErrors = computed(() =>
    this.getErrors(this.form.controls.address.controls.city, {
      required:  'City is required.',
      maxlength: 'Cannot exceed 30 characters.',
    }),
  );
  protected readonly stateProvinceErrors = computed(() =>
    this.getErrors(this.form.controls.address.controls.stateProvinceId, {
      required: 'State / province is required.',
    }),
  );
  protected readonly postalCodeErrors = computed(() =>
    this.getErrors(this.form.controls.address.controls.postalCode, {
      required:  'Postal code is required.',
      maxlength: 'Cannot exceed 15 characters.',
    }),
  );
  protected readonly addressTypeErrors = computed(() =>
    this.getErrors(this.form.controls.address.controls.addressTypeId, {
      required: 'Address type is required.',
    }),
  );

  /** Converts an errors map to an iterable list for template @for loops. */
  protected errorsToList(errors: Record<string, string> | null): Array<[string, string]> {
    return errors ? (Object.entries(errors) as Array<[string, string]>) : [];
  }

  /**
   * Returns validation error messages for a control, shown after submit or touch.
   * Always appends a `server` error when the API has set one on the control.
   */
  private getErrors(ctrl: AbstractControl, msgs: Partial<Record<string, string>>): Record<string, string> | null {
    this._formStatus();
    this._touchTick();
    if (!ctrl.errors || (!this.submitted() && !ctrl.touched)) {
      return null;
    }
    const result: Record<string, string> = {};
    for (const [key, msg] of Object.entries(msgs)) {
      if (ctrl.errors[key]) {
        result[key] = msg ?? key;
      }
    }
    if (ctrl.errors['server']) {
      result['server'] = ctrl.errors['server'] as string;
    }
    return Object.keys(result).length ? result : null;
  }

  ngOnInit(): void {
    forkJoin({
      addressTypes:     this.lookupApi.getAddressTypes(),
      stateProvinces:   this.lookupApi.getStateProvinces(),
      phoneNumberTypes: this.lookupApi.getPhoneNumberTypes(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.addressTypes.set(result.addressTypes);
          this.stateProvinces.set(result.stateProvinces);
          this.phoneNumberTypes.set(result.phoneNumberTypes);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.notificationService.error('Failed to load reference data. Please refresh and try again.');
        },
      });
  }

  /** Marks the current step's group as touched and, only if it is valid, advances to the next step. */
  protected onNext(): void {
    const group = this.currentStepGroup();
    group?.markAllAsTouched();
    this._touchTick.update((tick) => tick + 1);
    if (group?.invalid) {
      return;
    }
    this.currentStep.update((step) => (step < 4 ? ((step + 1) as WizardStep) : step));
  }

  /** Moves back one step. Not gated — the form is a single tree, so no data is lost. */
  protected onBack(): void {
    this.currentStep.update((step) => (step > 1 ? ((step - 1) as WizardStep) : step));
  }

  /** Jumps directly to a step from the review screen's "Edit" links. */
  protected onEditStep(step: WizardStep): void {
    this.currentStep.set(step);
  }

  private currentStepGroup(): AbstractControl | null {
    switch (this.currentStep()) {
      case 1:
        return this.form.controls.personalInfo;
      case 2:
        return this.form.controls.contact;
      case 3:
        return this.form.controls.address;
      default:
        return null;
    }
  }

  /**
   * Validates the whole form tree as a safety net and submits a create request from the review step.
   *
   * `phoneNumberTypeId` and `addressTypeId` are non-nullable `number` fields on `EmployeeCreate`
   * (unlike the equivalent `SalesPersonCreate` fields, which allow `null`) — each selected id is
   * resolved against its loaded lookup signal before building the payload, and `stateProvinceId`
   * is resolved to the full `StateProvince` object the API expects. Submission is blocked with a
   * distinct error toast per field if any of the three selections don't match a loaded value,
   * guarding against stale or tampered select state.
   */
  protected onSubmit(): void {
    if (this.isSaving()) {
      return;
    }
    this.submitted.set(true);
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    const personalInfo = this.form.controls.personalInfo.value;
    const contact = this.form.controls.contact.value;
    const address = this.form.controls.address.value;

    const phoneNumberTypeIdRaw = contact.phone?.phoneNumberTypeId;
    const phoneNumberTypeId = phoneNumberTypeIdRaw ? Number(phoneNumberTypeIdRaw) : null;
    const selectedPhoneType = this.phoneNumberTypes().find((p) => p.id === phoneNumberTypeId);
    if (!selectedPhoneType) {
      this.notificationService.error('Selected phone type is invalid. Please select a valid phone type and try again.');
      return;
    }

    const addressTypeIdRaw = address.addressTypeId;
    const addressTypeId = addressTypeIdRaw ? Number(addressTypeIdRaw) : null;
    const selectedAddressType = this.addressTypes().find((a) => a.id === addressTypeId);
    if (!selectedAddressType) {
      this.notificationService.error('Selected address type is invalid. Please select a valid address type and try again.');
      return;
    }

    const stateProvinceIdRaw = address.stateProvinceId;
    const stateProvinceId = stateProvinceIdRaw ? Number(stateProvinceIdRaw) : null;
    const selectedState = this.stateProvinces().find((s) => s.id === stateProvinceId);
    if (!selectedState) {
      this.notificationService.error('Selected state is invalid. Please select a valid state and try again.');
      return;
    }

    const model: EmployeeCreate = {
      firstName:         personalInfo.firstName ?? '',
      lastName:          personalInfo.lastName ?? '',
      middleName:        personalInfo.middleName ?? null,
      title:             personalInfo.title ?? null,
      suffix:            personalInfo.suffix ?? null,
      jobTitle:          personalInfo.jobTitle ?? '',
      maritalStatus:     personalInfo.maritalStatus as 'M' | 'S',
      gender:            personalInfo.gender as 'M' | 'F',
      salariedFlag:      false,
      organizationLevel: null,
      nationalIdNumber:  personalInfo.nationalIdNumber ?? '',
      loginId:           personalInfo.loginId ?? '',
      birthDate:         personalInfo.birthDate ?? '',
      phone: {
        phoneNumber:       contact.phone?.phoneNumber ?? '',
        phoneNumberTypeId: selectedPhoneType.id,
      },
      emailAddress: contact.emailAddress ?? '',
      address: {
        addressLine1: address.addressLine1 ?? '',
        addressLine2: address.addressLine2 ?? null,
        city:         address.city ?? '',
        stateProvince: {
          id:   selectedState.id,
          name: selectedState.name,
          code: selectedState.code,
        },
        postalCode: address.postalCode ?? '',
      },
      addressTypeId: selectedAddressType.id,
    };

    this.isSaving.set(true);
    this.hrApi
      .createEmployee(model)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (employee) => {
          this.notificationService.success('Employee created successfully.');
          this.isSaving.set(false);
          void this.router.navigate(['/hr/employees', employee.id]);
        },
        error: (err: unknown) => {
          this.isSaving.set(false);
          if (err instanceof ApiValidationError) {
            let anyMapped = false;
            let jumpStep: WizardStep | null = null;
            for (const ve of err.errors) {
              const mapping = API_PROPERTY_TO_FORM_PATH[ve.propertyName];
              if (mapping) {
                const ctrl = this.form.get(mapping.path);
                ctrl?.setErrors({ server: ve.errorMessage });
                ctrl?.markAsTouched();
                anyMapped = true;
                jumpStep ??= mapping.step;
              }
            }
            this._touchTick.update((tick) => tick + 1);
            if (jumpStep) {
              this.currentStep.set(jumpStep);
            }
            if (!anyMapped) {
              this.notificationService.error('Failed to create employee. Please try again.');
            }
          } else if (err instanceof ConflictError) {
            // A 409 carries no per-field signal (unlike the 400 path above), so this can only jump to
            // step 1 generically — both National ID Number and Login ID live there. Never forward
            // err.message to the UI; the toast text is hardcoded per this workspace's security rule.
            this.notificationService.error(
              'An employee with this National ID Number or Login ID already exists. Please check your entries and try again.',
            );
            this.currentStep.set(1);
          } else {
            this.notificationService.error('Failed to create employee. Please try again.');
          }
        },
      });
  }
}
