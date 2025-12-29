import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { HttpErrorResponse } from '@angular/common/http';
import { of, Subject, throwError } from 'rxjs';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { PurchasingApiService } from '@adventureworks-web/purchasing/data-access';
import type { PurchaseOrderDetail } from '@adventureworks-web/purchasing/data-access';
import { PurchaseOrderDetailComponent } from './purchase-order-detail';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

// Real AdventureWorks purchase order shape (PO 3932, vendor 1650 "American Bicycles and Wheels").
const mockPurchaseOrder: PurchaseOrderDetail = {
  purchaseOrderId: 3932,
  status: 1,
  statusLabel: 'Pending',
  orderDate: '2014-07-30T00:00:00',
  dueDate: '2014-08-13T00:00:00',
  shipDate: null,
  vendorId: 1650,
  vendorName: 'American Bicycles and Wheels',
  employeeId: 261,
  approvingEmployeeFullName: 'Reinout Hillmann',
  shipMethodId: 5,
  shipMethodName: 'CARGO TRANSPORT 5',
  // subTotal/totalDue are self-consistent with lineItems below (171.0765 + 20 = 191.0765) —
  // the component renders subTotal directly rather than summing lineItems client-side.
  subTotal: 191.0765,
  taxAmt: 13.6861,
  freight: 4.2769,
  totalDue: 209.0395,
  lineItems: [
    {
      purchaseOrderDetailId: 5,
      productId: 4,
      productName: 'Headset Ball Bearings',
      dueDate: '2014-08-13T00:00:00',
      orderQty: 3,
      unitPrice: 57.0255,
      lineTotal: 171.0765,
      receivedQty: 2,
      rejectedQty: 1,
      stockedQty: 1,
    },
    {
      purchaseOrderDetailId: 6,
      productId: 316,
      productName: 'Blade',
      dueDate: '2014-08-13T00:00:00',
      orderQty: 2,
      unitPrice: 10,
      lineTotal: 20,
      receivedQty: 2,
      rejectedQty: 0,
      stockedQty: 2,
    },
  ],
};

function buildRoute(id = '3932') {
  return {
    snapshot: {
      paramMap: { get: vi.fn().mockReturnValue(id) },
    },
  };
}

describe('PurchaseOrderDetailComponent', () => {
  let component: PurchaseOrderDetailComponent;
  let fixture: ComponentFixture<PurchaseOrderDetailComponent>;
  let purchasingApiService: PurchasingApiService;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [PurchaseOrderDetailComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    purchasingApiService = TestBed.inject(PurchasingApiService);
    router = TestBed.inject(Router);

    vi.spyOn(purchasingApiService, 'getPurchaseOrderDetail').mockReturnValue(of(mockPurchaseOrder));
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(PurchaseOrderDetailComponent);
    component = fixture.componentInstance;
  });

  it('renders the purchase order id and vendor name from the API response', () => {
    fixture.detectChanges();

    const title = fixture.nativeElement.querySelector('#aw-purchase-order-detail-title') as HTMLElement;
    const vendorLink = fixture.nativeElement.querySelector('#aw-purchase-order-detail-vendor-link') as HTMLElement;
    expect(title.textContent).toContain('PO3932');
    expect(vendorLink.textContent).toContain('American Bicycles and Wheels');
  });

  it('renders the vendor name as a routerLink to /purchasing/vendors/:vendorId', () => {
    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('#aw-purchase-order-detail-vendor-link') as HTMLAnchorElement;
    expect(link.getAttribute('href')).toBe('/purchasing/vendors/1650');
  });

  it('shows loading skeleton before the API resolves', () => {
    const subject = new Subject<PurchaseOrderDetail>();
    vi.spyOn(purchasingApiService, 'getPurchaseOrderDetail').mockReturnValue(subject.asObservable());

    fixture.detectChanges();

    expect(component['isLoading']()).toBe(true);
    const skeleton = fixture.nativeElement.querySelector('#aw-purchase-order-detail-loading') as HTMLElement;
    expect(skeleton).toBeTruthy();

    subject.next(mockPurchaseOrder);
    subject.complete();
    fixture.detectChanges();

    expect(component['isLoading']()).toBe(false);
  });

  it('renders the four metric tiles', () => {
    fixture.detectChanges();

    const subTotal = fixture.nativeElement.querySelector('#aw-purchase-order-detail-subtotal') as HTMLElement;
    const tax = fixture.nativeElement.querySelector('#aw-purchase-order-detail-tax') as HTMLElement;
    const freight = fixture.nativeElement.querySelector('#aw-purchase-order-detail-freight') as HTMLElement;
    const totalDue = fixture.nativeElement.querySelector('#aw-purchase-order-detail-total-due') as HTMLElement;

    expect(subTotal.textContent).toContain('191.08');
    expect(tax.textContent).toContain('13.69');
    expect(freight.textContent).toContain('4.28');
    expect(totalDue.textContent).toContain('209.04');
  });

  it('renders the status badge from statusLabel', () => {
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('#aw-purchase-order-detail-status-badge') as HTMLElement;
    expect(badge.textContent).toContain('pending');
    expect(badge.classList.contains('badge-warning')).toBe(true);
  });

  it('renders line item rows with product name and quantities', () => {
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll(
      '#aw-purchase-order-detail-line-items-table tbody tr',
    ) as NodeListOf<HTMLElement>;
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Headset Ball Bearings');
  });

  it('renders the totals row from the server-computed subTotal, not a client-side sum', () => {
    fixture.detectChanges();

    const total = fixture.nativeElement.querySelector('#aw-purchase-order-detail-line-items-total') as HTMLElement;
    expect(total.textContent).toContain('191.08');
  });

  it('shows not-found state on 404 response from the API', () => {
    vi.spyOn(purchasingApiService, 'getPurchaseOrderDetail').mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 404 })),
    );

    fixture.detectChanges();

    expect(component['notFound']()).toBe(true);
    const notFoundEl = fixture.nativeElement.querySelector('#aw-purchase-order-detail-not-found') as HTMLElement;
    expect(notFoundEl).toBeTruthy();
  });

  it('shows a generic error state on a non-404 API error', () => {
    vi.spyOn(purchasingApiService, 'getPurchaseOrderDetail').mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 })),
    );

    fixture.detectChanges();

    expect(component['hasError']()).toBe(true);
    const errorEl = fixture.nativeElement.querySelector('#aw-purchase-order-detail-error') as HTMLElement;
    expect(errorEl).toBeTruthy();
  });

  it('redirects to /purchasing/vendors when the route id is invalid', async () => {
    route = buildRoute('not-a-number');
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      imports: [PurchaseOrderDetailComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    const newRouter = TestBed.inject(Router);
    vi.spyOn(newRouter, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(PurchaseOrderDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(newRouter.navigate).toHaveBeenCalledWith(['/purchasing/vendors']);
  });
});
