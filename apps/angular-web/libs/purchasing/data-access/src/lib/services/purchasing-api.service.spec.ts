import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import { PurchasingApiService } from './purchasing-api.service';
import type { VendorListItem } from '../models/vendor-list-item.model';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://localhost:44369/api', name: 'Test API' },
  },
};

const BASE_URL = 'https://localhost:44369/api';

// Real AdventureWorks vendor row (BusinessEntityID 1576 — top spender by TotalDue).
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

describe('PurchasingApiService', () => {
  let service: PurchasingApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: ENVIRONMENT, useValue: mockEnvironment }],
    });
    service = TestBed.inject(PurchasingApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should be injectable', () => {
    expect(service).toBeTruthy();
  });

  describe('getVendors', () => {
    it('should GET vendors without params', () => {
      const mockData: SearchResult<VendorListItem> = {
        pageNumber: 1,
        pageSize: 25,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
        totalRecords: 1,
        results: [mockVendor],
      };

      service.getVendors().subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/vendors`);
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should GET vendors with pagination params', () => {
      const mockData: SearchResult<VendorListItem> = {
        pageNumber: 2,
        pageSize: 10,
        totalPages: 11,
        hasPreviousPage: true,
        hasNextPage: true,
        totalRecords: 104,
        results: [],
      };

      service.getVendors({ pageNumber: 2, pageSize: 10 }).subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/vendors?pageNumber=2&pageSize=10`);
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should GET vendors with all filter params in the query string', () => {
      const mockData: SearchResult<VendorListItem> = {
        pageNumber: 1,
        pageSize: 25,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
        totalRecords: 1,
        results: [mockVendor],
      };

      service
        .getVendors({
          pageNumber: 1,
          pageSize: 25,
          creditRating: 1,
          preferredVendorStatus: true,
          activeFlag: true,
        })
        .subscribe((result) => {
          expect(result).toEqual(mockData);
        });

      const req = httpTesting.expectOne(
        `${BASE_URL}/v1/vendors?pageNumber=1&pageSize=25&creditRating=1&preferredVendorStatus=true&activeFlag=true`,
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });
  });
});
