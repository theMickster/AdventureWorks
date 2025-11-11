import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { SalesPerson, Store } from '@adventureworks-web/sales/data-access';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import { StoreCreateComponent } from './store-create';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

// Sales persons sourced from AdventureWorks Sales.SalesPerson joined to Person.Person
const mockSalesPersons: SalesPerson[] = [
  {
    id: 274,
    title: null,
    firstName: 'Stephen',
    middleName: 'Y',
    lastName: 'Jiang',
    suffix: null,
    jobTitle: 'North American Sales Manager',
    emailAddress: 'stephen@example.com',
    territoryId: null,
    salesQuota: null,
    bonus: 0,
    commissionPct: 0,
    modifiedDate: '2026-01-01T00:00:00',
  },
  {
    id: 275,
    title: null,
    firstName: 'Michael',
    middleName: 'G',
    lastName: 'Blythe',
    suffix: null,
    jobTitle: 'Sales Representative',
    emailAddress: 'michael@example.com',
    territoryId: 1,
    salesQuota: 300000,
    bonus: 4100,
    commissionPct: 0.012,
    modifiedDate: '2026-01-01T00:00:00',
  },
  {
    id: 279,
    title: null,
    firstName: 'Tsvi',
    middleName: null,
    lastName: 'Reiter',
    suffix: null,
    jobTitle: 'Sales Representative',
    emailAddress: 'tsvi@example.com',
    territoryId: 6,
    salesQuota: 250000,
    bonus: 4000,
    commissionPct: 0.015,
    modifiedDate: '2026-01-01T00:00:00',
  },
];

const mockSalesPersonsResult: SearchResult<SalesPerson> = {
  pageNumber: 1,
  pageSize: 25,
  totalPages: 1,
  hasPreviousPage: false,
  hasNextPage: false,
  totalRecords: 3,
  results: mockSalesPersons,
};

const mockCreatedStore: Store = {
  id: 800,
  name: 'Contoso Bikes',
  modifiedDate: '2026-06-01T00:00:00',
  storeAddresses: [],
  storeContacts: [],
  salesPerson: null,
};

describe('StoreCreateComponent', () => {
  let component: StoreCreateComponent;
  let fixture: ComponentFixture<StoreCreateComponent>;
  let salesApiService: SalesApiService;
  let notificationService: NotificationService;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StoreCreateComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: { snapshot: { queryParams: {}, paramMap: { get: vi.fn().mockReturnValue(null) } } } },
      ],
    }).compileComponents();

    salesApiService = TestBed.inject(SalesApiService);
    notificationService = TestBed.inject(NotificationService);
    router = TestBed.inject(Router);

    vi.spyOn(salesApiService, 'getSalesPersons').mockReturnValue(of(mockSalesPersonsResult));
    vi.spyOn(salesApiService, 'createStore').mockReturnValue(of(mockCreatedStore));
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(StoreCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('renders without errors', () => {
    expect(component).toBeTruthy();
  });

  it('loads sales persons on init and populates options', () => {
    expect(salesApiService.getSalesPersons).toHaveBeenCalledWith({ pageNumber: 1, pageSize: 25 });
    expect(component['salesPersons']()).toHaveLength(3);
    expect(component['salesPersonOptions']()).toHaveLength(3);
    expect(component['salesPersonOptions']()[0].label).toBe('Stephen Jiang');
  });

  it('form starts invalid when name is empty', () => {
    expect(component['form'].invalid).toBe(true);
  });

  it('does not call createStore when form is invalid on submit', () => {
    component['onSubmit']();
    expect(salesApiService.createStore).not.toHaveBeenCalled();
  });

  it('sets submitted signal on invalid submit so nameErrors become visible', () => {
    component['onSubmit']();
    expect(component['submitted']()).toBe(true);
  });

  it('nameErrors returns required error after submit attempt with empty name', () => {
    component['onSubmit'](); // sets submitted = true
    fixture.detectChanges();

    const errors = component['nameErrors']();
    expect(errors).not.toBeNull();
    expect((errors as Record<string, string>)['required']).toBe('Name is required.');
  });

  it('nameErrors returns maxlength error when name exceeds 50 characters after submit', () => {
    component['form'].controls.name.setValue('a'.repeat(51));
    component['submitted'].set(true);
    fixture.detectChanges();

    const errors = component['nameErrors']();
    expect(errors).not.toBeNull();
    expect((errors as Record<string, string>)['maxlength']).toBe('Name cannot exceed 50 characters.');
  });

  it('nameErrors returns null before submit when control is untouched', () => {
    component['form'].controls.name.setValue('');
    // submitted is false, not touched — errors should not show
    expect(component['nameErrors']()).toBeNull();
  });

  it('nameErrors returns null when name is valid after submit', () => {
    component['form'].controls.name.setValue('Valid Store Name');
    component['submitted'].set(true);
    fixture.detectChanges();

    expect(component['nameErrors']()).toBeNull();
  });

  it('whitespace-only name is trimmed to empty and triggers required error', () => {
    component['form'].controls.name.setValue('   ');
    component['onSubmit']();

    expect(component['form'].controls.name.value).toBe('');
    const errors = component['nameErrors']();
    expect(errors).not.toBeNull();
    expect((errors as Record<string, string>)['required']).toBe('Name is required.');
    expect(salesApiService.createStore).not.toHaveBeenCalled();
  });

  it('leading and trailing whitespace is trimmed before submission', () => {
    component['form'].setValue({ name: '  Contoso Bikes  ', salesPersonId: null });
    component['onSubmit']();

    expect(salesApiService.createStore).toHaveBeenCalledWith({ name: 'Contoso Bikes', salesPersonId: null });
  });

  it('second call while loading is blocked (double-submit guard)', () => {
    component['form'].setValue({ name: 'Contoso Bikes', salesPersonId: null });
    component['onSubmit']();
    component['onSubmit'](); // second call should be blocked
    expect(salesApiService.createStore).toHaveBeenCalledTimes(1);
  });

  it('valid name only calls createStore with salesPersonId null and navigates to detail', () => {
    component['form'].setValue({ name: 'Contoso Bikes', salesPersonId: null });
    component['onSubmit']();

    expect(salesApiService.createStore).toHaveBeenCalledWith({ name: 'Contoso Bikes', salesPersonId: null });
    expect(notificationService['notifications']()).toEqual(
      expect.arrayContaining([expect.objectContaining({ type: 'success', message: 'Store created successfully.' })]),
    );
    expect(router.navigate).toHaveBeenCalledWith(['/sales/stores', 800]);
  });

  it('valid name with salesPersonId calls createStore with both fields', () => {
    component['form'].setValue({ name: 'Contoso Bikes', salesPersonId: '279' });
    component['onSubmit']();

    expect(salesApiService.createStore).toHaveBeenCalledWith({ name: 'Contoso Bikes', salesPersonId: 279 });
  });

  it('API create error shows error toast and does not navigate', () => {
    vi.spyOn(salesApiService, 'createStore').mockReturnValue(throwError(() => new Error('Server error')));
    vi.spyOn(notificationService, 'error');

    component['form'].setValue({ name: 'Contoso Bikes', salesPersonId: null });
    component['onSubmit']();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to create store. Please try again.');
    expect(router.navigate).not.toHaveBeenCalled();
    expect(component['isLoading']()).toBe(false);
  });

  it('getSalesPersons error shows error toast but form remains usable', () => {
    vi.spyOn(salesApiService, 'getSalesPersons').mockReturnValue(throwError(() => new Error('Network error')));
    vi.spyOn(notificationService, 'error');

    // Recreate component to trigger ngOnInit with the error mock
    fixture = TestBed.createComponent(StoreCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to load sales persons.');
    expect(component['form'].valid).toBe(false); // name still empty — form works normally
    component['form'].controls.name.setValue('Test Store');
    expect(component['form'].valid).toBe(true); // form is usable
  });
});
