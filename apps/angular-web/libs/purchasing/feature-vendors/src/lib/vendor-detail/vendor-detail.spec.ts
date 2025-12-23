import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, of, Subject, throwError } from 'rxjs';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { PurchasingApiService } from '@adventureworks-web/purchasing/data-access';
import type { PurchaseOrderSummary, VendorDetail } from '@adventureworks-web/purchasing/data-access';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import { VendorDetailComponent } from './vendor-detail';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

// Real AdventureWorks vendor row (BusinessEntityID 1496 — "Advanced Bicycles").
const mockVendor: VendorDetail = {
  vendorId: 1496,
  name: 'Advanced Bicycles',
  accountNumber: 'ADVANCED0001',
  creditRatingLabel: 'Superior',
  preferredVendorStatus: true,
  activeFlag: true,
  totalSpend: 762.94,
  poCount: 2,
  avgPoValue: 381.47,
};

const mockPoResult: SearchResult<PurchaseOrderSummary> = {
  pageNumber: 1,
  pageSize: 25,
  totalPages: 1,
  hasPreviousPage: false,
  hasNextPage: false,
  totalRecords: 1,
  results: [
    {
      purchaseOrderId: 3932,
      orderDate: '2014-07-30T00:00:00',
      dueDate: '2014-08-13T00:00:00',
      status: 1,
      statusLabel: 'Pending',
      totalDue: 302.44,
    },
  ],
};

let queryParamsSub: BehaviorSubject<Record<string, string>>;

function buildRoute(id = '1496', queryParams: Record<string, string> = {}) {
  queryParamsSub = new BehaviorSubject<Record<string, string>>(queryParams);
  return {
    snapshot: {
      paramMap: { get: vi.fn().mockReturnValue(id) },
    },
    queryParams: queryParamsSub,
  };
}

describe('VendorDetailComponent', () => {
  let component: VendorDetailComponent;
  let fixture: ComponentFixture<VendorDetailComponent>;
  let purchasingApiService: PurchasingApiService;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [VendorDetailComponent],
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

    vi.spyOn(purchasingApiService, 'getVendorDetail').mockReturnValue(of(mockVendor));
    vi.spyOn(purchasingApiService, 'getVendorPurchaseOrders').mockReturnValue(of(mockPoResult));
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(VendorDetailComponent);
    component = fixture.componentInstance;
  });

  it('renders vendor name and account number from API response', () => {
    fixture.detectChanges();

    const name = fixture.nativeElement.querySelector('#aw-vendor-detail-name') as HTMLElement;
    const account = fixture.nativeElement.querySelector('#aw-vendor-detail-account-number') as HTMLElement;
    expect(name.textContent).toContain('Advanced Bicycles');
    expect(account.textContent).toContain('ADVANCED0001');
  });

  it('shows loading skeleton before the vendor API resolves', () => {
    const subject = new Subject<VendorDetail>();
    vi.spyOn(purchasingApiService, 'getVendorDetail').mockReturnValue(subject.asObservable());

    fixture.detectChanges();

    expect(component['isLoadingVendor']()).toBe(true);
    const skeleton = fixture.nativeElement.querySelector('#aw-vendor-detail-loading') as HTMLElement;
    expect(skeleton).toBeTruthy();

    subject.next(mockVendor);
    subject.complete();
    fixture.detectChanges();

    expect(component['isLoadingVendor']()).toBe(false);
  });

  it('renders the three spend metric tiles', () => {
    fixture.detectChanges();

    const totalSpend = fixture.nativeElement.querySelector('#aw-vendor-detail-total-spend') as HTMLElement;
    const poCount = fixture.nativeElement.querySelector('#aw-vendor-detail-po-count') as HTMLElement;
    const avgPoValue = fixture.nativeElement.querySelector('#aw-vendor-detail-avg-po-value') as HTMLElement;

    expect(totalSpend.textContent).toContain('762.94');
    expect(poCount.textContent?.trim()).toBe('2');
    expect(avgPoValue.textContent).toContain('381.47');
  });

  it('renders preferred and active badges', () => {
    fixture.detectChanges();

    const preferred = fixture.nativeElement.querySelector('#aw-vendor-detail-preferred-badge') as HTMLElement;
    const active = fixture.nativeElement.querySelector('#aw-vendor-detail-active-badge') as HTMLElement;
    expect(preferred.classList.contains('badge-success')).toBe(true);
    expect(active.classList.contains('badge-success')).toBe(true);
  });

  it('shows not-found state on 404 response from the vendor API and never calls the PO-history API', () => {
    vi.spyOn(purchasingApiService, 'getVendorDetail').mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 404 })),
    );

    fixture.detectChanges();

    expect(component['vendorNotFound']()).toBe(true);
    const notFoundEl = fixture.nativeElement.querySelector('#aw-vendor-detail-not-found') as HTMLElement;
    expect(notFoundEl).toBeTruthy();
    expect(purchasingApiService.getVendorPurchaseOrders).not.toHaveBeenCalled();
  });

  it('shows a generic error state on a non-404 vendor API error', () => {
    vi.spyOn(purchasingApiService, 'getVendorDetail').mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 })),
    );

    fixture.detectChanges();

    expect(component['vendorHasError']()).toBe(true);
    const errorEl = fixture.nativeElement.querySelector('#aw-vendor-detail-error') as HTMLElement;
    expect(errorEl).toBeTruthy();
  });

  it('loads PO history only after the vendor resolves successfully', () => {
    fixture.detectChanges();

    expect(purchasingApiService.getVendorPurchaseOrders).toHaveBeenCalledWith(1496, { pageNumber: 1, pageSize: 25 });
  });

  it('a slower stale PO-history request never overwrites the table with results from an older query', () => {
    // Regression test: route.queryParams re-emitting before the previous PO-history request
    // resolves must cancel that stale request (via switchMap), not let it race the newer one.
    const slowFirstRequest = new Subject<SearchResult<PurchaseOrderSummary>>();
    const fastSecondResult: SearchResult<PurchaseOrderSummary> = {
      ...mockPoResult,
      results: [
        {
          purchaseOrderId: 4444,
          orderDate: '2014-09-01T00:00:00',
          dueDate: '2014-09-15T00:00:00',
          status: 4,
          statusLabel: 'Complete',
          totalDue: 999.99,
        },
      ],
    };

    vi.spyOn(purchasingApiService, 'getVendorPurchaseOrders')
      .mockReturnValueOnce(slowFirstRequest.asObservable())
      .mockReturnValueOnce(of(fastSecondResult));

    fixture.detectChanges();

    // Second query-param emission fires (and resolves) before the first request's Subject emits.
    queryParamsSub.next({ status: '4' });
    fixture.detectChanges();

    // Now the stale first request finally resolves — it must be ignored (switchMap unsubscribed it).
    slowFirstRequest.next(mockPoResult);
    slowFirstRequest.complete();
    fixture.detectChanges();

    expect(component['purchaseOrders']()).toEqual(fastSecondResult.results);
  });

  it('renders PO history rows', () => {
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('#aw-vendor-detail-po-table tbody tr') as NodeListOf<HTMLElement>;
    expect(rows.length).toBeGreaterThan(0);
  });

  it('shows the DataTableComponent empty state for a zero-PO vendor (e.g. "Cycling Master", id 1502)', () => {
    const zeroPoVendor: VendorDetail = {
      vendorId: 1502,
      name: 'Cycling Master',
      accountNumber: 'CYCLING0001',
      creditRatingLabel: 'Superior',
      preferredVendorStatus: false,
      activeFlag: true,
      totalSpend: 0,
      poCount: 0,
      avgPoValue: 0,
    };
    const emptyPoResult: SearchResult<PurchaseOrderSummary> = {
      pageNumber: 1,
      pageSize: 25,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
      totalRecords: 0,
      results: [],
    };

    vi.spyOn(purchasingApiService, 'getVendorDetail').mockReturnValue(of(zeroPoVendor));
    vi.spyOn(purchasingApiService, 'getVendorPurchaseOrders').mockReturnValue(of(emptyPoResult));

    fixture.detectChanges();

    // Zero-value metric tiles render without error (no divide-by-zero / NaN artifacts).
    const totalSpend = fixture.nativeElement.querySelector('#aw-vendor-detail-total-spend') as HTMLElement;
    const avgPoValue = fixture.nativeElement.querySelector('#aw-vendor-detail-avg-po-value') as HTMLElement;
    expect(totalSpend.textContent).toContain('0.00');
    expect(avgPoValue.textContent).toContain('0.00');

    const rows = fixture.nativeElement.querySelectorAll('#aw-vendor-detail-po-table tbody tr') as NodeListOf<HTMLElement>;
    expect(rows.length).toBe(0);
    const empty = fixture.nativeElement.querySelector('#aw-vendor-detail-po-table-empty');
    expect(empty).toBeTruthy();
  });

  it('shows the PO-history error state on a failed purchase-orders request', () => {
    vi.spyOn(purchasingApiService, 'getVendorPurchaseOrders').mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 })),
    );

    fixture.detectChanges();

    expect(component['purchaseOrdersHasError']()).toBe(true);
    expect(component['isLoadingPurchaseOrders']()).toBe(false);

    const errorEl = fixture.nativeElement.querySelector('#aw-vendor-detail-po-error') as HTMLElement;
    expect(errorEl).toBeTruthy();
    const table = fixture.nativeElement.querySelector('#aw-vendor-detail-po-table');
    expect(table).toBeFalsy();
  });

  it('changing the PO-history page navigates with the new pageNumber merged into the URL', () => {
    fixture.detectChanges();

    component['onPageChange'](2);

    expect(router.navigate).toHaveBeenCalledWith([], {
      relativeTo: route,
      queryParams: { pageNumber: 2 },
      queryParamsHandling: 'merge',
    });
  });

  it('renders the PO id cell as a routerLink to /purchasing/purchase-orders/:id', () => {
    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('#aw-vendor-detail-po-table a.link-primary') as HTMLAnchorElement;
    expect(link).toBeTruthy();
    expect(link.getAttribute('href')).toBe('/purchasing/purchase-orders/3932');
  });

  it('applying filters navigates with merged query params and pageNumber reset to 1', () => {
    fixture.detectChanges();

    component['filterForm'].setValue({ status: '4', startDate: '2014-01-01', endDate: '2014-12-31' });
    component['onApplyFilters']();

    expect(router.navigate).toHaveBeenCalledWith([], {
      relativeTo: route,
      queryParams: { status: 4, startDate: '2014-01-01', endDate: '2014-12-31', pageNumber: 1 },
      queryParamsHandling: 'merge',
    });
  });

  it('resetting filters nulls every filter param and pageNumber', () => {
    fixture.detectChanges();

    component['onResetFilters']();

    expect(router.navigate).toHaveBeenCalledWith([], {
      relativeTo: route,
      queryParams: { status: null, startDate: null, endDate: null, pageNumber: null },
      queryParamsHandling: 'merge',
    });
  });

  it('restores an out-of-range status URL param as "All statuses" rather than a raw junk value', async () => {
    route = buildRoute('1496', { status: '9' });
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      imports: [VendorDetailComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    vi.spyOn(TestBed.inject(PurchasingApiService), 'getVendorDetail').mockReturnValue(of(mockVendor));
    vi.spyOn(TestBed.inject(PurchasingApiService), 'getVendorPurchaseOrders').mockReturnValue(of(mockPoResult));

    fixture = TestBed.createComponent(VendorDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component['filterForm'].getRawValue().status).toBe('');
  });

  it('redirects to /purchasing/vendors when the route id is invalid', async () => {
    route = buildRoute('not-a-number');
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      imports: [VendorDetailComponent],
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

    fixture = TestBed.createComponent(VendorDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(newRouter.navigate).toHaveBeenCalledWith(['/purchasing/vendors']);
  });
});
