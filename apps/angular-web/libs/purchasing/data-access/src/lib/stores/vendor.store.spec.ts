import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import type { VendorListItem } from '../models/vendor-list-item.model';
import { VendorStore } from './vendor.store';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

// Real AdventureWorks vendor rows (BusinessEntityID 1576, 1678 — top spenders by TotalDue).
const mockVendor: VendorListItem = {
  vendorId: 1576,
  name: 'Superior Bicycles',
  accountNumber: 'SUPERIOR0001',
  creditRatingLabel: 'Superior',
  preferredVendorStatus: true,
  activeFlag: true,
  totalSpend: 5034266.74,
  poCount: 50,
  isHighRisk: false,
};

const mockHighRiskVendor: VendorListItem = {
  vendorId: 1678,
  name: 'Proseware, Inc.',
  accountNumber: 'PROSEWAR0001',
  creditRatingLabel: 'Average',
  preferredVendorStatus: false,
  activeFlag: false,
  totalSpend: 2593901.31,
  poCount: 51,
  isHighRisk: true,
};

const mockSearchResult: SearchResult<VendorListItem> = {
  pageNumber: 1,
  pageSize: 25,
  totalPages: 5,
  totalRecords: 104,
  hasPreviousPage: false,
  hasNextPage: true,
  results: [mockVendor, mockHighRiskVendor],
};

describe('VendorStore', () => {
  let store: InstanceType<typeof VendorStore>;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        VendorStore,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    });
    store = TestBed.inject(VendorStore);
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
    it('transitions loading -> loaded and populates entities and pagination', () => {
      store.loadPage({ pageNumber: 1, pageSize: 25 });

      expect(store.isLoading()).toBe(true);
      expect(store.requestStatus()).toBe('loading');

      const req = httpTesting.expectOne('https://api.test.com/v1/vendors?pageNumber=1&pageSize=25');
      expect(req.request.method).toBe('GET');
      req.flush(mockSearchResult);

      expect(store.requestStatus()).toBe('loaded');
      expect(store.isLoaded()).toBe(true);
      expect(store.entities()).toEqual([mockVendor, mockHighRiskVendor]);
      expect(store.totalRecords()).toBe(104);
      expect(store.pageNumber()).toBe(1);
    });

    it('forwards creditRating, preferredVendorStatus, and activeFlag filters in the query string and applies the filtered result', () => {
      store.loadPage({ pageNumber: 1, pageSize: 25, creditRating: 4, preferredVendorStatus: true, activeFlag: true });

      const req = httpTesting.expectOne(
        'https://api.test.com/v1/vendors?pageNumber=1&pageSize=25&creditRating=4&preferredVendorStatus=true&activeFlag=true',
      );
      expect(req.request.method).toBe('GET');
      req.flush({ ...mockSearchResult, totalRecords: 0, totalPages: 0, results: [] });

      expect(store.isLoaded()).toBe(true);
      expect(store.hasError()).toBe(false);
      expect(store.entities()).toEqual([]);
      expect(store.totalRecords()).toBe(0);
    });

    it('handles empty results without error', () => {
      const emptyResult: SearchResult<VendorListItem> = {
        ...mockSearchResult,
        totalRecords: 0,
        totalPages: 0,
        results: [],
      };

      store.loadPage({ pageNumber: 1, pageSize: 25 });
      const req = httpTesting.expectOne('https://api.test.com/v1/vendors?pageNumber=1&pageSize=25');
      req.flush(emptyResult);

      expect(store.entities()).toEqual([]);
      expect(store.isLoaded()).toBe(true);
      expect(store.hasError()).toBe(false);
    });

    it('handles null results without error', () => {
      const nullResult: SearchResult<VendorListItem> = {
        ...mockSearchResult,
        totalRecords: 0,
        totalPages: 0,
        results: null,
      };

      store.loadPage({ pageNumber: 1, pageSize: 25 });
      const req = httpTesting.expectOne('https://api.test.com/v1/vendors?pageNumber=1&pageSize=25');
      req.flush(nullResult);

      expect(store.entities()).toEqual([]);
      expect(store.isLoaded()).toBe(true);
    });

    it("sets requestStatus to 'error' on a 500 response without throwing (AC3)", () => {
      store.loadPage({ pageNumber: 1, pageSize: 25 });
      const req = httpTesting.expectOne('https://api.test.com/v1/vendors?pageNumber=1&pageSize=25');

      expect(() => req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' })).not.toThrow();

      expect(store.requestStatus()).toBe('error');
      expect(store.hasError()).toBe(true);
      expect(store.error()).toBeTruthy();
    });
  });
});
