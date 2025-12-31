import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { HttpErrorResponse } from '@angular/common/http';
import { of, Subject, throwError } from 'rxjs';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { CustomerDetail } from '@adventureworks-web/sales/data-access';
import { CustomerDetailComponent } from './customer-detail';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

// Real AdventureWorks store customer (CustomerID 29486, Store "Riders Company").
const mockStoreCustomer: CustomerDetail = {
  customerId: 29486,
  displayName: 'Riders Company',
  ltvRank: 3,
  totalCustomerCount: 19820,
  totalSpend: 158726.45,
  orderCount: 12,
  avgOrderValue: 13227.2,
  lastOrderDate: '2014-05-01T00:00:00',
  isInactive: false,
  customerType: 'Store',
  storeId: 296,
  storeName: 'Riders Company',
  firstName: null,
  lastName: null,
};

// Real AdventureWorks individual customer (CustomerID 11091, Dalton Perez).
const mockIndividualCustomer: CustomerDetail = {
  customerId: 11091,
  displayName: 'Dalton Perez',
  ltvRank: 100,
  totalCustomerCount: 19820,
  totalSpend: 8000,
  orderCount: 28,
  avgOrderValue: 285.71,
  lastOrderDate: '2013-11-01T00:00:00',
  isInactive: false,
  customerType: 'Individual',
  storeId: null,
  storeName: null,
  firstName: 'Dalton',
  lastName: 'Perez',
};

// Synthetic zero-order customer fixture — not tied to a specific real AdventureWorks row.
const mockZeroOrderCustomer: CustomerDetail = {
  customerId: 999999,
  displayName: 'No Orders',
  ltvRank: 19820,
  totalCustomerCount: 19820,
  totalSpend: 0,
  orderCount: 0,
  avgOrderValue: 0,
  lastOrderDate: null,
  isInactive: true,
  customerType: 'Individual',
  storeId: null,
  storeName: null,
  firstName: 'No',
  lastName: 'Orders',
};

function buildRoute(id = '29486') {
  return {
    snapshot: {
      paramMap: { get: vi.fn().mockReturnValue(id) },
    },
  };
}

describe('CustomerDetailComponent', () => {
  let component: CustomerDetailComponent;
  let fixture: ComponentFixture<CustomerDetailComponent>;
  let salesApiService: SalesApiService;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  async function setup(id = '29486') {
    route = buildRoute(id);

    await TestBed.configureTestingModule({
      imports: [CustomerDetailComponent],
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
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(CustomerDetailComponent);
    component = fixture.componentInstance;
  }

  it('renders the store name as a routerLink to /sales/stores/:storeId', async () => {
    await setup();
    vi.spyOn(salesApiService, 'getCustomerDetail').mockReturnValue(of(mockStoreCustomer));

    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('#aw-customer-detail-name-link') as HTMLAnchorElement;
    expect(link).toBeTruthy();
    expect(link.textContent).toContain('Riders Company');
    expect(link.getAttribute('href')).toBe('/sales/stores/296');
  });

  it('renders the individual customer name as plain text, not a link', async () => {
    await setup('11091');
    vi.spyOn(salesApiService, 'getCustomerDetail').mockReturnValue(of(mockIndividualCustomer));

    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('#aw-customer-detail-name-link');
    const name = fixture.nativeElement.querySelector('#aw-customer-detail-name') as HTMLElement;
    expect(link).toBeNull();
    expect(name).toBeTruthy();
    expect(name.textContent).toContain('Dalton Perez');
  });

  it('renders the LTV rank and total customer count', async () => {
    await setup();
    vi.spyOn(salesApiService, 'getCustomerDetail').mockReturnValue(of(mockStoreCustomer));

    fixture.detectChanges();

    const rank = fixture.nativeElement.querySelector('#aw-customer-detail-rank') as HTMLElement;
    expect(rank.textContent).toContain('#3');
    expect(rank.textContent).toContain('19,820');
  });

  it('shows the inactive badge when isInactive is true', async () => {
    await setup('999999');
    vi.spyOn(salesApiService, 'getCustomerDetail').mockReturnValue(of(mockZeroOrderCustomer));

    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('#aw-customer-detail-inactive-badge') as HTMLElement;
    expect(badge).toBeTruthy();
    expect(badge.classList.contains('badge-secondary')).toBe(true);
  });

  it('does not show the inactive badge when isInactive is false', async () => {
    await setup();
    vi.spyOn(salesApiService, 'getCustomerDetail').mockReturnValue(of(mockStoreCustomer));

    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('#aw-customer-detail-inactive-badge');
    expect(badge).toBeNull();
  });

  it('renders the four metric tiles', async () => {
    await setup();
    vi.spyOn(salesApiService, 'getCustomerDetail').mockReturnValue(of(mockStoreCustomer));

    fixture.detectChanges();

    const totalSpend = fixture.nativeElement.querySelector('#aw-customer-detail-total-spend') as HTMLElement;
    const orderCount = fixture.nativeElement.querySelector('#aw-customer-detail-order-count') as HTMLElement;
    const avgOrderValue = fixture.nativeElement.querySelector('#aw-customer-detail-avg-order-value') as HTMLElement;
    const lastOrderDate = fixture.nativeElement.querySelector('#aw-customer-detail-last-order-date') as HTMLElement;

    expect(totalSpend.textContent).toContain('158,726.45');
    expect(orderCount.textContent).toContain('12');
    expect(avgOrderValue.textContent).toContain('13,227.20');
    expect(lastOrderDate.textContent).toContain('2014');
  });

  it('renders $0.00 total spend and a "No orders found" empty state for a zero-order customer', async () => {
    await setup('999999');
    vi.spyOn(salesApiService, 'getCustomerDetail').mockReturnValue(of(mockZeroOrderCustomer));

    fixture.detectChanges();

    const totalSpend = fixture.nativeElement.querySelector('#aw-customer-detail-total-spend') as HTMLElement;
    const avgOrderValue = fixture.nativeElement.querySelector('#aw-customer-detail-avg-order-value') as HTMLElement;
    const emptyState = fixture.nativeElement.querySelector('#aw-customer-detail-no-orders') as HTMLElement;

    expect(totalSpend.textContent).toContain('$0.00');
    expect(avgOrderValue.textContent).toContain('$0.00');
    expect(emptyState).toBeTruthy();
  });

  it('shows loading skeleton before the API resolves', async () => {
    await setup();
    const subject = new Subject<CustomerDetail>();
    vi.spyOn(salesApiService, 'getCustomerDetail').mockReturnValue(subject.asObservable());

    fixture.detectChanges();

    expect(component['isLoading']()).toBe(true);
    const skeleton = fixture.nativeElement.querySelector('#aw-customer-detail-loading') as HTMLElement;
    expect(skeleton).toBeTruthy();

    subject.next(mockStoreCustomer);
    subject.complete();
    fixture.detectChanges();

    expect(component['isLoading']()).toBe(false);
  });

  it('shows not-found state on 404 response from the API', async () => {
    await setup();
    vi.spyOn(salesApiService, 'getCustomerDetail').mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 404 })),
    );

    fixture.detectChanges();

    expect(component['notFound']()).toBe(true);
    const notFoundEl = fixture.nativeElement.querySelector('#aw-customer-detail-not-found') as HTMLElement;
    expect(notFoundEl).toBeTruthy();
  });

  it('shows a generic error state on a non-404 API error', async () => {
    await setup();
    vi.spyOn(salesApiService, 'getCustomerDetail').mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 })),
    );

    fixture.detectChanges();

    expect(component['hasError']()).toBe(true);
    const errorEl = fixture.nativeElement.querySelector('#aw-customer-detail-error') as HTMLElement;
    expect(errorEl).toBeTruthy();
  });

  it('redirects to /sales/customers when the route id is invalid', async () => {
    await setup('not-a-number');
    vi.spyOn(salesApiService, 'getCustomerDetail').mockReturnValue(of(mockStoreCustomer));

    fixture.detectChanges();

    expect(router.navigate).toHaveBeenCalledWith(['/sales/customers']);
  });
});
