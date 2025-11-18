import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { ApiValidationError, ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { SalesPerson } from '@adventureworks-web/sales/data-access';
import { LookupApiService } from '@adventureworks-web/shared/data-access';
import type { AddressType, PhoneNumberType, SalesTerritory, StateProvince } from '@adventureworks-web/shared/data-access';
import { SalesPersonCreateComponent } from './sales-person-create';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockTerritories: SalesTerritory[] = [
  {
    id: 1,
    name: 'Northwest',
    group: 'North America',
    salesYtd: 0,
    salesLastYear: 0,
    costYtd: 0,
    costLastYear: 0,
    countryRegion: { code: 'US', name: 'United States' },
  },
];

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

const mockCreatedPerson: SalesPerson = {
  id: 300,
  title: null,
  firstName: 'Jane',
  middleName: null,
  lastName: 'Doe',
  suffix: null,
  jobTitle: 'Sales Representative',
  emailAddress: 'jane@example.com',
  territoryId: 1,
  salesQuota: null,
  bonus: 0,
  commissionPct: 0,
  salesYtd: 0,
  territoryName: 'Northwest',
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
    },
    employment: {
      nationalIdNumber: '123456789',
      loginId: 'adventure-works\\jane.doe',
      jobTitle: 'Sales Representative',
      birthDate: '1985-03-15',
      hireDate: '2026-01-01',
      maritalStatus: 'S' as const,
      gender: 'F' as const,
      salariedFlag: false,
      organizationLevel: null,
    },
    contact: {
      emailAddress: 'jane@example.com',
      phone: {
        phoneNumber: '555-1234',
        phoneNumberTypeId: 1,
      },
      address: {
        addressLine1: '123 Main St',
        addressLine2: null,
        city: 'Seattle',
        stateProvinceId: 79,
        postalCode: '98101',
      },
      addressTypeId: 2,
    },
    salesConfig: {
      territoryId: 1,
      salesQuota: null,
      bonus: 0,
      commissionPct: 0,
    },
  };
}

describe('SalesPersonCreateComponent', () => {
  let component: SalesPersonCreateComponent;
  let fixture: ComponentFixture<SalesPersonCreateComponent>;
  let salesApiService: SalesApiService;
  let lookupApiService: LookupApiService;
  let notificationService: NotificationService;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalesPersonCreateComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
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
      ],
    }).compileComponents();

    salesApiService = TestBed.inject(SalesApiService);
    lookupApiService = TestBed.inject(LookupApiService);
    notificationService = TestBed.inject(NotificationService);
    router = TestBed.inject(Router);

    vi.spyOn(lookupApiService, 'getTerritories').mockReturnValue(of(mockTerritories));
    vi.spyOn(lookupApiService, 'getAddressTypes').mockReturnValue(of(mockAddressTypes));
    vi.spyOn(lookupApiService, 'getStateProvinces').mockReturnValue(of(mockStateProvinces));
    vi.spyOn(lookupApiService, 'getPhoneNumberTypes').mockReturnValue(of(mockPhoneNumberTypes));
    vi.spyOn(salesApiService, 'createSalesPerson').mockReturnValue(of(mockCreatedPerson));
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(SalesPersonCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('renders without errors', () => {
    expect(component).toBeTruthy();
  });

  describe('reference data loading', () => {
    it('loads all four lookups on ngOnInit and populates signals', () => {
      expect(lookupApiService.getTerritories).toHaveBeenCalled();
      expect(lookupApiService.getAddressTypes).toHaveBeenCalled();
      expect(lookupApiService.getStateProvinces).toHaveBeenCalled();
      expect(lookupApiService.getPhoneNumberTypes).toHaveBeenCalled();

      expect(component['territories']()).toHaveLength(1);
      expect(component['addressTypes']()).toHaveLength(2);
      expect(component['stateProvinces']()).toHaveLength(2);
      expect(component['phoneNumberTypes']()).toHaveLength(3);
    });

    it('sets isLoading to false after successful load', () => {
      expect(component['isLoading']()).toBe(false);
    });

    it('sets isLoading to false and shows error toast when reference data fails', async () => {
      vi.spyOn(lookupApiService, 'getTerritories').mockReturnValue(
        throwError(() => new Error('Network error')),
      );
      vi.spyOn(notificationService, 'error');

      fixture = TestBed.createComponent(SalesPersonCreateComponent);
      component = fixture.componentInstance;
      fixture.detectChanges();

      expect(component['isLoading']()).toBe(false);
      expect(notificationService.error).toHaveBeenCalledWith(
        'Failed to load reference data. Please refresh and try again.',
      );
    });
  });

  describe('form validation', () => {
    it('form starts invalid when fields are empty', () => {
      expect(component['form'].invalid).toBe(true);
    });

    it('submit with empty form sets submitted to true', () => {
      component['onSubmit']();
      expect(component['submitted']()).toBe(true);
    });

    it('submit with empty form does not call createSalesPerson', () => {
      component['onSubmit']();
      expect(salesApiService.createSalesPerson).not.toHaveBeenCalled();
    });

    it('personalInfoHasErrors is true when personalInfo group invalid after submission', () => {
      component['onSubmit']();
      fixture.detectChanges();
      expect(component['personalInfoHasErrors']()).toBe(true);
    });

    it('employmentHasErrors is true when employment group invalid after submission', () => {
      component['onSubmit']();
      fixture.detectChanges();
      expect(component['employmentHasErrors']()).toBe(true);
    });

    it('contactHasErrors is true when contact group invalid after submission', () => {
      component['onSubmit']();
      fixture.detectChanges();
      expect(component['contactHasErrors']()).toBe(true);
    });

    it('personalInfoHasErrors is false before submission even with empty fields', () => {
      expect(component['personalInfoHasErrors']()).toBe(false);
    });

    it('renders error indicator spans with aria-label when form is submitted empty', () => {
      component['onSubmit']();
      fixture.detectChanges();

      const indicators = fixture.nativeElement.querySelectorAll('[aria-label="This section has errors"]');
      expect(indicators.length).toBeGreaterThan(0);
    });

    it('isSaving guard prevents double-submit', () => {
      component['form'].setValue(buildValidFormValue());
      component['isSaving'].set(true);
      component['onSubmit']();
      expect(salesApiService.createSalesPerson).not.toHaveBeenCalled();
    });
  });

  describe('successful submission', () => {
    it('calls createSalesPerson with stateProvince object (full id/name/code, not just id)', () => {
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(salesApiService.createSalesPerson).toHaveBeenCalledWith(
        expect.objectContaining({
          address: expect.objectContaining({
            stateProvince: { id: 79, name: 'Washington', code: 'WA' },
          }),
        }),
      );
    });

    it('calls createSalesPerson with correct personal info fields', () => {
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(salesApiService.createSalesPerson).toHaveBeenCalledWith(
        expect.objectContaining({
          firstName: 'Jane',
          lastName: 'Doe',
          emailAddress: 'jane@example.com',
        }),
      );
    });

    it('shows success toast after create', () => {
      vi.spyOn(notificationService, 'success');
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(notificationService.success).toHaveBeenCalledWith('Sales person created successfully.');
    });

    it('resets isSaving to false before navigating on success', () => {
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(component['isSaving']()).toBe(false);
    });

    it('navigates to the new sales person detail page after create', () => {
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(router.navigate).toHaveBeenCalledWith(['/sales/persons', 300]);
    });
  });

  describe('API failure on submission', () => {
    it('sets isSaving to false on API error', () => {
      vi.spyOn(salesApiService, 'createSalesPerson').mockReturnValue(
        throwError(() => new Error('Server error')),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(component['isSaving']()).toBe(false);
    });

    it('shows error toast on API failure', () => {
      vi.spyOn(salesApiService, 'createSalesPerson').mockReturnValue(
        throwError(() => new Error('Server error')),
      );
      vi.spyOn(notificationService, 'error');
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(notificationService.error).toHaveBeenCalledWith(
        'Failed to create sales person. Please try again.',
      );
    });

    it('does not navigate on API failure', () => {
      vi.spyOn(salesApiService, 'createSalesPerson').mockReturnValue(
        throwError(() => new Error('Server error')),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(router.navigate).not.toHaveBeenCalled();
    });
  });

  describe('stateProvince boundary guard', () => {
    it('does not call createSalesPerson when stateProvinceId has no matching entry in stateProvinces', () => {
      const formValue = buildValidFormValue();
      formValue.contact.address.stateProvinceId = 9999;
      component['form'].setValue(formValue);
      component['onSubmit']();

      expect(salesApiService.createSalesPerson).not.toHaveBeenCalled();
    });

    it('shows distinct error toast when stateProvinceId does not match any loaded state', () => {
      vi.spyOn(notificationService, 'error');
      const formValue = buildValidFormValue();
      formValue.contact.address.stateProvinceId = 9999;
      component['form'].setValue(formValue);
      component['onSubmit']();

      expect(notificationService.error).toHaveBeenCalledWith(
        'Selected state is invalid. Please select a valid state and try again.',
      );
    });
  });

  describe('navigation', () => {
    it('cancel link has routerLink to /sales/persons', () => {
      fixture.detectChanges();
      const compiled: HTMLElement = fixture.nativeElement;
      const cancelLink = compiled.querySelector('#aw-sales-person-create-cancel') as HTMLAnchorElement | null;
      expect(cancelLink).toBeTruthy();
    });

    it('back link has routerLink to /sales/persons', () => {
      fixture.detectChanges();
      const compiled: HTMLElement = fixture.nativeElement;
      const backLink = compiled.querySelector('#aw-sales-person-create-back') as HTMLAnchorElement | null;
      expect(backLink).toBeTruthy();
    });
  });

  describe('tab navigation', () => {
    it('activeTab defaults to personalInfo', () => {
      expect(component['activeTab']()).toBe('personalInfo');
    });

    it('onTabChange updates activeTab', () => {
      component['onTabChange']('employment');
      expect(component['activeTab']()).toBe('employment');
    });
  });

  describe('date validation', () => {
    it('birthDate with age < 18 years produces minAge error', () => {
      const tooYoung = new Date();
      tooYoung.setFullYear(tooYoung.getFullYear() - 17);
      const ctrl = component['form'].controls.employment.controls.birthDate;
      ctrl.setValue(tooYoung.toISOString().slice(0, 10));
      ctrl.markAsTouched();
      expect(ctrl.errors?.['minAge']).toBeTruthy();
    });

    it('birthDate with age >= 18 years has no minAge error', () => {
      const ctrl = component['form'].controls.employment.controls.birthDate;
      ctrl.setValue('1985-03-15');
      expect(ctrl.errors?.['minAge']).toBeFalsy();
    });

    it('hireDate more than 10 days in the future produces notFutureDate error', () => {
      const elevenDaysOut = new Date();
      elevenDaysOut.setDate(elevenDaysOut.getDate() + 11);
      const ctrl = component['form'].controls.employment.controls.hireDate;
      ctrl.setValue(elevenDaysOut.toISOString().slice(0, 10));
      ctrl.markAsTouched();
      expect(ctrl.errors?.['notFutureDate']).toBeTruthy();
    });

    it('hireDate 10 days from now has no notFutureDate error', () => {
      const tenDaysOut = new Date();
      tenDaysOut.setDate(tenDaysOut.getDate() + 10);
      const ctrl = component['form'].controls.employment.controls.hireDate;
      ctrl.setValue(tenDaysOut.toISOString().slice(0, 10));
      expect(ctrl.errors?.['notFutureDate']).toBeFalsy();
    });

    it('hireDate in the past has no notFutureDate error', () => {
      const ctrl = component['form'].controls.employment.controls.hireDate;
      ctrl.setValue('2024-01-01');
      expect(ctrl.errors?.['notFutureDate']).toBeFalsy();
    });

    it('hireDate before birthDate produces hireAfterBirth group error', () => {
      const employment = component['form'].controls.employment;
      employment.controls.birthDate.setValue('1990-06-15');
      employment.controls.hireDate.setValue('1990-01-01');
      employment.controls.hireDate.markAsTouched();
      expect(employment.errors?.['hireAfterBirth']).toBeTruthy();
    });

    it('hireAfterBirth group error surfaces in hireDateErrors computed after submission', () => {
      component['submitted'].set(true);
      const employment = component['form'].controls.employment;
      employment.controls.birthDate.setValue('1990-06-15');
      employment.controls.hireDate.setValue('1990-01-01');
      employment.controls.hireDate.markAsTouched();
      fixture.detectChanges();
      expect(component['hireDateErrors']()?.['hireAfterBirth']).toBeTruthy();
    });
  });

  describe('title and suffix maxLength validation', () => {
    it('title longer than 8 characters produces maxlength error', () => {
      const ctrl = component['form'].controls.personalInfo.controls.title;
      ctrl.setValue('MrMrMrMrX'); // 9 chars
      ctrl.markAsTouched();
      expect(ctrl.errors?.['maxlength']).toBeTruthy();
    });

    it('title at exactly 8 characters has no maxlength error', () => {
      const ctrl = component['form'].controls.personalInfo.controls.title;
      ctrl.setValue('Dr. Sir.'); // 8 chars
      expect(ctrl.errors?.['maxlength']).toBeFalsy();
    });

    it('suffix longer than 10 characters produces maxlength error', () => {
      const ctrl = component['form'].controls.personalInfo.controls.suffix;
      ctrl.setValue('Jr. Sr. IV.'); // 11 chars
      ctrl.markAsTouched();
      expect(ctrl.errors?.['maxlength']).toBeTruthy();
    });

    it('suffix at exactly 10 characters has no maxlength error', () => {
      const ctrl = component['form'].controls.personalInfo.controls.suffix;
      ctrl.setValue('Jr. Sr. IV'); // 10 chars
      expect(ctrl.errors?.['maxlength']).toBeFalsy();
    });
  });

  describe('API validation error mapping', () => {
    it('maps ApiValidationError top-level propertyName (BirthDate) to the correct form control', () => {
      vi.spyOn(salesApiService, 'createSalesPerson').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'BirthDate', errorCode: 'Rule-22', errorMessage: 'Sales person must be at least 18 years old', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      const ctrl = component['form'].controls.employment.controls.birthDate;
      expect(ctrl.errors?.['server']).toBe('Sales person must be at least 18 years old');
      expect(ctrl.touched).toBe(true);
    });

    it('maps nested Phone.PhoneNumber propertyName to the correct form control', () => {
      vi.spyOn(salesApiService, 'createSalesPerson').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'Phone.PhoneNumber', errorCode: 'Rule-30', errorMessage: 'Phone number cannot be greater than 25 characters', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      const ctrl = component['form'].controls.contact.controls.phone.controls.phoneNumber;
      expect(ctrl.errors?.['server']).toBe('Phone number cannot be greater than 25 characters');
      expect(ctrl.touched).toBe(true);
    });

    it('maps nested Address.City propertyName to the correct form control', () => {
      vi.spyOn(salesApiService, 'createSalesPerson').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'Address.City', errorCode: 'Rule-04', errorMessage: 'City cannot be null, empty, or whitespace', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      const ctrl = component['form'].controls.contact.controls.address.controls.city;
      expect(ctrl.errors?.['server']).toBe('City cannot be null, empty, or whitespace');
      expect(ctrl.touched).toBe(true);
    });

    it('does not show a generic toast when at least one property is mapped', () => {
      vi.spyOn(notificationService, 'error');
      vi.spyOn(salesApiService, 'createSalesPerson').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'HireDate', errorCode: 'Rule-24', errorMessage: 'Hire date cannot be in the future', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(notificationService.error).not.toHaveBeenCalled();
    });

    it('shows generic toast when no property in ApiValidationError maps to the form', () => {
      vi.spyOn(notificationService, 'error');
      vi.spyOn(salesApiService, 'createSalesPerson').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'UnknownField', errorCode: 'Rule-99', errorMessage: 'Unknown', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(notificationService.error).toHaveBeenCalledWith('Failed to create sales person. Please try again.');
    });

    it('shows generic toast for non-validation errors (e.g. 500)', () => {
      vi.spyOn(notificationService, 'error');
      vi.spyOn(salesApiService, 'createSalesPerson').mockReturnValue(
        throwError(() => new Error('Internal server error')),
      );
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();

      expect(notificationService.error).toHaveBeenCalledWith('Failed to create sales person. Please try again.');
    });

    it('server error is surfaced via getErrors in the field error computed after markAsTouched', () => {
      vi.spyOn(salesApiService, 'createSalesPerson').mockReturnValue(
        throwError(() => new ApiValidationError(
          [{ propertyName: 'FirstName', errorCode: 'Rule-10', errorMessage: 'First name cannot exceed 50 characters', correlationId: 'abc' }],
          'abc',
        )),
      );
      component['submitted'].set(true);
      component['form'].setValue(buildValidFormValue());
      component['onSubmit']();
      fixture.detectChanges();

      expect(component['firstNameErrors']()?.['server']).toBe('First name cannot exceed 50 characters');
    });
  });
});
