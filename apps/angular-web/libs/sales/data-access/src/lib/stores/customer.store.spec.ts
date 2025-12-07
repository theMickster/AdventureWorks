import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import type { CustomerListItem } from '../models/customer-list-item.model';
import { CustomerStore } from './customer.store';

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

const mockSearchResult: SearchResult<CustomerListItem> = {
  pageNumber: 1,
  pageSize: 25,
  totalPages: 1,
  totalRecords: 1,
  hasPreviousPage: false,
  hasNextPage: false,
  results: [mockCustomer],
};

describe('CustomerStore', () => {
  let store: InstanceType<typeof CustomerStore>;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        CustomerStore,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    });
    store = TestBed.inject(CustomerStore);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should have idle initial state', () => {
    expect(store.entities()).toEqual([]);
    expect(store.requestStatus()).toBe('idle');
    expect(store.totalRecords()).toBe(0);
    expect(store.pageNumber()).toBe(1);
  });

  describe('loadPage', () => {
    it('should load entities and set pagination', () => {
      store.loadPage({ pageNumber: 1, pageSize: 25 });

      expect(store.isLoading()).toBe(true);

      const req = httpTesting.expectOne('https://api.test.com/v1/customers?pageNumber=1&pageSize=25');
      expect(req.request.method).toBe('GET');
      req.flush(mockSearchResult);

      expect(store.entities()).toEqual([mockCustomer]);
      expect(store.isLoaded()).toBe(true);
      expect(store.totalRecords()).toBe(1);
      expect(store.pageNumber()).toBe(1);
    });

    it('should forward the search param to the query string', () => {
      store.loadPage({ pageNumber: 1, pageSize: 25, search: 'yang' });

      const req = httpTesting.expectOne('https://api.test.com/v1/customers?pageNumber=1&pageSize=25&search=yang');
      expect(req.request.method).toBe('GET');
      req.flush(mockSearchResult);

      expect(store.entities()).toEqual([mockCustomer]);
    });

    it('should handle empty results without error', () => {
      const emptyResult: SearchResult<CustomerListItem> = {
        ...mockSearchResult,
        totalRecords: 0,
        totalPages: 0,
        results: [],
      };

      store.loadPage({ pageNumber: 1, pageSize: 25 });
      const req = httpTesting.expectOne('https://api.test.com/v1/customers?pageNumber=1&pageSize=25');
      req.flush(emptyResult);

      expect(store.entities()).toEqual([]);
      expect(store.isLoaded()).toBe(true);
      expect(store.hasError()).toBe(false);
    });

    it('should handle null results without error', () => {
      const nullResult: SearchResult<CustomerListItem> = {
        ...mockSearchResult,
        totalRecords: 0,
        totalPages: 0,
        results: null,
      };

      store.loadPage({ pageNumber: 1, pageSize: 25 });
      const req = httpTesting.expectOne('https://api.test.com/v1/customers?pageNumber=1&pageSize=25');
      req.flush(nullResult);

      expect(store.entities()).toEqual([]);
      expect(store.isLoaded()).toBe(true);
    });

    it('should set error requestStatus on HTTP failure without throwing', () => {
      expect(() => {
        store.loadPage({ pageNumber: 1, pageSize: 25 });
        const req = httpTesting.expectOne('https://api.test.com/v1/customers?pageNumber=1&pageSize=25');
        req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });
      }).not.toThrow();

      expect(store.requestStatus()).toBe('error');
      expect(store.hasError()).toBe(true);
      expect(store.error()).toBeTruthy();
    });
  });
});
