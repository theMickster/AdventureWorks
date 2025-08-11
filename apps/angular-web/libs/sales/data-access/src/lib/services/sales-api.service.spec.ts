import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { SalesApiService } from './sales-api.service';
import type { Store, StoreCreate, StoreUpdate } from '../models/store.model';
import type { SalesPerson, SalesPersonCreate, SalesPersonUpdate } from '../models/sales-person.model';
import type { StoreSearchBody } from '../models/store-search.model';
import type { SalesPersonSearchBody } from '../models/sales-person-search.model';
import type { SearchResult } from '@adventureworks-web/shared/data-access';

const mockEnvironment = {
  production: false,
  defaultLocale: 'en',
  api: {
    primary: { baseUrl: 'https://localhost:44369/api', name: 'Test API' },
  },
};

const BASE_URL = 'https://localhost:44369/api';

describe('SalesApiService', () => {
  let service: SalesApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: ENVIRONMENT, useValue: mockEnvironment }],
    });
    service = TestBed.inject(SalesApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should be injectable', () => {
    expect(service).toBeTruthy();
  });

  describe('Stores', () => {
    it('should GET stores without params', () => {
      const mockData: SearchResult<Store> = {
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
        totalRecords: 1,
        results: [
          {
            id: 1,
            name: 'Bike World',
            modifiedDate: '2024-01-01T00:00:00',
            storeAddresses: [],
            storeContacts: [],
            salesPerson: null,
          },
        ],
      };

      service.getStores().subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/stores`);
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should GET stores with params', () => {
      const mockData: SearchResult<Store> = {
        pageNumber: 2,
        pageSize: 20,
        totalPages: 3,
        hasPreviousPage: true,
        hasNextPage: true,
        totalRecords: 50,
        results: [],
      };

      service.getStores({ pageNumber: 2, pageSize: 20 }).subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/stores?pageNumber=2&pageSize=20`);
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should GET a single store by id', () => {
      const mockData: Store = {
        id: 42,
        name: 'Mountain Gear',
        modifiedDate: '2024-01-01T00:00:00',
        storeAddresses: [],
        storeContacts: [],
        salesPerson: null,
      };

      service.getStore(42).subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/stores/42`);
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should POST to create a store', () => {
      const body: StoreCreate = { name: 'New Store', salesPersonId: 5 };
      const mockResponse: Store = {
        id: 99,
        name: 'New Store',
        modifiedDate: '2024-06-01T00:00:00',
        storeAddresses: [],
        storeContacts: [],
        salesPerson: null,
      };

      service.createStore(body).subscribe((result) => {
        expect(result).toEqual(mockResponse);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/stores`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush(mockResponse);
    });

    it('should PUT to update a store', () => {
      const body: StoreUpdate = { id: 42, name: 'Updated Store', salesPersonId: 3 };
      const mockResponse: Store = {
        id: 42,
        name: 'Updated Store',
        modifiedDate: '2024-06-15T00:00:00',
        storeAddresses: [],
        storeContacts: [],
        salesPerson: null,
      };

      service.updateStore(42, body).subscribe((result) => {
        expect(result).toEqual(mockResponse);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/stores/42`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(body);
      req.flush(mockResponse);
    });

    it('should POST to search stores with query params', () => {
      const body: StoreSearchBody = { name: 'Bike' };
      const mockData: SearchResult<Store> = {
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
        totalRecords: 2,
        results: [
          {
            id: 1,
            name: 'Bike World',
            modifiedDate: '2024-01-01T00:00:00',
            storeAddresses: [],
            storeContacts: [],
            salesPerson: null,
          },
        ],
      };

      service.searchStores({ pageNumber: 1, pageSize: 10 }, body).subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/stores/search?pageNumber=1&pageSize=10`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush(mockData);
    });
  });

  describe('SalesPersons', () => {
    it('should GET sales persons without params', () => {
      const mockData: SearchResult<SalesPerson> = {
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
        totalRecords: 1,
        results: [
          {
            id: 282,
            title: 'Mr.',
            firstName: 'José',
            middleName: null,
            lastName: 'Saraiva',
            suffix: null,
            jobTitle: 'Sales Representative',
            emailAddress: 'josé0@adventure-works.com',
            territoryId: 7,
            salesQuota: 250000,
            bonus: 5000,
            commissionPct: 0.012,
            modifiedDate: '2024-01-01T00:00:00',
          },
        ],
      };

      service.getSalesPersons().subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/salespersons`);
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should GET sales persons with params', () => {
      const mockData: SearchResult<SalesPerson> = {
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
        totalRecords: 0,
        results: [],
      };

      service.getSalesPersons({ orderBy: 'lastName' }).subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/salespersons?orderBy=lastName`);
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should GET a single sales person by id', () => {
      const mockData: SalesPerson = {
        id: 282,
        title: 'Mr.',
        firstName: 'José',
        middleName: null,
        lastName: 'Saraiva',
        suffix: null,
        jobTitle: 'Sales Representative',
        emailAddress: 'josé0@adventure-works.com',
        territoryId: 7,
        salesQuota: 250000,
        bonus: 5000,
        commissionPct: 0.012,
        modifiedDate: '2024-01-01T00:00:00',
      };

      service.getSalesPerson(282).subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/salespersons/282`);
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should POST to create a sales person', () => {
      const body: SalesPersonCreate = {
        firstName: 'Jane',
        lastName: 'Doe',
        nationalIdNumber: '123456789',
        loginId: 'adventure-works\\jane0',
        jobTitle: 'Sales Representative',
        birthDate: '1990-01-15',
        hireDate: '2024-06-01',
        maritalStatus: 'S',
        gender: 'F',
        salariedFlag: true,
        bonus: 3000,
        commissionPct: 0.01,
        phone: { phoneNumber: '555-0100', phoneNumberTypeId: 1 },
        emailAddress: 'jane0@adventure-works.com',
        address: {
          addressLine1: '123 Main St',
          city: 'Seattle',
          stateProvince: { id: 79, name: 'Washington', code: 'WA' },
          postalCode: '98101',
        },
        addressTypeId: 2,
      };
      const mockResponse: SalesPerson = {
        id: 300,
        title: null,
        firstName: 'Jane',
        middleName: null,
        lastName: 'Doe',
        suffix: null,
        jobTitle: 'Sales Representative',
        emailAddress: 'jane0@adventure-works.com',
        territoryId: null,
        salesQuota: null,
        bonus: 3000,
        commissionPct: 0.01,
        modifiedDate: '2024-06-01T00:00:00',
      };

      service.createSalesPerson(body).subscribe((result) => {
        expect(result).toEqual(mockResponse);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/salespersons`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush(mockResponse);
    });

    it('should PUT to update a sales person', () => {
      const body: SalesPersonUpdate = {
        id: 282,
        firstName: 'José',
        lastName: 'Saraiva',
        jobTitle: 'Senior Sales Representative',
        maritalStatus: 'M',
        gender: 'M',
        salariedFlag: true,
        bonus: 7500,
        commissionPct: 0.015,
      };
      const mockResponse: SalesPerson = {
        id: 282,
        title: 'Mr.',
        firstName: 'José',
        middleName: null,
        lastName: 'Saraiva',
        suffix: null,
        jobTitle: 'Senior Sales Representative',
        emailAddress: 'josé0@adventure-works.com',
        territoryId: 7,
        salesQuota: 250000,
        bonus: 7500,
        commissionPct: 0.015,
        modifiedDate: '2024-06-15T00:00:00',
      };

      service.updateSalesPerson(282, body).subscribe((result) => {
        expect(result).toEqual(mockResponse);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/salespersons/282`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(body);
      req.flush(mockResponse);
    });

    it('should POST to search sales persons with query params', () => {
      const body: SalesPersonSearchBody = { lastName: 'Saraiva' };
      const mockData: SearchResult<SalesPerson> = {
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
        totalRecords: 1,
        results: [
          {
            id: 282,
            title: 'Mr.',
            firstName: 'José',
            middleName: null,
            lastName: 'Saraiva',
            suffix: null,
            jobTitle: 'Sales Representative',
            emailAddress: 'josé0@adventure-works.com',
            territoryId: 7,
            salesQuota: 250000,
            bonus: 5000,
            commissionPct: 0.012,
            modifiedDate: '2024-01-01T00:00:00',
          },
        ],
      };

      service.searchSalesPersons({ orderBy: 'lastName' }, body).subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/salespersons/search?orderBy=lastName`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush(mockData);
    });
  });
});
