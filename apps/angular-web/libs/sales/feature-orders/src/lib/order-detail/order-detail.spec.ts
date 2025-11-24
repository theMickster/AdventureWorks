import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { HttpErrorResponse } from '@angular/common/http';
import { of, Subject, throwError } from 'rxjs';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { SalesOrderDetail } from '@adventureworks-web/sales/data-access';
import { OrderDetailComponent } from './order-detail';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

// Sales Order 43659 — well-known AdventureWorks sample order with a sales person and 3 line items
const mockOrder: SalesOrderDetail = {
  salesOrderId: 43659,
  salesOrderNumber: 'SO43659',
  orderDate: '2011-05-31T00:00:00',
  dueDate: '2011-06-12T00:00:00',
  shipDate: '2011-06-07T00:00:00',
  status: 5,
  statusDescription: 'Shipped',
  subTotal: 20565.6206,
  taxAmt: 1971.5149,
  freight: 616.0984,
  totalDue: 23153.2339,
  customerName: 'Jon Yang',
  salesPersonId: 279,
  salesPersonName: 'Linda Mitchell',
  billToAddress: {
    addressLine1: '3761 N. 14th St',
    addressLine2: null,
    city: 'Rockhampton',
    stateProvince: 'Queensland',
    postalCode: '4700',
  },
  shipToAddress: {
    addressLine1: '3761 N. 14th St',
    addressLine2: null,
    city: 'Rockhampton',
    stateProvince: 'Queensland',
    postalCode: '4700',
  },
  lineItems: [
    {
      salesOrderDetailId: 1,
      productName: 'Mountain-100 Silver, 38',
      orderQty: 1,
      unitPrice: 2024.994,
      unitPriceDiscount: 0.0,
      lineTotal: 2024.994,
    },
    {
      salesOrderDetailId: 2,
      productName: 'Sport-100 Helmet, Black',
      orderQty: 2,
      unitPrice: 34.99,
      unitPriceDiscount: 0.02,
      lineTotal: 68.5804,
    },
    {
      salesOrderDetailId: 3,
      productName: 'LL Road Frame - Black, 58',
      orderQty: 1,
      unitPrice: 337.22,
      unitPriceDiscount: 0.0,
      lineTotal: 337.22,
    },
  ],
};

function buildRoute(id = '43659', queryParams: Record<string, string> = {}) {
  return {
    snapshot: {
      paramMap: { get: vi.fn().mockReturnValue(id) },
      queryParams,
    },
  };
}

describe('OrderDetailComponent', () => {
  let component: OrderDetailComponent;
  let fixture: ComponentFixture<OrderDetailComponent>;
  let salesApiService: SalesApiService;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [OrderDetailComponent],
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

    vi.spyOn(salesApiService, 'getSalesOrder').mockReturnValue(of(mockOrder));
    vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(OrderDetailComponent);
    component = fixture.componentInstance;
  });

  it('renders order number from API response', () => {
    fixture.detectChanges();

    const el = fixture.nativeElement.querySelector('#aw-order-detail-number') as HTMLElement;
    expect(el.textContent).toContain('SO43659');
  });

  it('shows loading skeleton before API resolves', () => {
    const subject = new Subject<SalesOrderDetail>();
    vi.spyOn(salesApiService, 'getSalesOrder').mockReturnValue(subject.asObservable());

    fixture.detectChanges();

    expect(component['isLoading']()).toBe(true);
    const skeleton = fixture.nativeElement.querySelector('#aw-order-detail-loading') as HTMLElement;
    expect(skeleton).toBeTruthy();

    subject.next(mockOrder);
    subject.complete();
    fixture.detectChanges();

    expect(component['isLoading']()).toBe(false);
  });

  it('renders correct status badge class for Shipped status', () => {
    fixture.detectChanges();

    // statusDescription is 'Shipped'; lowercased key maps to badge-success in STATUS_BADGE_MAP
    const badge = fixture.nativeElement.querySelector('#aw-order-detail-status') as HTMLElement;
    expect(badge).toBeTruthy();
    expect(badge.classList.contains('badge-success')).toBe(true);
  });

  it('renders financial summary values', () => {
    fixture.detectChanges();

    const subtotal = fixture.nativeElement.querySelector('#aw-order-detail-subtotal') as HTMLElement;
    const tax = fixture.nativeElement.querySelector('#aw-order-detail-tax') as HTMLElement;
    const freight = fixture.nativeElement.querySelector('#aw-order-detail-freight') as HTMLElement;
    const totalDue = fixture.nativeElement.querySelector('#aw-order-detail-total-due') as HTMLElement;

    expect(subtotal.textContent).toContain('20,565.62');
    expect(tax.textContent).toContain('1,971.51');
    expect(freight.textContent).toContain('616.10');
    expect(totalDue.textContent).toContain('23,153.23');
  });

  it('renders both address cards with city names visible', () => {
    fixture.detectChanges();

    const billToCity = fixture.nativeElement.querySelector('#aw-order-detail-billto-city') as HTMLElement;
    const shipToCity = fixture.nativeElement.querySelector('#aw-order-detail-shipto-city') as HTMLElement;

    expect(billToCity.textContent).toContain('Rockhampton');
    expect(shipToCity.textContent).toContain('Rockhampton');
  });

  it('renders line items table with correct row count', () => {
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('#aw-order-detail-line-items tbody tr') as NodeListOf<HTMLElement>;
    expect(rows.length).toBe(3);
  });

  it('renders salesperson name as a link when salesPersonName is non-null', () => {
    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('#aw-order-detail-salesperson-link') as HTMLAnchorElement;
    expect(link).toBeTruthy();
    expect(link.textContent?.trim()).toBe('Linda Mitchell');
  });

  it('shows dash for salesperson when salesPersonName is null', () => {
    vi.spyOn(salesApiService, 'getSalesOrder').mockReturnValue(
      of({ ...mockOrder, salesPersonId: null, salesPersonName: null }),
    );
    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('#aw-order-detail-salesperson-link');
    expect(link).toBeNull();

    const none = fixture.nativeElement.querySelector('#aw-order-detail-salesperson-none') as HTMLElement;
    expect(none.textContent?.trim()).toBe('—');
  });

  it('salesperson link routes to /sales/persons/:salesPersonId', () => {
    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('#aw-order-detail-salesperson-link') as HTMLAnchorElement;
    expect(link.getAttribute('href')).toBe('/sales/persons/279');
  });

  it('shows not-found state on 404 response from API', () => {
    vi.spyOn(salesApiService, 'getSalesOrder').mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 404 })),
    );
    fixture.detectChanges();

    expect(component['notFound']()).toBe(true);
    const notFoundEl = fixture.nativeElement.querySelector('#aw-order-detail-not-found') as HTMLElement;
    expect(notFoundEl).toBeTruthy();
  });
});
