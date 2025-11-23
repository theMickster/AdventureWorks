import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { patchState } from '@ngrx/signals';
import { setAllEntities } from '@ngrx/signals/entities';
import { unprotected } from '@ngrx/signals/testing';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { setError } from '@adventureworks-web/shared/data-access';
import { LookupApiService } from '@adventureworks-web/shared/data-access';
import { SalesApiService, SalesOrderStore } from '@adventureworks-web/sales/data-access';
import type { SalesOrder } from '@adventureworks-web/sales/data-access';
import { StatusBadgeComponent } from '@adventureworks-web/shared/ui';
import { OrderListComponent } from './order-list';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const selectId = (o: SalesOrder) => o.salesOrderId;

const mockOrder: SalesOrder = {
  salesOrderId: 43659,
  salesOrderNumber: 'SO43659',
  orderDate: '2011-05-31T00:00:00',
  status: 5,
  statusDescription: 'Shipped',
  totalDue: 23153.2339,
  customerName: 'A Bike Store',
  salesPersonName: 'Michael Blythe',
};

function buildRoute(queryParams: Record<string, string> = {}) {
  return {
    snapshot: { queryParams },
    queryParams: { subscribe: vi.fn() },
  };
}

describe('OrderListComponent', () => {
  let component: OrderListComponent;
  let fixture: ComponentFixture<OrderListComponent>;
  let salesOrderStore: InstanceType<typeof SalesOrderStore>;
  let salesApi: SalesApiService;
  let lookupApi: LookupApiService;
  let notificationService: NotificationService;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  const emptySearchResult = {
    pageNumber: 1,
    pageSize: 100,
    totalPages: 0,
    totalRecords: 0,
    hasPreviousPage: false,
    hasNextPage: false,
    results: [],
  };

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [OrderListComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    salesOrderStore = TestBed.inject(SalesOrderStore);
    salesApi = TestBed.inject(SalesApiService);
    lookupApi = TestBed.inject(LookupApiService);
    notificationService = TestBed.inject(NotificationService);
    router = TestBed.inject(Router);

    vi.spyOn(salesOrderStore, 'loadPage');
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    // Dropdown data must not block the grid; stub the lookups so forkJoin resolves.
    vi.spyOn(salesApi, 'getSalesPersons').mockReturnValue(of(emptySearchResult));
    vi.spyOn(lookupApi, 'getTerritories').mockReturnValue(of([]));

    fixture = TestBed.createComponent(OrderListComponent);
    component = fixture.componentInstance;
  });

  it('renders without errors', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('loads page 1 with default OrderDate desc sort when no URL params', () => {
    fixture.detectChanges();

    expect(salesOrderStore.loadPage).toHaveBeenCalledWith({
      pageNumber: 1,
      pageSize: 25,
      orderBy: 'orderDate',
      sortOrder: 'desc',
    });
  });

  it('restores filter params from the URL and forwards them to loadPage', () => {
    route.snapshot.queryParams = {
      orderDateFrom: '2013-01-01',
      orderDateTo: '2013-12-31',
      status: '5',
      salesPersonId: '279',
      territoryId: '4',
    };

    fixture.detectChanges();

    expect(salesOrderStore.loadPage).toHaveBeenCalledWith({
      pageNumber: 1,
      pageSize: 25,
      orderBy: 'orderDate',
      sortOrder: 'desc',
      orderDateFrom: '2013-01-01',
      orderDateTo: '2013-12-31',
      status: 5,
      salesPersonId: 279,
      territoryId: 4,
    });
  });

  it('clamps junk pageNumber from the URL to 1', () => {
    route.snapshot.queryParams = { pageNumber: '-3' };

    fixture.detectChanges();

    expect(salesOrderStore.loadPage).toHaveBeenCalledWith(
      expect.objectContaining({ pageNumber: 1 }),
    );
  });

  it('clamps zero pageNumber from the URL to 1', () => {
    route.snapshot.queryParams = { pageNumber: '0' };

    fixture.detectChanges();

    expect(salesOrderStore.loadPage).toHaveBeenCalledWith(
      expect.objectContaining({ pageNumber: 1 }),
    );
  });

  it('ignores an invalid orderBy from the URL and falls back to the default sort', () => {
    route.snapshot.queryParams = { orderBy: 'dropTable', sortOrder: 'asc' };

    fixture.detectChanges();

    expect(salesOrderStore.loadPage).toHaveBeenCalledWith(
      expect.objectContaining({ orderBy: 'orderDate', sortOrder: 'desc' }),
    );
    expect(salesOrderStore.loadPage).not.toHaveBeenCalledWith(
      expect.objectContaining({ orderBy: 'dropTable' }),
    );
  });

  it('restores a valid orderBy from the URL', () => {
    route.snapshot.queryParams = { orderBy: 'totalDue', sortOrder: 'asc' };

    fixture.detectChanges();

    expect(salesOrderStore.loadPage).toHaveBeenCalledWith(
      expect.objectContaining({ orderBy: 'totalDue', sortOrder: 'asc' }),
    );
    expect(component['sortColumn']()).toBe('totalDue');
    expect(component['sortDirection']()).toBe('asc');
  });

  it('applies a filter, resets to page 1, and writes back to the URL with merge', () => {
    fixture.detectChanges();
    component['filterForm'].patchValue({ status: '5' });

    component['onApplyFilters']();

    expect(salesOrderStore.loadPage).toHaveBeenLastCalledWith(
      expect.objectContaining({ pageNumber: 1, pageSize: 25, status: 5 }),
    );
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        queryParams: expect.objectContaining({ status: 5, pageNumber: 1 }),
        queryParamsHandling: 'merge',
      }),
    );
  });

  it('omits a cleared filter from the API call and nulls its URL param (not empty string or 0)', () => {
    fixture.detectChanges();

    // Set then clear the status filter; salesPersonId is also empty.
    component['filterForm'].patchValue({ status: '5' });
    component['filterForm'].patchValue({ status: '' });

    component['onApplyFilters']();

    const lastCall = (salesOrderStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.at(-1)?.[0] as Record<
      string,
      unknown
    >;
    // Cleared filters must be absent from the params object — not present as '' or 0.
    expect('status' in lastCall).toBe(false);
    expect('salesPersonId' in lastCall).toBe(false);
    expect('territoryId' in lastCall).toBe(false);
    expect('orderDateFrom' in lastCall).toBe(false);
    expect('orderDateTo' in lastCall).toBe(false);

    expect(router.navigate).toHaveBeenLastCalledWith(
      [],
      expect.objectContaining({
        queryParams: {
          orderDateFrom: null,
          orderDateTo: null,
          status: null,
          salesPersonId: null,
          territoryId: null,
          pageNumber: 1,
        },
        queryParamsHandling: 'merge',
      }),
    );
  });

  it('restores all five filters plus a valid page and sort from a bookmarked URL', () => {
    route.snapshot.queryParams = {
      orderDateFrom: '2013-01-01',
      orderDateTo: '2013-12-31',
      status: '2',
      salesPersonId: '283',
      territoryId: '6',
      pageNumber: '4',
      orderBy: 'totalDue',
      sortOrder: 'asc',
    };

    fixture.detectChanges();

    expect(salesOrderStore.loadPage).toHaveBeenCalledWith({
      pageNumber: 4,
      pageSize: 25,
      orderBy: 'totalDue',
      sortOrder: 'asc',
      orderDateFrom: '2013-01-01',
      orderDateTo: '2013-12-31',
      status: 2,
      salesPersonId: 283,
      territoryId: 6,
    });
  });

  it('neutralizes BOTH junk orderBy and junk pageNumber in the same bookmarked URL', () => {
    route.snapshot.queryParams = { orderBy: '; DROP TABLE', pageNumber: 'not-a-number', sortOrder: 'asc' };

    fixture.detectChanges();

    const lastCall = (salesOrderStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.at(-1)?.[0] as Record<
      string,
      unknown
    >;
    expect(lastCall['pageNumber']).toBe(1);
    expect(lastCall['orderBy']).toBe('orderDate');
    expect(lastCall['sortOrder']).toBe('desc');
  });

  it('resets all filters and sort, nulls URL params, and reloads the default view', () => {
    route.snapshot.queryParams = { status: '5', orderBy: 'totalDue', sortOrder: 'asc' };
    fixture.detectChanges();

    component['onResetFilters']();

    expect(salesOrderStore.loadPage).toHaveBeenLastCalledWith({
      pageNumber: 1,
      pageSize: 25,
      orderBy: 'orderDate',
      sortOrder: 'desc',
    });
    expect(component['sortColumn']()).toBe('');
    expect(component['filterForm'].getRawValue()).toEqual({
      orderDateFrom: '',
      orderDateTo: '',
      status: '',
      salesPersonId: '',
      territoryId: '',
    });
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        queryParams: {
          orderDateFrom: null,
          orderDateTo: null,
          status: null,
          salesPersonId: null,
          territoryId: null,
          pageNumber: null,
          orderBy: null,
          sortOrder: null,
        },
        queryParamsHandling: 'merge',
      }),
    );
  });

  it('sorts on an allowed column and writes the sort to the URL', () => {
    fixture.detectChanges();

    component['onSortChange']({ column: 'totalDue', direction: 'asc' });

    expect(salesOrderStore.loadPage).toHaveBeenLastCalledWith(
      expect.objectContaining({ orderBy: 'totalDue', sortOrder: 'asc' }),
    );
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: expect.objectContaining({ orderBy: 'totalDue', sortOrder: 'asc' }) }),
    );
  });

  it('does not call the store for a sort on a disallowed column key', () => {
    fixture.detectChanges();
    const callsBefore = (salesOrderStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.length;

    component['onSortChange']({ column: 'customerName', direction: 'asc' });

    expect((salesOrderStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.length).toBe(callsBefore);
  });

  it('carries active sort and filters across pages on page change', () => {
    fixture.detectChanges();
    component['filterForm'].patchValue({ territoryId: '4' });
    component['sortColumn'].set('totalDue');
    component['sortDirection'].set('asc');

    component['onPageChange'](3);

    expect(salesOrderStore.loadPage).toHaveBeenLastCalledWith({
      pageNumber: 3,
      pageSize: 25,
      orderBy: 'totalDue',
      sortOrder: 'asc',
      territoryId: 4,
    });
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: { pageNumber: 3 } }),
    );
  });

  it('navigates to the order detail on row click', () => {
    fixture.detectChanges();

    component['onRowClick']({ salesOrderId: 43659 });

    expect(router.navigate).toHaveBeenCalledWith(['/sales/orders', 43659]);
  });

  it('shows an error toast when the store reports an error', async () => {
    vi.spyOn(notificationService, 'error');
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(salesOrderStore), setError('load failed'));
    fixture.detectChanges();
    await fixture.whenStable();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to load sales orders. Please try again.');
  });

  it('projects status label, currency-ready total, and a dash for a null sales person', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    const orderNoSp: SalesOrder = { ...mockOrder, salesOrderId: 43660, salesPersonName: null };
    patchState(unprotected(salesOrderStore), setAllEntities([mockOrder, orderNoSp], { selectId }));
    fixture.detectChanges();

    const rows = component['rows']();
    expect(rows).toHaveLength(2);
    expect(rows[0]).toMatchObject({
      salesOrderId: 43659,
      salesOrderNumber: 'SO43659',
      orderDate: '2011-05-31',
      status: 'Shipped',
      statusKey: 'shipped',
      totalDue: 23153.2339,
      salesPersonName: 'Michael Blythe',
    });
    expect(rows[1]['salesPersonName']).toBe('—');
  });

  it('resolves the badge color through the real StatusBadgeComponent: the bound statusKey maps to a color, the cased status would fall through to gray', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    // statusDescription is the CASED server string "Shipped". The row projection exposes both the
    // cased `status` (for the column's nominal value) and the lowercased `statusKey`. The template
    // binds the badge to `statusKey`. This test feeds each value into the REAL StatusBadgeComponent
    // with the component's REAL statusBadgeMap to prove the case-sensitivity contract that the bug hinged on.
    patchState(unprotected(salesOrderStore), setAllEntities([{ ...mockOrder, statusDescription: 'Shipped' }], { selectId }));
    const projectedRow = component['rows']()[0];
    const map = component['statusBadgeMap'];

    // The value the template actually binds (statusKey) resolves to the mapped color.
    const goodFixture = TestBed.createComponent(StatusBadgeComponent);
    goodFixture.componentRef.setInput('status', projectedRow['statusKey'] as string);
    goodFixture.componentRef.setInput('statusMap', map);
    goodFixture.detectChanges();
    const goodSpan = goodFixture.nativeElement.querySelector('span') as HTMLElement;
    expect(goodSpan.className).toBe('badge badge-success');

    // The cased value (the original bug) would fall through to the gray badge-outline fallback —
    // this is why the template must bind statusKey, not status.
    const badFixture = TestBed.createComponent(StatusBadgeComponent);
    badFixture.componentRef.setInput('status', projectedRow['status'] as string);
    badFixture.componentRef.setInput('statusMap', map);
    badFixture.detectChanges();
    const badSpan = badFixture.nativeElement.querySelector('span') as HTMLElement;
    expect(badSpan.className).toBe('badge badge-outline');
  });

  it('maps every one of the 6 server status strings to a defined badge class (no badge-outline fallback)', () => {
    const map = component['statusBadgeMap'];
    // Keys are the server StatusDescription strings, lowercased (note "in process" — lowercase 'p').
    const serverStatuses = ['in process', 'approved', 'backordered', 'rejected', 'shipped', 'cancelled'];
    for (const key of serverStatuses) {
      expect(map[key]).toBeDefined();
      expect(map[key]).not.toBe('badge-outline');
      expect(map[key]).toMatch(/^badge-/);
    }
  });

  it('renders the grid even when the dropdown lookups fail (forkJoin error must not blank the list)', async () => {
    // Re-create the component with a failing lookup to prove the grid load is independent.
    TestBed.resetTestingModule();
    const failingRoute = buildRoute();
    await TestBed.configureTestingModule({
      imports: [OrderListComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: failingRoute },
      ],
    }).compileComponents();

    const store = TestBed.inject(SalesOrderStore);
    const api = TestBed.inject(SalesApiService);
    const lookup = TestBed.inject(LookupApiService);
    const notify = TestBed.inject(NotificationService);
    vi.spyOn(store, 'loadPage');
    vi.spyOn(notify, 'error');
    vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    vi.spyOn(api, 'getSalesPersons').mockReturnValue(throwError(() => new Error('lookup down')));
    vi.spyOn(lookup, 'getTerritories').mockReturnValue(throwError(() => new Error('lookup down')));

    const failFixture = TestBed.createComponent(OrderListComponent);
    failFixture.detectChanges();
    await failFixture.whenStable();

    // The grid load fired with the default view despite the lookup failure.
    expect(store.loadPage).toHaveBeenCalledWith({ pageNumber: 1, pageSize: 25, orderBy: 'orderDate', sortOrder: 'desc' });
    // The failure surfaced as the filter-options message, NOT the grid-load error message.
    expect(notify.error).toHaveBeenCalledWith('Failed to load filter options. Filtering may be unavailable.');
    expect(notify.error).not.toHaveBeenCalledWith('Failed to load sales orders. Please try again.');
    // The grid did not enter an error state from the lookup failure.
    expect(store.hasError()).toBe(false);
  });
});
