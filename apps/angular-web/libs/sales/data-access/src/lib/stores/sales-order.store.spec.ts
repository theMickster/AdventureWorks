import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { throwError } from 'rxjs';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { ApiEmptyResultError } from '@adventureworks-web/shared/data-access';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
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
});
