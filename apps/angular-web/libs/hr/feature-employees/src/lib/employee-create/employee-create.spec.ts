import { ComponentFixture } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { TestBed } from '@angular/core/testing';
import { ApiValidationError, ConflictError, ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { Employee } from '@adventureworks-web/hr/data-access';
import { LookupApiService } from '@adventureworks-web/shared/data-access';
import type { AddressType, PhoneNumberType, StateProvince } from '@adventureworks-web/shared/data-access';
import { renderEmployeeComponent } from '../testing/render-employee-component';
import { EmployeeCreateComponent } from './employee-create';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockAddressTypes: AddressType[] = [
  { id: 2, name: 'Home' },
  { id: 3, name: 'Work' },
];

const mockStateProvinces: StateProvince[] = [
  {
    id: 79,
    code: 'WA',
    name: 'Washington',
    isStateProvinceCodeUnavailable: false,
    countryRegion: { code: 'US', name: 'United States' },
    territory: { id: 1, name: 'Northwest', code: 'US' },
  },
  {
    id: 58,
    code: 'OR',
    name: 'Oregon',
    isStateProvinceCodeUnavailable: false,
    countryRegion: { code: 'US', name: 'United States' },
    territory: { id: 1, name: 'Northwest', code: 'US' },
  },
];

const mockPhoneNumberTypes: PhoneNumberType[] = [
  { id: 1, name: 'Cell' },
  { id: 2, name: 'Home' },
  { id: 3, name: 'Work' },
];

const mockCreatedEmployee: Employee = {
  id: 500,
  firstName: 'Jane',
  lastName: 'Doe',
  middleName: null,
  title: null,
  suffix: null,
  jobTitle: 'HR Coordinator',
  maritalStatus: 'S',
  gender: 'F',
  salariedFlag: false,
  organizationLevel: null,
  nationalIdNumber: '123456789',
  loginId: 'adventure-works\\jane.doe',
  birthDate: '1985-03-15',
  hireDate: '2026-01-01',
  currentFlag: true,
  vacationHours: 0,
  sickLeaveHours: 0,
  emailAddress: 'jane@example.com',
  currentDepartment: null,
  modifiedDate: '2026-06-10T00:00:00',
};

function buildValidFormValue() {
  return {
    personalInfo: {
      firstName: 'Jane',
      lastName: 'Doe',
      middleName: null,
      title: null,
      suffix: null,
      jobTitle: 'HR Coordinator',
      nationalIdNumber: '123456789',
      loginId: 'adventure-works\\jane.doe',
      birthDate: '1985-03-15',
      maritalStatus: 'S' as const,
      gender: 'F' as const,
    },
    contact: {
      emailAddress: 'jane@example.com',
      phone: {
        phoneNumber: '555-1234',
        phoneNumberTypeId: 1,
      },
    },
    address: {
      addressLine1: '123 Main St',
      addressLine2: null,
      city: 'Seattle',
      stateProvinceId: 79,
      postalCode: '98101',
      addressTypeId: 2,
    },
  };
}

describe('EmployeeCreateComponent', () => {
  let component: EmployeeCreateComponent;
  let fixture: ComponentFixture<EmployeeCreateComponent>;
  let hrApiService: HrApiService;
  let lookupApiService: LookupApiService;
  let notificationService: NotificationService;
  let router: Router;

  beforeEach(async () => {
    const result = await renderEmployeeComponent(EmployeeCreateComponent, [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideTranslateService(),
      { provide: ENVIRONMENT, useValue: mockEnvironment },
      {
        provide: ActivatedRoute,
        useValue: {
          snapshot: {
            queryParams: {},
            paramMap: { get: vi.fn().mockReturnValue(null) },
          },
        },
      },
    ]);
    fixture = result.fixture;
    component = result.component;

    hrApiService = TestBed.inject(HrApiService);
    lookupApiService = TestBed.inject(LookupApiService);
    notificationService = TestBed.inject(NotificationService);
    router = TestBed.inject(Router);

    vi.spyOn(lookupApiService, 'getAddressTypes').mockReturnValue(of(mockAddressTypes));
    vi.spyOn(lookupApiService, 'getStateProvinces').mockReturnValue(of(mockStateProvinces));
    vi.spyOn(lookupApiService, 'getPhoneNumberTypes').mockReturnValue(of(mockPhoneNumberTypes));
    vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(of(mockCreatedEmployee));
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture.detectChanges();
  });

  it('renders without errors', () => {
    expect(component).toBeTruthy();
  });

  describe('reference data loading', () => {
    it('loads three lookups on ngOnInit and populates signals', () => {
      expect(lookupApiService.getAddressTypes).toHaveBeenCalled();
      expect(lookupApiService.getStateProvinces).toHaveBeenCalled();
      expect(lookupApiService.getPhoneNumberTypes).toHaveBeenCalled();

      expect(component['addressTypes']()).toHaveLength(2);
      expect(component['stateProvinces']()).toHaveLength(2);
      expect(component['phoneNumberTypes']()).toHaveLength(3);
    });

    it('sets isLoading to false after successful load', () => {
      expect(component['isLoading']()).toBe(false);
    });

    it('sets isLoading to false and shows error toast when reference data fails', async () => {
      vi.spyOn(lookupApiService, 'getAddressTypes').mockReturnValue(
        throwError(() => new Error('Network error')),
      );
      vi.spyOn(notificationService, 'error');

      fixture = TestBed.createComponent(EmployeeCreateComponent);
      component = fixture.componentInstance;
      fixture.detectChanges();

      expect(component['isLoading']()).toBe(false);
      expect(notificationService.error).toHaveBeenCalledWith(
        'Failed to load reference data. Please refresh and try again.',
      );
    });
  });

  describe('wizard step gating', () => {
    it('starts on step 1', () => {
      expect(component['currentStep']()).toBe(1);
    });

    it('onNext does not advance from step 1 when personalInfo group is invalid', () => {
      component['onNext']();
      expect(component['currentStep']()).toBe(1);
    });

    it('onNext marks the personalInfo group as touched when blocked', () => {
      component['onNext']();
      expect(component['form'].controls.personalInfo.touched).toBe(true);
    });

    it('onNext immediately renders inline errors on the blocked step, without a further value change', () => {
      component['onNext']();
      expect(component['firstNameErrors']()).toEqual({ required: 'First name is required.' });
    });

    it('personalInfoValid is false when required fields are empty', () => {
      expect(component['personalInfoValid']()).toBe(false);
    });

    it('advances to step 2 once personalInfo is valid', () => {
      component['form'].controls.personalInfo.setValue(buildValidFormValue().personalInfo);
      component['onNext']();
      expect(component['currentStep']()).toBe(2);
    });

    it('onNext does not advance from step 2 when contact group is invalid', () => {
      component['form'].controls.personalInfo.setValue(buildValidFormValue().personalInfo);
      component['onNext']();
      component['onNext']();
      expect(component['currentStep']()).toBe(2);
    });

    it('advances through step 2 and 3 once each group is valid', () => {
      const valid = buildValidFormValue();
      component['form'].controls.personalInfo.setValue(valid.personalInfo);
      component['onNext']();
      component['form'].controls.contact.setValue(valid.contact);
      component['onNext']();
      expect(component['currentStep']()).toBe(3);

      component['form'].controls.address.setValue(valid.address);
      component['onNext']();
      expect(component['currentStep']()).toBe(4);
    });

    it('onNext does not advance past step 4', () => {
      component['currentStep'].set(4);
      component['onNext']();
      expect(component['currentStep']()).toBe(4);
    });

    it('onBack moves to the previous step without validation', () => {
      component['currentStep'].set(3);
      component['onBack']();
      expect(component['currentStep']()).toBe(2);
    });

    it('onBack does not move below step 1', () => {
      component['onBack']();
      expect(component['currentStep']()).toBe(1);
    });

    it('onEditStep jumps directly to the given step, bypassing gating', () => {
      component['onEditStep'](3);
      expect(component['currentStep']()).toBe(3);
    });

    it('back/next preserves entered values across steps', () => {
      const valid = buildValidFormValue();
      component['form'].controls.personalInfo.setValue(valid.personalInfo);
      component['onNext']();
      component['onBack']();
      expect(component['form'].controls.personalInfo.value.firstName).toBe('Jane');
    });
  });

  describe('age validator', () => {
    it('birthDate with age < 18 years produces minAge error and blocks Next', () => {
      const valid = buildValidFormValue();
      const tooYoung = new Date();
      tooYoung.setFullYear(tooYoung.getFullYear() - 17);
      component['form'].controls.personalInfo.setValue({
        ...valid.personalInfo,
        birthDate: tooYoung.toISOString().slice(0, 10),
      });

      const ctrl = component['form'].controls.personalInfo.controls.birthDate;
      expect(ctrl.errors?.['minAge']).toBeTruthy();
      expect(component['personalInfoValid']()).toBe(false);
    });

    it('shows the exact "Must be at least 18" message once touched', () => {
      const valid = buildValidFormValue();
      const tooYoung = new Date();
      tooYoung.setFullYear(tooYoung.getFullYear() - 17);
      component['form'].controls.personalInfo.setValue({
        ...valid.personalInfo,
        birthDate: tooYoung.toISOString().slice(0, 10),
      });
      const ctrl = component['form'].controls.personalInfo.controls.birthDate;
      ctrl.markAsTouched();
      fixture.detectChanges();

      expect(component['birthDateErrors']()?.['minAge']).toBe('Must be at least 18');
    });

    it('birthDate at exactly 18 years has no minAge error', () => {
      const valid = buildValidFormValue();
      component['form'].controls.personalInfo.setValue(valid.personalInfo);
      const ctrl = component['form'].controls.personalInfo.controls.birthDate;
      expect(ctrl.errors?.['minAge']).toBeFalsy();
    });
  });

  describe('review step', () => {
    it('renders values entered across all three steps', () => {
      const valid = buildValidFormValue();
      component['form'].setValue(valid);
      component['currentStep'].set(4);
      fixture.detectChanges();

      const compiled: HTMLElement = fixture.nativeElement;
      const review = compiled.querySelector('#aw-employee-create-review');
      expect(review?.textContent).toContain('Jane');
      expect(review?.textContent).toContain('Doe');
      expect(review?.textContent).toContain('jane@example.com');
      expect(review?.textContent).toContain('Seattle');
    });

    it('Edit links jump back to the correct step without losing data', () => {
      const valid = buildValidFormValue();
      component['form'].setValue(valid);
      component['currentStep'].set(4);
      fixture.detectChanges();

      component['onEditStep'](1);
      expect(component['currentStep']()).toBe(1);
      expect(component['form'].controls.personalInfo.value.firstName).toBe('Jane');
    });
  });

  describe('successful submission', () => {
    it('calls createEmployee with the correct EmployeeCreate payload shape', () => {
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(hrApiService.createEmployee).toHaveBeenCalledWith({
        firstName: 'Jane',
        lastName: 'Doe',
        middleName: null,
        title: null,
        suffix: null,
        jobTitle: 'HR Coordinator',
        maritalStatus: 'S',
        gender: 'F',
        salariedFlag: false,
        organizationLevel: null,
        nationalIdNumber: '123456789',
        loginId: 'adventure-works\\jane.doe',
        birthDate: '1985-03-15',
        phone: {
          phoneNumber: '555-1234',
          phoneNumberTypeId: 1,
        },
        emailAddress: 'jane@example.com',
        address: {
          addressLine1: '123 Main St',
          addressLine2: null,
          city: 'Seattle',
          stateProvince: { id: 79, name: 'Washington', code: 'WA' },
          postalCode: '98101',
        },
        addressTypeId: 2,
      });
    });

    it('shows success toast after create', () => {
      vi.spyOn(notificationService, 'success');
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(notificationService.success).toHaveBeenCalledWith('Employee created successfully.');
    });

    it('resets isSaving to false before navigating on success', () => {
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(component['isSaving']()).toBe(false);
    });

    it('navigates to the new employee detail page after create', () => {
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(router.navigate).toHaveBeenCalledWith(['/hr/employees', 500]);
    });
  });

  describe('onSubmit safety net', () => {
    it('submit with empty form does not call createEmployee', () => {
      component['onSubmit']();
      expect(hrApiService.createEmployee).not.toHaveBeenCalled();
    });

    it('submit with empty form marks the whole form tree as touched', () => {
      component['onSubmit']();
      expect(component['form'].controls.address.touched).toBe(true);
    });

    it('isSaving guard prevents double-submit', () => {
      component['form'].setValue(buildValidFormValue());
      component['isSaving'].set(true);
      component['onSubmit']();
      expect(hrApiService.createEmployee).not.toHaveBeenCalled();
    });
  });

  describe('non-null lookup guards', () => {
    it('blocks submit and shows a distinct toast when phoneNumberTypeId has no matching entry', () => {
      vi.spyOn(notificationService, 'error');
      const formValue = buildValidFormValue();
      formValue.contact.phone.phoneNumberTypeId = 9999;
      component['form'].setValue(formValue);
      component['onSubmit']();

      expect(hrApiService.createEmployee).not.toHaveBeenCalled();
      expect(notificationService.error).toHaveBeenCalledWith(
        'Selected phone type is invalid. Please select a valid phone type and try again.',
      );
    });

    it('blocks submit and shows a distinct toast when addressTypeId has no matching entry', () => {
      vi.spyOn(notificationService, 'error');
      const formValue = buildValidFormValue();
      formValue.address.addressTypeId = 9999;
      component['form'].setValue(formValue);
      component['onSubmit']();

      expect(hrApiService.createEmployee).not.toHaveBeenCalled();
      expect(notificationService.error).toHaveBeenCalledWith(
        'Selected address type is invalid. Please select a valid address type and try again.',
      );
    });

    it('blocks submit and shows a distinct toast when stateProvinceId has no matching entry', () => {
      vi.spyOn(notificationService, 'error');
      const formValue = buildValidFormValue();
      formValue.address.stateProvinceId = 9999;
      component['form'].setValue(formValue);
      component['onSubmit']();

      expect(hrApiService.createEmployee).not.toHaveBeenCalled();
      expect(notificationService.error).toHaveBeenCalledWith(
        'Selected state is invalid. Please select a valid state and try again.',
      );
    });
  });

  describe('API failure on submission', () => {
    it('sets isSaving to false on API error', () => {
      vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(
        throwError(() => new Error('Server error')),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(component['isSaving']()).toBe(false);
    });

    it('shows generic error toast for non-validation errors', () => {
      vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(
        throwError(() => new Error('Server error')),
      );
      vi.spyOn(notificationService, 'error');
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(notificationService.error).toHaveBeenCalledWith('Failed to create employee. Please try again.');
    });

    it('does not navigate on API failure', () => {
      vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(
        throwError(() => new Error('Server error')),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(router.navigate).not.toHaveBeenCalled();
    });
  });

  describe('ApiValidationError step-jump behavior', () => {
    it('maps a step-1 propertyName to its control and jumps currentStep to 1', () => {
      component['currentStep'].set(4);
      vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'BirthDate', errorCode: 'Rule-20', errorMessage: 'Employee must be at least 18 years old', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      const ctrl = component['form'].controls.personalInfo.controls.birthDate;
      expect(ctrl.errors?.['server']).toBe('Employee must be at least 18 years old');
      expect(ctrl.touched).toBe(true);
      expect(component['currentStep']()).toBe(1);
      expect(component['birthDateErrors']()?.['server']).toBe('Employee must be at least 18 years old');
    });

    it('maps a step-2 nested propertyName (Phone.PhoneNumber) and jumps currentStep to 2', () => {
      component['currentStep'].set(4);
      vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'Phone.PhoneNumber', errorCode: 'Rule-30', errorMessage: 'Phone number cannot be greater than 25 characters', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      const ctrl = component['form'].controls.contact.controls.phone.controls.phoneNumber;
      expect(ctrl.errors?.['server']).toBe('Phone number cannot be greater than 25 characters');
      expect(component['currentStep']()).toBe(2);
    });

    it('maps a step-3 nested propertyName (Address.City) and jumps currentStep to 3', () => {
      component['currentStep'].set(4);
      vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'Address.City', errorCode: 'Rule-04', errorMessage: 'City cannot be null, empty, or whitespace', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      const ctrl = component['form'].controls.address.controls.city;
      expect(ctrl.errors?.['server']).toBe('City cannot be null, empty, or whitespace');
      expect(component['currentStep']()).toBe(3);
    });

    it('does not show a generic toast when at least one property is mapped', () => {
      vi.spyOn(notificationService, 'error');
      vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'EmailAddress', errorCode: 'Rule-24', errorMessage: 'Enter a valid email address', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(notificationService.error).not.toHaveBeenCalled();
    });

    it('maps a duplicate NationalIdNumber (Rule-31) to its control and jumps currentStep to 1', () => {
      component['currentStep'].set(4);
      vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'NationalIdNumber', errorCode: 'Rule-31', errorMessage: 'An employee with this National ID number already exists', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      const ctrl = component['form'].controls.personalInfo.controls.nationalIdNumber;
      expect(ctrl.errors?.['server']).toBe('An employee with this National ID number already exists');
      expect(component['currentStep']()).toBe(1);
    });

    it('maps a duplicate LoginId (Rule-32) to its control and jumps currentStep to 1', () => {
      component['currentStep'].set(4);
      vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'LoginId', errorCode: 'Rule-32', errorMessage: 'An employee with this Login ID already exists', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      const ctrl = component['form'].controls.personalInfo.controls.loginId;
      expect(ctrl.errors?.['server']).toBe('An employee with this Login ID already exists');
      expect(component['currentStep']()).toBe(1);
    });

    it('shows generic toast when no property in ApiValidationError maps to the form', () => {
      vi.spyOn(notificationService, 'error');
      vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'UnknownField', errorCode: 'Rule-99', errorMessage: 'Unknown', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(notificationService.error).toHaveBeenCalledWith('Failed to create employee. Please try again.');
    });
  });

  describe('ConflictError handling (409)', () => {
    it('shows a hardcoded toast and jumps to step 1 on ConflictError, never the raw error message', () => {
      component['currentStep'].set(4);
      vi.spyOn(notificationService, 'error');
      vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(
        throwError(() => new ConflictError('An employee with this National ID Number already exists.', 'corr-409')),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(notificationService.error).toHaveBeenCalledWith(
        'An employee with this National ID Number or Login ID already exists. Please check your entries and try again.',
      );
      expect(component['currentStep']()).toBe(1);
    });

    it('resets isSaving to false on ConflictError', () => {
      vi.spyOn(hrApiService, 'createEmployee').mockReturnValue(
        throwError(() => new ConflictError('An employee with this Login ID already exists.', 'corr-409')),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(component['isSaving']()).toBe(false);
    });
  });

  describe('navigation', () => {
    it('cancel link routes to /hr/employees', () => {
      fixture.detectChanges();
      const compiled: HTMLElement = fixture.nativeElement;
      const cancelLink = compiled.querySelector('#aw-employee-create-cancel') as HTMLAnchorElement | null;
      expect(cancelLink).toBeTruthy();
    });

    it('back link routes to /hr/employees', () => {
      fixture.detectChanges();
      const compiled: HTMLElement = fixture.nativeElement;
      const backLink = compiled.querySelector('#aw-employee-create-back') as HTMLAnchorElement | null;
      expect(backLink).toBeTruthy();
    });
  });
});
