import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import type { SalesPerson } from '../models/sales-person.model';
import { SalesPersonStore } from './sales-person.store';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockSalesPerson: SalesPerson = {
  id: 1,
  title: null,
  firstName: 'Jane',
  middleName: null,
  lastName: 'Doe',
  suffix: null,
  jobTitle: 'Sales Representative',
  emailAddress: 'jane.doe@example.com',
  territoryId: 1,
  salesQuota: 250000,
  bonus: 5000,
  commissionPct: 0.02,
  modifiedDate: '2026-01-01T00:00:00',
};

const mockSearchResult: SearchResult<SalesPerson> = {
  pageNumber: 1,
  pageSize: 10,
  totalPages: 1,
  totalRecords: 1,
  hasPreviousPage: false,
  hasNextPage: false,
  results: [mockSalesPerson],
};

describe('SalesPersonStore', () => {
  let store: InstanceType<typeof SalesPersonStore>;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        SalesPersonStore,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    });
    store = TestBed.inject(SalesPersonStore);
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
      store.loadPage({ pageNumber: 1, pageSize: 10 });

      expect(store.isLoading()).toBe(true);

      const req = httpTesting.expectOne('https://api.test.com/v1/salespersons?pageNumber=1&pageSize=10');
      expect(req.request.method).toBe('GET');
      req.flush(mockSearchResult);

      expect(store.entities()).toEqual([mockSalesPerson]);
      expect(store.isLoaded()).toBe(true);
      expect(store.totalRecords()).toBe(1);
    });

    it('should handle empty results without error', () => {
      const emptyResult: SearchResult<SalesPerson> = {
        ...mockSearchResult,
        totalRecords: 0,
        totalPages: 0,
        results: [],
      };

      store.loadPage({ pageNumber: 1, pageSize: 10 });
      const req = httpTesting.expectOne('https://api.test.com/v1/salespersons?pageNumber=1&pageSize=10');
      req.flush(emptyResult);

      expect(store.entities()).toEqual([]);
      expect(store.isLoaded()).toBe(true);
      expect(store.hasError()).toBe(false);
    });

    it('should handle null results without error', () => {
      const nullResult: SearchResult<SalesPerson> = {
        ...mockSearchResult,
        totalRecords: 0,
        totalPages: 0,
        results: null,
      };

      store.loadPage({ pageNumber: 1, pageSize: 10 });
      const req = httpTesting.expectOne('https://api.test.com/v1/salespersons?pageNumber=1&pageSize=10');
      req.flush(nullResult);

      expect(store.entities()).toEqual([]);
      expect(store.isLoaded()).toBe(true);
    });

    it('should set error state on failure', () => {
      store.loadPage({ pageNumber: 1, pageSize: 10 });
      const req = httpTesting.expectOne('https://api.test.com/v1/salespersons?pageNumber=1&pageSize=10');
      req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

      expect(store.hasError()).toBe(true);
      expect(store.error()).toBeTruthy();
    });
  });

  describe('search', () => {
    it('should search entities and set pagination', () => {
      store.search({ params: { pageNumber: 1, pageSize: 10 }, body: { firstName: 'Jane' } });

      expect(store.isLoading()).toBe(true);

      const req = httpTesting.expectOne('https://api.test.com/v1/salespersons/search?pageNumber=1&pageSize=10');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ firstName: 'Jane' });
      req.flush(mockSearchResult);

      expect(store.entities()).toEqual([mockSalesPerson]);
      expect(store.isLoaded()).toBe(true);
    });
  });

  describe('loadById', () => {
    it('should load a single entity', () => {
      store.loadById(1);

      expect(store.isLoading()).toBe(true);

      const req = httpTesting.expectOne('https://api.test.com/v1/salespersons/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockSalesPerson);

      expect(store.entities()).toEqual([mockSalesPerson]);
      expect(store.isLoaded()).toBe(true);
    });

    it('should set error on failure', () => {
      store.loadById(999);
      const req = httpTesting.expectOne('https://api.test.com/v1/salespersons/999');
      req.flush('Not Found', { status: 404, statusText: 'Not Found' });

      expect(store.hasError()).toBe(true);
    });
  });

  describe('create', () => {
    it('should add created entity to store', () => {
      const createModel = {
        firstName: 'John',
        lastName: 'Smith',
        nationalIdNumber: '123456789',
        loginId: 'adventure-works\\john',
        jobTitle: 'Sales Representative',
        birthDate: '1990-01-01',
        hireDate: '2026-01-01',
        maritalStatus: 'S' as const,
        gender: 'M' as const,
        salariedFlag: true,
        bonus: 5000,
        commissionPct: 0.02,
        phone: { phoneNumber: '555-0100', phoneNumberTypeId: 1 },
        emailAddress: 'john.smith@example.com',
        address: {
          addressLine1: '123 Main St',
          city: 'Seattle',
          stateProvince: { id: 1, name: 'Washington', code: 'WA' },
          postalCode: '98101',
        },
        addressTypeId: 1,
      };

      store.create(createModel);

      const req = httpTesting.expectOne('https://api.test.com/v1/salespersons');
      expect(req.request.method).toBe('POST');
      req.flush({ ...mockSalesPerson, id: 2, firstName: 'John', lastName: 'Smith' });

      expect(store.entities().length).toBe(1);
      expect(store.isLoaded()).toBe(true);
    });
  });

  describe('update', () => {
    it('should update existing entity in store', () => {
      // First load the entity
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/salespersons/1');
      loadReq.flush(mockSalesPerson);

      // Then update
      const updateModel = {
        id: 1,
        firstName: 'Jane',
        lastName: 'Updated',
        jobTitle: 'Senior Sales Representative',
        maritalStatus: 'S' as const,
        gender: 'F' as const,
        salariedFlag: true,
        bonus: 6000,
        commissionPct: 0.03,
      };
      store.update({ id: 1, model: updateModel });

      const updateReq = httpTesting.expectOne('https://api.test.com/v1/salespersons/1');
      expect(updateReq.request.method).toBe('PUT');
      updateReq.flush({ ...mockSalesPerson, lastName: 'Updated', jobTitle: 'Senior Sales Representative' });

      expect(store.entities()[0].lastName).toBe('Updated');
      expect(store.isLoaded()).toBe(true);
    });
  });
});
