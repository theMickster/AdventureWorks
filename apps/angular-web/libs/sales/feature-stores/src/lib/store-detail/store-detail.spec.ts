import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { of, Subject, throwError } from 'rxjs';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { Store } from '@adventureworks-web/sales/data-access';
import { StoreDetailComponent } from './store-detail';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

// Store 292 — Next-Door Bike Store (derived from Postman collection evidence)
const mockStore: Store = {
  id: 292,
  name: 'Next-Door Bike Store',
  modifiedDate: '2026-01-01T00:00:00',
  storeAddresses: [
    {
      id: 985,
      storeId: 292,
      addressTypeId: 3,
      addressTypeName: 'Main Office',
      addressLine1: '4900 Mt. Wilson Way',
      addressLine2: null,
      city: 'Mentor',
      stateProvinceCode: 'OH',
      stateProvinceName: 'Ohio',
      countryRegionCode: 'US',
      countryRegionName: 'United States',
      postalCode: '44060',
      modifiedDate: '2026-01-01T00:00:00',
    },
  ],
  storeContacts: [
    {
      id: 292,
      storeId: 292,
      title: null,
      firstName: 'Gustavo',
      middleName: null,
      lastName: 'Achong',
      suffix: null,
      contactTypeId: 11,
      contactTypeName: 'Owner',
    },
  ],
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

describe('StoreDetailComponent', () => {
  let component: StoreDetailComponent;
  let fixture: ComponentFixture<StoreDetailComponent>;
  let salesApiService: SalesApiService;
  let notificationService: NotificationService;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [StoreDetailComponent],
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
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(StoreDetailComponent);
    component = fixture.componentInstance;
  });

  it('renders without errors', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('shows loading skeleton during fetch', () => {
    const subject = new Subject<Store>();
    vi.spyOn(salesApiService, 'getStore').mockReturnValue(subject.asObservable());

    fixture.detectChanges();

    expect(component['isLoading']()).toBe(true);

    subject.next(mockStore);
    subject.complete();
    fixture.detectChanges();

    expect(component['isLoading']()).toBe(false);
  });

  it('renders store name and sales person after load', () => {
    fixture.detectChanges();

    expect(component['store']()).toEqual(mockStore);
    expect(component['salesPersonName']()).toBe('Tsvi Reiter');
  });

  it('shows dash for null salesPerson', () => {
    vi.spyOn(salesApiService, 'getStore').mockReturnValue(of({ ...mockStore, salesPerson: null }));
    fixture.detectChanges();

    expect(component['salesPersonName']()).toBe('—');
  });

  it('defaults to Addresses tab', () => {
    fixture.detectChanges();

    expect(component['activeTab']()).toBe('addresses');
  });

  it('onTabChange switches to contacts tab', () => {
    fixture.detectChanges();

    component['onTabChange']('contacts');

    expect(component['activeTab']()).toBe('contacts');
  });

  it('renders address cards when addresses present', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const addresses = component['store']()?.storeAddresses ?? [];
    expect(addresses).toHaveLength(1);
    expect(addresses[0].city).toBe('Mentor');
    expect(addresses[0].addressTypeName).toBe('Main Office');
  });

  it('shows empty state when no addresses', async () => {
    vi.spyOn(salesApiService, 'getStore').mockReturnValue(of({ ...mockStore, storeAddresses: [] }));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(component['store']()?.storeAddresses).toHaveLength(0);
    expect(component['activeTab']()).toBe('addresses');
  });

  it('renders contact cards when contacts present', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const contacts = component['store']()?.storeContacts ?? [];
    expect(contacts).toHaveLength(1);
    expect(contacts[0].firstName).toBe('Gustavo');
    expect(contacts[0].contactTypeName).toBe('Owner');
  });

  it('shows empty state when no contacts', async () => {
    vi.spyOn(salesApiService, 'getStore').mockReturnValue(of({ ...mockStore, storeContacts: [] }));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    component['onTabChange']('contacts');
    fixture.detectChanges();

    expect(component['store']()?.storeContacts).toHaveLength(0);
    expect(component['activeTab']()).toBe('contacts');
  });

  it('shows error toast when API call fails', () => {
    vi.spyOn(notificationService, 'error');
    vi.spyOn(salesApiService, 'getStore').mockReturnValue(throwError(() => new Error('Network error')));

    fixture.detectChanges();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to load store. Please try again.');
    expect(component['isLoading']()).toBe(false);

    const errorState = fixture.nativeElement.querySelector('#aw-store-detail-error');
    expect(errorState).toBeTruthy();
  });

  it('redirects to list when id is invalid (NaN)', () => {
    route.snapshot.paramMap.get = vi.fn().mockReturnValue('abc');

    fixture.detectChanges();

    expect(router.navigate).toHaveBeenCalledWith(['/sales/stores']);
    expect(salesApiService.getStore).not.toHaveBeenCalled();
  });

  it('redirects to list when id is zero', () => {
    route.snapshot.paramMap.get = vi.fn().mockReturnValue('0');

    fixture.detectChanges();

    expect(router.navigate).toHaveBeenCalledWith(['/sales/stores']);
    expect(salesApiService.getStore).not.toHaveBeenCalled();
  });

  it('backQueryParams preserves search and page from query params', () => {
    route.snapshot.queryParams = {
      search: 'Bike',
      pageNumber: '2',
      orderBy: 'name',
      sortOrder: 'desc',
    };

    fixture.detectChanges();

    expect(component['backQueryParams']()).toEqual({
      search: 'Bike',
      pageNumber: '2',
      orderBy: 'name',
      sortOrder: 'desc',
    });
  });

  it('backQueryParams returns empty object with no params', () => {
    fixture.detectChanges();

    expect(component['backQueryParams']()).toEqual({});
  });
});
