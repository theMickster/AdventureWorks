import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { patchState } from '@ngrx/signals';
import { setAllEntities } from '@ngrx/signals/entities';
import { unprotected } from '@ngrx/signals/testing';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { setError } from '@adventureworks-web/shared/data-access';
import { CustomerStore } from '@adventureworks-web/sales/data-access';
import type { CustomerListItem } from '@adventureworks-web/sales/data-access';
import { CustomerListComponent } from './customer-list';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockCustomer: CustomerListItem = {
  customerId: 11000,
  displayName: 'Jon Yang',
  customerType: 'Individual',
  storeId: null,
  ltvRank: 1,
  totalSpend: 8249,
  orderCount: 3,
  isInactive: false,
};

function buildRoute(queryParams: Record<string, string> = {}) {
  return {
    snapshot: { queryParams },
    queryParams: new BehaviorSubject<Record<string, string>>(queryParams),
  };
}

describe('CustomerListComponent', () => {
  let component: CustomerListComponent;
  let fixture: ComponentFixture<CustomerListComponent>;
  let customerStore: InstanceType<typeof CustomerStore>;
  let notificationService: NotificationService;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [CustomerListComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    customerStore = TestBed.inject(CustomerStore);
    notificationService = TestBed.inject(NotificationService);
    router = TestBed.inject(Router);

    vi.spyOn(customerStore, 'loadPage');
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(CustomerListComponent);
    component = fixture.componentInstance;
  });

  it('renders without errors', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('initializes with default page load on no URL params', () => {
    fixture.detectChanges();

    expect(customerStore.loadPage).toHaveBeenCalledWith({ pageNumber: 1, pageSize: 25 });
  });

  it('initializes with search from URL params', () => {
    route.snapshot.queryParams = { search: 'Bike', pageNumber: '2' };
    route.queryParams.next({ search: 'Bike', pageNumber: '2' });

    fixture.detectChanges();

    expect(customerStore.loadPage).toHaveBeenCalledWith({ pageNumber: 2, pageSize: 25, search: 'Bike' });
  });

  it('onSearch updates the URL with the search term and resets to page 1', () => {
    fixture.detectChanges();

    component['onSearch']('Yang');

    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: { search: 'Yang', pageNumber: 1 } }),
    );
  });

  it('onSearch with empty string clears the search URL param', () => {
    fixture.detectChanges();

    component['onSearch']('');

    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: { search: null, pageNumber: 1 } }),
    );
  });

  it('onClearSearch resets to page 1 and clears URL param', () => {
    fixture.detectChanges();

    component['onClearSearch']();

    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: { search: null, pageNumber: null } }),
    );
  });

  it('onPageChange updates the URL page param', () => {
    fixture.detectChanges();

    component['onPageChange'](3);

    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: { pageNumber: 3 } }),
    );
  });

  it('rows computed correctly from store entities, including derived statusKey', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(customerStore), setAllEntities([mockCustomer], { selectId: (c) => c.customerId }));
    fixture.detectChanges();

    const rows = component['rows']();
    expect(rows).toHaveLength(1);
    expect(rows[0]).toMatchObject({
      customerId: 11000,
      ltvRank: 1,
      displayName: 'Jon Yang',
      customerType: 'Individual',
      totalSpend: 8249,
      orderCount: 3,
      statusKey: 'active',
    });
  });

  it('rows derives inactive statusKey for inactive customers', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(
      unprotected(customerStore),
      setAllEntities([{ ...mockCustomer, isInactive: true }], { selectId: (c) => c.customerId }),
    );
    fixture.detectChanges();

    const rows = component['rows']();
    expect(rows[0]['statusKey']).toBe('inactive');
  });

  it('shows error toast when the store has an error, without throwing', async () => {
    vi.spyOn(notificationService, 'error');
    fixture.detectChanges();
    await fixture.whenStable();

    expect(() => {
      patchState(unprotected(customerStore), setError('load failed'));
    }).not.toThrow();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to load customers. Please try again.');
  });

  it('shows the empty state and issues no extraneous store calls when results are empty', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    const callsBefore = (customerStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.length;
    patchState(unprotected(customerStore), setAllEntities([] as CustomerListItem[], { selectId: (c) => c.customerId }));
    fixture.detectChanges();

    expect(component['rows']()).toEqual([]);
    expect((customerStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.length).toBe(callsBefore);
  });

  it('onRowClick navigates to the customer detail route', () => {
    fixture.detectChanges();

    component['onRowClick']({ customerId: 11000 });

    expect(router.navigate).toHaveBeenCalledWith(['/sales/customers', 11000]);
  });
});
