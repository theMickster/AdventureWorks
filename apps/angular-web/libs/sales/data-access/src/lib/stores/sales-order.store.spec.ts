import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { of, Subject, throwError } from 'rxjs';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { ApiEmptyResultError } from '@adventureworks-web/shared/data-access';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import type { SalesOrderAnalytics } from '../models/sales-order-analytics.model';
import type { SalesOrder } from '../models/sales-order.model';
import { SalesApiService } from '../services/sales-api.service';
import { SalesOrderStore } from './sales-order.store';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

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

const mockSearchResult: SearchResult<SalesOrder> = {
  pageNumber: 1,
  pageSize: 25,
  totalPages: 1,
  totalRecords: 1,
  hasPreviousPage: false,
  hasNextPage: false,
  results: [mockOrder],
};

describe('SalesOrderStore', () => {
  let store: InstanceType<typeof SalesOrderStore>;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        SalesOrderStore,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    });
    store = TestBed.inject(SalesOrderStore);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('has idle initial state', () => {
    expect(store.entities()).toEqual([]);
    expect(store.requestStatus()).toBe('idle');
    expect(store.totalRecords()).toBe(0);
  });

  describe('loadPage', () => {
    it('loads entities and sets pagination on success', () => {
      store.loadPage({ pageNumber: 1, pageSize: 25 });

      expect(store.isLoading()).toBe(true);

      const req = httpTesting.expectOne('https://api.test.com/v1/sales-orders?pageNumber=1&pageSize=25');
      expect(req.request.method).toBe('GET');
      req.flush(mockSearchResult);

      expect(store.entities()).toEqual([mockOrder]);
      expect(store.isLoaded()).toBe(true);
      expect(store.totalRecords()).toBe(1);
    });

    it('sets error state on failure', () => {
      store.loadPage({ pageNumber: 1, pageSize: 25 });
      const req = httpTesting.expectOne('https://api.test.com/v1/sales-orders?pageNumber=1&pageSize=25');
      req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

      expect(store.hasError()).toBe(true);
      expect(store.error()).toBeTruthy();
    });

    it('treats ApiEmptyResultError as an empty loaded result, not an error', () => {
      const salesApi = TestBed.inject(SalesApiService);
      vi.spyOn(salesApi, 'getSalesOrders').mockReturnValue(
        throwError(() => new ApiEmptyResultError('No results')),
      );

      store.loadPage({ pageNumber: 1, pageSize: 25 });

      expect(store.entities()).toEqual([]);
      expect(store.isLoaded()).toBe(true);
      expect(store.hasError()).toBe(false);
    });

    it('forwards all filter params to the sales-orders API', () => {
      store.loadPage({
        pageNumber: 2,
        pageSize: 25,
        orderBy: 'totalDue',
        sortOrder: 'desc',
        orderDateFrom: '2013-01-01',
        orderDateTo: '2013-12-31',
        status: 5,
        salesPersonId: 279,
        territoryId: 4,
      });

      const req = httpTesting.expectOne(
        'https://api.test.com/v1/sales-orders?pageNumber=2&pageSize=25&orderBy=totalDue&sortOrder=desc&orderDateFrom=2013-01-01&orderDateTo=2013-12-31&status=5&salesPersonId=279&territoryId=4',
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockSearchResult);

      expect(store.isLoaded()).toBe(true);
    });
  });

  describe('loadAnalytics', () => {
    const mockAnalytics: SalesOrderAnalytics = {
      totalRevenue: 500000,
      orderCount: 42,
      percentageOfTotal: 15.5,
      monthlyTrend: [
        { year: 2013, month: 1, revenue: 50000 },
        { year: 2013, month: 2, revenue: 45000 },
      ],
    };

    it('transitions loading → loaded and populates analytics on success', () => {
      const salesApi = TestBed.inject(SalesApiService);
      vi.spyOn(salesApi, 'getOrderAnalytics').mockReturnValue(of(mockAnalytics));

      store.loadAnalytics({ orderDateFrom: '2013-01-01', orderDateTo: '2013-12-31' });

      expect(store.analyticsStatus()).toBe('loaded');
      expect(store.analytics()).toEqual(mockAnalytics);
      expect(store.analyticsIsLoading()).toBe(false);
      expect(store.analyticsHasError()).toBe(false);
    });

    it('transitions to error and keeps analytics null on 500', () => {
      const salesApi = TestBed.inject(SalesApiService);
      vi.spyOn(salesApi, 'getOrderAnalytics').mockReturnValue(
        throwError(() => new Error('Internal Server Error')),
      );

      store.loadAnalytics({});

      expect(store.analyticsStatus()).toBe('error');
      expect(store.analytics()).toBeNull();
      expect(store.analyticsHasError()).toBe(true);
      expect(store.analyticsError()).toBe('Failed to load order analytics');
      // CWE-209: raw server error message must never reach analyticsError
      expect(store.analyticsError()).not.toBe('Internal Server Error');
    });

    it('switchMap cancellation: only the second call result populates state', () => {
      const salesApi = TestBed.inject(SalesApiService);

      const firstSubject = new Subject<SalesOrderAnalytics>();
      const secondAnalytics: SalesOrderAnalytics = { totalRevenue: 2, orderCount: 2, percentageOfTotal: 2, monthlyTrend: [] };

      let callCount = 0;
      vi.spyOn(salesApi, 'getOrderAnalytics').mockImplementation(() => {
        callCount++;
        if (callCount === 1) {
          // First call: slow observable that has not emitted yet
          return firstSubject.asObservable();
        }
        return of(secondAnalytics);
      });

      store.loadAnalytics({ territoryId: 1 });
      store.loadAnalytics({ territoryId: 2 });

      // switchMap cancels first; only second result applies
      expect(store.analytics()?.orderCount).toBe(2);
      expect(store.analyticsStatus()).toBe('loaded');

      firstSubject.complete();
    });
  });

  describe('applyFilters', () => {
    it('calls both getSalesOrders and getOrderAnalytics when applyFilters is called', () => {
      const salesApi = TestBed.inject(SalesApiService);
      const getSalesOrdersSpy = vi.spyOn(salesApi, 'getSalesOrders').mockReturnValue(of({
        pageNumber: 1, pageSize: 25, totalPages: 1, totalRecords: 1,
        hasPreviousPage: false, hasNextPage: false, results: [mockOrder],
      }));
      const getOrderAnalyticsSpy = vi.spyOn(salesApi, 'getOrderAnalytics').mockReturnValue(of({
        totalRevenue: 100, orderCount: 1, percentageOfTotal: 5, monthlyTrend: [],
      }));

      store.applyFilters({
        pageNumber: 1,
        pageSize: 25,
        orderDateFrom: '2013-01-01',
        orderDateTo: '2013-12-31',
        territoryId: 2,
      });

      expect(getSalesOrdersSpy).toHaveBeenCalledTimes(1);
      expect(getOrderAnalyticsSpy).toHaveBeenCalledTimes(1);

      const analyticsCallArg = getOrderAnalyticsSpy.mock.calls[0][0];
      expect(analyticsCallArg).not.toHaveProperty('pageNumber');
      expect(analyticsCallArg).not.toHaveProperty('pageSize');
      expect(analyticsCallArg.orderDateFrom).toBe('2013-01-01');
      expect(analyticsCallArg.territoryId).toBe(2);
    });

    it('loadPage directly does NOT trigger getOrderAnalytics', () => {
      const salesApi = TestBed.inject(SalesApiService);
      vi.spyOn(salesApi, 'getSalesOrders').mockReturnValue(of({
        pageNumber: 1, pageSize: 25, totalPages: 1, totalRecords: 1,
        hasPreviousPage: false, hasNextPage: false, results: [mockOrder],
      }));
      const getOrderAnalyticsSpy = vi.spyOn(salesApi, 'getOrderAnalytics');

      store.loadPage({ pageNumber: 1, pageSize: 25 });

      expect(getOrderAnalyticsSpy).not.toHaveBeenCalled();
    });
  });
});
