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
import { StoreEditComponent } from './store-edit';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

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
    salesYtd: 559697.5639,
    territoryName: null,
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
    salesYtd: 3763178.1787,
    territoryName: 'Central',
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
    salesYtd: 1750406.4785,
    territoryName: 'Southeast',
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

// Store 292 — Next-Door Bike Store (consistent with store-detail.spec.ts)
const mockStore: Store = {
  id: 292,
  name: 'Next-Door Bike Store',
  modifiedDate: '2026-01-01T00:00:00',
  storeAddresses: [],
  storeContacts: [],
  salesPerson: {
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
};

function buildRoute(id = '292', queryParams: Record<string, string> = {}) {
  return {
    snapshot: {
      paramMap: { get: vi.fn().mockReturnValue(id) },
      queryParams,
    },
  };
}

describe('StoreEditComponent', () => {
  let component: StoreEditComponent;
  let fixture: ComponentFixture<StoreEditComponent>;
  let salesApiService: SalesApiService;
  let notificationService: NotificationService;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [StoreEditComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    salesApiService = TestBed.inject(SalesApiService);
    notificationService = TestBed.inject(NotificationService);
    router = TestBed.inject(Router);

    vi.spyOn(salesApiService, 'getStore').mockReturnValue(of(mockStore));
    vi.spyOn(salesApiService, 'getSalesPersons').mockReturnValue(of(mockSalesPersonsResult));
    vi.spyOn(salesApiService, 'updateStore').mockReturnValue(of(mockStore));
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(StoreEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('renders without errors', () => {
    expect(component).toBeTruthy();
  });

  it('redirects to list when id is NaN', () => {
    route.snapshot.paramMap.get = vi.fn().mockReturnValue('abc');
    vi.mocked(router.navigate).mockClear();
    vi.mocked(salesApiService.getStore).mockClear();
    fixture = TestBed.createComponent(StoreEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(router.navigate).toHaveBeenCalledWith(['/sales/stores']);
    expect(salesApiService.getStore).not.toHaveBeenCalled();
  });

  it('redirects to list when id is zero', () => {
    route.snapshot.paramMap.get = vi.fn().mockReturnValue('0');
    vi.mocked(router.navigate).mockClear();
    vi.mocked(salesApiService.getStore).mockClear();
    fixture = TestBed.createComponent(StoreEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(router.navigate).toHaveBeenCalledWith(['/sales/stores']);
    expect(salesApiService.getStore).not.toHaveBeenCalled();
  });

  it('redirects to list when id is negative', () => {
    route.snapshot.paramMap.get = vi.fn().mockReturnValue('-1');
    vi.mocked(router.navigate).mockClear();
    vi.mocked(salesApiService.getStore).mockClear();
    fixture = TestBed.createComponent(StoreEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(router.navigate).toHaveBeenCalledWith(['/sales/stores']);
    expect(salesApiService.getStore).not.toHaveBeenCalled();
  });

  it('loads store and sales persons in parallel and populates form', () => {
    expect(salesApiService.getStore).toHaveBeenCalledWith(292);
    expect(salesApiService.getSalesPersons).toHaveBeenCalledWith({ pageNumber: 1, pageSize: 25 });

    expect(component['storeId']).toBe(292);
    expect(component['form'].value.name).toBe('Next-Door Bike Store');
    expect(component['form'].value.salesPersonId).toBe('279');
    expect(component['salesPersons']()).toHaveLength(3);
    expect(component['isLoading']()).toBe(false);
  });

  it('shows error toast when load fails', () => {
    vi.spyOn(salesApiService, 'getStore').mockReturnValue(throwError(() => new Error('Server error')));
    vi.spyOn(notificationService, 'error');

    fixture = TestBed.createComponent(StoreEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to load store. Please try again.');
    expect(component['isLoading']()).toBe(false);
  });

  it('valid update calls updateStore and navigates to detail', () => {
    component['form'].setValue({ name: 'Updated Store Name', salesPersonId: '279' });
    component['onSubmit']();

    expect(salesApiService.updateStore).toHaveBeenCalledWith(292, {
      id: 292,
      name: 'Updated Store Name',
      salesPersonId: 279,
    });
    expect(notificationService['notifications']()).toEqual(
      expect.arrayContaining([expect.objectContaining({ type: 'success', message: 'Store updated successfully.' })]),
    );
    expect(router.navigate).toHaveBeenCalledWith(['/sales/stores', 292]);
  });

  it('update with null salesPersonId sends null to API', () => {
    component['form'].setValue({ name: 'Store Without SP', salesPersonId: null });
    component['onSubmit']();

    expect(salesApiService.updateStore).toHaveBeenCalledWith(292, {
      id: 292,
      name: 'Store Without SP',
      salesPersonId: null,
    });
  });

  it('API update error shows error toast and does not navigate', () => {
    vi.spyOn(salesApiService, 'updateStore').mockReturnValue(throwError(() => new Error('Server error')));
    vi.spyOn(notificationService, 'error');

    component['form'].setValue({ name: 'Updated Store Name', salesPersonId: null });
    component['onSubmit']();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to update store. Please try again.');
    expect(router.navigate).not.toHaveBeenCalled();
    expect(component['isSaving']()).toBe(false);
  });

  it('does not call updateStore when form is invalid', () => {
    component['form'].controls.name.setValue('');
    component['onSubmit']();

    expect(salesApiService.updateStore).not.toHaveBeenCalled();
    expect(component['submitted']()).toBe(true);
  });

  it('whitespace-only name is trimmed to empty and triggers required error', () => {
    component['form'].controls.name.setValue('   ');
    component['onSubmit']();

    expect(component['form'].controls.name.value).toBe('');
    const errors = component['nameErrors']();
    expect(errors).not.toBeNull();
    expect((errors as Record<string, string>)['required']).toBe('Name is required.');
    expect(salesApiService.updateStore).not.toHaveBeenCalled();
  });

  it('leading and trailing whitespace is trimmed before submission', () => {
    component['form'].setValue({ name: '  Updated Store  ', salesPersonId: null });
    component['onSubmit']();

    expect(salesApiService.updateStore).toHaveBeenCalledWith(292, expect.objectContaining({ name: 'Updated Store' }));
  });

  it('second call while saving is blocked (double-submit guard)', () => {
    component['form'].setValue({ name: 'Test Store', salesPersonId: null });
    component['onSubmit']();
    component['onSubmit'](); // second call should be blocked
    expect(salesApiService.updateStore).toHaveBeenCalledTimes(1);
  });

  it('backQueryParams preserves search and page from query params', () => {
    route.snapshot.queryParams = {
      search: 'Bike',
      pageNumber: '2',
      orderBy: 'name',
      sortOrder: 'desc',
    };

    fixture = TestBed.createComponent(StoreEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component['backQueryParams']()).toEqual({
      search: 'Bike',
      pageNumber: '2',
      orderBy: 'name',
      sortOrder: 'desc',
    });
  });

  it('backQueryParams returns empty object with no params', () => {
    expect(component['backQueryParams']()).toEqual({});
  });

  it('salesPersonOptions maps loaded sales persons to value/label pairs', () => {
    const options = component['salesPersonOptions']();
    expect(options).toHaveLength(3);
    expect(options[2]).toEqual({ value: 279, label: 'Tsvi Reiter' });
  });

  it('nameErrors returns null when name is valid after submit', () => {
    component['submitted'].set(true);
    expect(component['nameErrors']()).toBeNull();
  });

  it('nameErrors returns required error when name cleared and submitted', () => {
    component['form'].controls.name.setValue('');
    component['submitted'].set(true);
    fixture.detectChanges();

    const errors = component['nameErrors']();
    expect(errors).not.toBeNull();
    expect((errors as Record<string, string>)['required']).toBe('Name is required.');
  });
});
