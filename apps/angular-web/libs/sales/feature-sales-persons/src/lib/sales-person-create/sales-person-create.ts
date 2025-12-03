import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidatorFn,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { SalesPersonCreate } from '@adventureworks-web/sales/data-access';
import { LookupApiService } from '@adventureworks-web/shared/data-access';
import type { AddressType, PhoneNumberType, SalesTerritory, StateProvince } from '@adventureworks-web/shared/data-access';
import { InputFieldComponent, SelectFieldComponent, SkeletonComponent, ToggleFieldComponent } from '@adventureworks-web/shared/ui';
import { ApiValidationError, minAgeValidator, NotificationService } from '@adventureworks-web/shared/util';

// ── Module-level validators ────────────────────────────────────────────────

function notFutureDateValidator(): ValidatorFn {
  return (ctrl) => {
    if (!ctrl.value) return null;
    const max = new Date();
    max.setDate(max.getDate() + 10);
    return new Date(ctrl.value as string) <= max ? null : { notFutureDate: true };
  };
}

function hireDateAfterBirthDateValidator(group: AbstractControl): ValidationErrors | null {
  const birth = (group as FormGroup).get('birthDate')?.value as string;
  const hire = (group as FormGroup).get('hireDate')?.value as string;
  if (!birth || !hire) return null;
  return new Date(hire) > new Date(birth) ? null : { hireAfterBirth: true };
}

// Maps API property names to nested form control paths.
// Top-level fields: bare name (e.g. "BirthDate").
// Nested validators via SetValidator get a parent prefix (e.g. "Phone.PhoneNumber",
// "Address.AddressLine1") — confirmed from SalesPersonPhoneValidator and AddressBaseModelValidator.
const API_PROPERTY_TO_FORM_PATH: Record<string, string[]> = {
  FirstName:                ['personalInfo', 'firstName'],
  LastName:                 ['personalInfo', 'lastName'],
  MiddleName:               ['personalInfo', 'middleName'],
  Title:                    ['personalInfo', 'title'],
  Suffix:                   ['personalInfo', 'suffix'],
  NationalIdNumber:         ['employment', 'nationalIdNumber'],
  LoginId:                  ['employment', 'loginId'],
  JobTitle:                 ['employment', 'jobTitle'],
  BirthDate:                ['employment', 'birthDate'],
  HireDate:                 ['employment', 'hireDate'],
  MaritalStatus:            ['employment', 'maritalStatus'],
  Gender:                   ['employment', 'gender'],
  OrganizationLevel:        ['employment', 'organizationLevel'],
  EmailAddress:             ['contact', 'emailAddress'],
  'Phone.PhoneNumber':      ['contact', 'phone', 'phoneNumber'],
  'Phone.PhoneNumberTypeId':['contact', 'phone', 'phoneNumberTypeId'],
  AddressTypeId:            ['contact', 'addressTypeId'],
  'Address.AddressLine1':   ['contact', 'address', 'addressLine1'],
  'Address.AddressLine2':   ['contact', 'address', 'addressLine2'],
  'Address.City':           ['contact', 'address', 'city'],
  'Address.PostalCode':     ['contact', 'address', 'postalCode'],
  'Address.StateProvince':  ['contact', 'address', 'stateProvinceId'],
  TerritoryId:              ['salesConfig', 'territoryId'],
  SalesQuota:               ['salesConfig', 'salesQuota'],
  Bonus:                    ['salesConfig', 'bonus'],
  CommissionPct:            ['salesConfig', 'commissionPct'],
};

// ── Component ──────────────────────────────────────────────────────────────

type ActiveTab = 'personalInfo' | 'employment' | 'contact' | 'salesConfig';

@Component({
  selector: 'aw-sales-person-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputFieldComponent, SelectFieldComponent, SkeletonComponent, ToggleFieldComponent],
  templateUrl: './sales-person-create.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Create form for a new sales person at `sales/persons/new`.
 *
 * Four reference-data lists (territories, address types, state/provinces, phone number types)
 * are loaded in parallel via `forkJoin` in `ngOnInit`. The form is split across four tabs;
 * each tab shows an error badge driven by a `sectionHasErrors` computed signal once the user
 * has attempted a first submit. Calls `SalesApiService.createSalesPerson()` directly — no NgRx store.
 *
 * Route ordering: this route (`persons/new`) must appear before `persons/:id` in `sales.routes.ts`
 * so the literal string "new" is not captured as the `:id` param.
 */
export class SalesPersonCreateComponent implements OnInit {
  private readonly salesApi = inject(SalesApiService);
  private readonly lookupApi = inject(LookupApiService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly submitted = signal(false);

  protected readonly territories = signal<SalesTerritory[]>([]);
  protected readonly addressTypes = signal<AddressType[]>([]);
  protected readonly stateProvinces = signal<StateProvince[]>([]);
  protected readonly phoneNumberTypes = signal<PhoneNumberType[]>([]);

  /** Controls which form tab is visible. Defaults to `'personalInfo'` on load. */
  protected readonly activeTab = signal<ActiveTab>('personalInfo');

  // Max selectable dates for the native date pickers — computed once at construction
  protected readonly maxBirthDate: string = (() => {
    const d = new Date();
    d.setFullYear(d.getFullYear() - 18);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  })();
  protected readonly maxHireDate: string = (() => {
    const d = new Date();
    d.setDate(d.getDate() + 10);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  })();

  protected readonly form = this.fb.group({
    personalInfo: this.fb.group({
      firstName:  ['', [Validators.required, Validators.maxLength(50)]],
      lastName:   ['', [Validators.required, Validators.maxLength(50)]],
      middleName: [null as string | null],
      title:      [null as string | null, Validators.maxLength(8)],
      suffix:     [null as string | null, Validators.maxLength(10)],
    }),
    employment: this.fb.group(
      {
        nationalIdNumber: ['', [Validators.required, Validators.maxLength(15)]],
        loginId:          ['', [Validators.required, Validators.maxLength(256)]],
        jobTitle:         ['', [Validators.required, Validators.maxLength(50)]],
        birthDate:        ['', [Validators.required, minAgeValidator(18)]],
        hireDate:         ['', [Validators.required, notFutureDateValidator()]],
        maritalStatus:    ['' as 'M' | 'S' | '', Validators.required],
        gender:           ['' as 'M' | 'F' | '', Validators.required],
        salariedFlag:     [false],
        organizationLevel:[null as number | null],
      },
      { validators: hireDateAfterBirthDateValidator },
    ),
    contact: this.fb.group({
      emailAddress: ['', [Validators.required, Validators.email, Validators.maxLength(50)]],
      phone: this.fb.group({
        phoneNumber:      ['', [Validators.required, Validators.maxLength(25)]],
        phoneNumberTypeId:[null as number | null, Validators.required],
      }),
      address: this.fb.group({
        addressLine1:    ['', [Validators.required, Validators.maxLength(60)]],
        addressLine2:    [null as string | null],
        city:            ['', [Validators.required, Validators.maxLength(30)]],
        stateProvinceId: [null as number | null, Validators.required],
        postalCode:      ['', [Validators.required, Validators.maxLength(15)]],
      }),
      addressTypeId: [null as number | null, Validators.required],
    }),
    salesConfig: this.fb.group({
      territoryId:   [null as number | null],
      salesQuota:    [null as number | null],
      bonus:         [0, Validators.required],
      commissionPct: [0, Validators.required],
    }),
  });

  // Bridge form value changes into the signal graph so computed() re-evaluates whenever any field changes
  private readonly _formStatus = toSignal(this.form.valueChanges, { initialValue: this.form.value });

  protected readonly territoryOptions = computed(() =>
    this.territories().map((t) => ({ value: t.id, label: t.name })),
  );
  protected readonly addressTypeOptions = computed(() =>
    this.addressTypes().map((a) => ({ value: a.id, label: a.name })),
  );
  protected readonly stateProvinceOptions = computed(() =>
    this.stateProvinces().map((s) => ({ value: s.id, label: `${s.name} (${s.code})` })),
  );
  protected readonly phoneNumberTypeOptions = computed(() =>
    this.phoneNumberTypes().map((p) => ({ value: p.id, label: p.name })),
  );

  /** True after first submit attempt when the Personal Info tab group contains invalid fields. */
  protected readonly personalInfoHasErrors = computed(() => {
    this._formStatus();
    return this.submitted() && this.form.controls.personalInfo.invalid;
  });
  /** True after first submit attempt when the Employment tab group contains invalid fields. */
  protected readonly employmentHasErrors = computed(() => {
    this._formStatus();
    return this.submitted() && this.form.controls.employment.invalid;
  });
  /** True after first submit attempt when the Contact tab group contains invalid fields. */
  protected readonly contactHasErrors = computed(() => {
    this._formStatus();
    return this.submitted() && this.form.controls.contact.invalid;
  });
  /** True after first submit attempt when the Sales Config tab group contains invalid fields. */
  protected readonly salesConfigHasErrors = computed(() => {
    this._formStatus();
    return this.submitted() && this.form.controls.salesConfig.invalid;
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

  // ── Employment field errors ────────────────────────────────────────────

  protected readonly nationalIdNumberErrors = computed(() =>
    this.getErrors(this.form.controls.employment.controls.nationalIdNumber, {
      required: 'National ID number is required.',
      maxlength: 'Cannot exceed 15 characters.',
    }),
  );
  protected readonly loginIdErrors = computed(() =>
    this.getErrors(this.form.controls.employment.controls.loginId, {
      required: 'Login ID is required.',
      maxlength: 'Cannot exceed 256 characters.',
    }),
  );
  protected readonly jobTitleErrors = computed(() =>
    this.getErrors(this.form.controls.employment.controls.jobTitle, {
      required: 'Job title is required.',
      maxlength: 'Cannot exceed 50 characters.',
    }),
  );
  protected readonly birthDateErrors = computed(() =>
    this.getErrors(this.form.controls.employment.controls.birthDate, {
      required: 'Birth date is required.',
      minAge:   'Sales person must be at least 18 years old.',
    }),
  );
  /** Hire date errors include both control-level and cross-field group-level validation. */
  protected readonly hireDateErrors = computed((): Record<string, string> | null => {
    this._formStatus();
    const ctrl = this.form.controls.employment.controls.hireDate;
    const groupErrors = this.form.controls.employment.errors;
    if (!ctrl.errors && !groupErrors) return null;
    if (!this.submitted() && !ctrl.touched) return null;
    const msgs: Record<string, string> = {};
    if (ctrl.errors?.['required'])      msgs['required']      = 'Hire date is required.';
    if (ctrl.errors?.['notFutureDate']) msgs['notFutureDate'] = 'Hire date cannot be more than 10 days in the future.';
    if (groupErrors?.['hireAfterBirth']) msgs['hireAfterBirth'] = 'Hire date must be after birth date.';
    if (ctrl.errors?.['server'])        msgs['server']        = ctrl.errors['server'] as string;
    return Object.keys(msgs).length ? msgs : null;
  });
  protected readonly maritalStatusErrors = computed(() =>
    this.getErrors(this.form.controls.employment.controls.maritalStatus, {
      required: 'Marital status is required.',
    }),
  );
  protected readonly genderErrors = computed(() =>
    this.getErrors(this.form.controls.employment.controls.gender, {
      required: 'Gender is required.',
    }),
  );

  // ── Contact field errors ───────────────────────────────────────────────

  protected readonly emailAddressErrors = computed(() =>
    this.getErrors(this.form.controls.contact.controls.emailAddress, {
      required: 'Email address is required.',
      email:    'Enter a valid email address.',
      maxlength: 'Cannot exceed 50 characters.',
    }),
  );
  protected readonly phoneNumberErrors = computed(() =>
    this.getErrors(this.form.controls.contact.controls.phone.controls.phoneNumber, {
      required: 'Phone number is required.',
      maxlength: 'Cannot exceed 25 characters.',
    }),
  );
  protected readonly phoneNumberTypeErrors = computed(() =>
    this.getErrors(this.form.controls.contact.controls.phone.controls.phoneNumberTypeId, {
      required: 'Phone type is required.',
    }),
  );
  protected readonly addressTypeErrors = computed(() =>
    this.getErrors(this.form.controls.contact.controls.addressTypeId, {
      required: 'Address type is required.',
    }),
  );
  protected readonly addressLine1Errors = computed(() =>
    this.getErrors(this.form.controls.contact.controls.address.controls.addressLine1, {
      required: 'Address line 1 is required.',
      maxlength: 'Cannot exceed 60 characters.',
    }),
  );
  protected readonly cityErrors = computed(() =>
    this.getErrors(this.form.controls.contact.controls.address.controls.city, {
      required: 'City is required.',
      maxlength: 'Cannot exceed 30 characters.',
    }),
  );
  protected readonly stateProvinceErrors = computed(() =>
    this.getErrors(this.form.controls.contact.controls.address.controls.stateProvinceId, {
      required: 'State / province is required.',
    }),
  );
  protected readonly postalCodeErrors = computed(() =>
    this.getErrors(this.form.controls.contact.controls.address.controls.postalCode, {
      required: 'Postal code is required.',
      maxlength: 'Cannot exceed 15 characters.',
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
    if (!ctrl.errors || (!this.submitted() && !ctrl.touched)) return null;
    const result: Record<string, string> = {};
    for (const [key, msg] of Object.entries(msgs)) {
      if (ctrl.errors[key]) result[key] = msg ?? key;
    }
    if (ctrl.errors['server']) result['server'] = ctrl.errors['server'] as string;
    return Object.keys(result).length ? result : null;
  }

  ngOnInit(): void {
    forkJoin({
      territories:     this.lookupApi.getTerritories(),
      addressTypes:    this.lookupApi.getAddressTypes(),
      stateProvinces:  this.lookupApi.getStateProvinces(),
      phoneNumberTypes: this.lookupApi.getPhoneNumberTypes(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.territories.set(result.territories);
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

  protected onTabChange(tab: ActiveTab): void {
    this.activeTab.set(tab);
  }

  /**
   * Validates the form and submits a create request.
   *
   * The `stateProvinceId` value emitted by `SelectFieldComponent` is a string; it is parsed to
   * a number and then resolved against the loaded `stateProvinces` signal to obtain the full
   * `StateProvince` object required by the API. Submission is blocked if no matching province
   * is found (guards against stale or tampered select values).
   *
   * On a 400 API response, each `ValidationError.propertyName` is mapped back to its form
   * control via `API_PROPERTY_TO_FORM_PATH`, `setErrors({ server: message })` is called, and
   * the control is marked touched so the inline error renders immediately.
   */
  protected onSubmit(): void {
    if (this.isSaving()) return;
    this.submitted.set(true);
    if (this.form.invalid) return;

    const addressValue = this.form.controls.contact.controls.address.value;
    // SelectFieldComponent CVA emits strings; parse to number for the lookup
    const stateProvinceIdRaw = addressValue.stateProvinceId;
    const stateProvinceId = stateProvinceIdRaw ? Number(stateProvinceIdRaw) : null;
    const selectedState = this.stateProvinces().find((s) => s.id === stateProvinceId);
    if (!selectedState) {
      this.notificationService.error('Selected state is invalid. Please select a valid state and try again.');
      return;
    }

    const personalInfo = this.form.controls.personalInfo.value;
    const employment   = this.form.controls.employment.value;
    const contact      = this.form.controls.contact.value;
    const salesConfig  = this.form.controls.salesConfig.value;

    const phoneNumberTypeIdRaw = contact.phone?.phoneNumberTypeId;
    const addressTypeIdRaw     = contact.addressTypeId;
    const territoryIdRaw       = salesConfig.territoryId;

    const model: SalesPersonCreate = {
      firstName:        personalInfo.firstName ?? '',
      lastName:         personalInfo.lastName ?? '',
      middleName:       personalInfo.middleName ?? null,
      title:            personalInfo.title ?? null,
      suffix:           personalInfo.suffix ?? null,
      nationalIdNumber: employment.nationalIdNumber ?? '',
      loginId:          employment.loginId ?? '',
      jobTitle:         employment.jobTitle ?? '',
      birthDate:        employment.birthDate ?? '',
      hireDate:         employment.hireDate ?? '',
      maritalStatus:    employment.maritalStatus as 'M' | 'S',
      gender:           employment.gender as 'M' | 'F',
      salariedFlag:     employment.salariedFlag ?? false,
      organizationLevel: employment.organizationLevel ?? null,
      emailAddress:     contact.emailAddress ?? '',
      phone: {
        phoneNumber:      contact.phone?.phoneNumber ?? '',
        phoneNumberTypeId: phoneNumberTypeIdRaw ? Number(phoneNumberTypeIdRaw) : null,
      },
      address: {
        addressLine1: addressValue.addressLine1 ?? '',
        addressLine2: addressValue.addressLine2 ?? null,
        city:         addressValue.city ?? '',
        stateProvince: {
          id:   selectedState.id,
          name: selectedState.name,
          code: selectedState.code,
        },
        postalCode: addressValue.postalCode ?? '',
      },
      addressTypeId: addressTypeIdRaw ? Number(addressTypeIdRaw) : null,
      territoryId:   territoryIdRaw ? Number(territoryIdRaw) : null,
      salesQuota:    salesConfig.salesQuota ?? null,
      bonus:         salesConfig.bonus ?? 0,
      commissionPct: salesConfig.commissionPct ?? 0,
    };

    this.isSaving.set(true);
    this.salesApi
      .createSalesPerson(model)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (person) => {
          this.notificationService.success('Sales person created successfully.');
          this.isSaving.set(false);
          void this.router.navigate(['/sales/persons', person.id]);
        },
        error: (err: unknown) => {
          this.isSaving.set(false);
          if (err instanceof ApiValidationError) {
            let anyMapped = false;
            for (const ve of err.errors) {
              const path = API_PROPERTY_TO_FORM_PATH[ve.propertyName];
              if (path) {
                const ctrl = this.form.get(path);
                ctrl?.setErrors({ server: ve.errorMessage });
                ctrl?.markAsTouched();
                anyMapped = true;
              }
            }
            if (!anyMapped) {
              this.notificationService.error('Failed to create sales person. Please try again.');
            }
          } else {
            this.notificationService.error('Failed to create sales person. Please try again.');
          }
        },
      });
  }
}
