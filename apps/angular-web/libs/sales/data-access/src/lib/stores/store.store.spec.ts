import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import type { Store } from '../models/store.model';
import { StoreStore } from './store.store';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockStore: Store = {
  id: 1,
  name: 'Test Store',
  modifiedDate: '2026-01-01T00:00:00',
  storeAddresses: [],
  storeContacts: [],
  salesPerson: null,
};

const mockSearchResult: SearchResult<Store> = {
  pageNumber: 1,
  pageSize: 10,
  totalPages: 1,
  totalRecords: 1,
  hasPreviousPage: false,
  hasNextPage: false,
  results: [mockStore],
};

describe('StoreStore', () => {
  let store: InstanceType<typeof StoreStore>;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        StoreStore,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    });
    store = TestBed.inject(StoreStore);
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

      const req = httpTesting.expectOne('https://api.test.com/v1/stores?pageNumber=1&pageSize=10');
      expect(req.request.method).toBe('GET');
      req.flush(mockSearchResult);

      expect(store.entities()).toEqual([mockStore]);
      expect(store.isLoaded()).toBe(true);
      expect(store.totalRecords()).toBe(1);
      expect(store.pageNumber()).toBe(1);
    });

    it('should handle empty results without error', () => {
      const emptyResult: SearchResult<Store> = {
        ...mockSearchResult,
        totalRecords: 0,
        totalPages: 0,
        results: [],
      };

      store.loadPage({ pageNumber: 1, pageSize: 10 });
      const req = httpTesting.expectOne('https://api.test.com/v1/stores?pageNumber=1&pageSize=10');
      req.flush(emptyResult);

      expect(store.entities()).toEqual([]);
      expect(store.isLoaded()).toBe(true);
      expect(store.hasError()).toBe(false);
    });

    it('should handle null results without error', () => {
      const nullResult: SearchResult<Store> = {
        ...mockSearchResult,
        totalRecords: 0,
        totalPages: 0,
        results: null,
      };

      store.loadPage({ pageNumber: 1, pageSize: 10 });
      const req = httpTesting.expectOne('https://api.test.com/v1/stores?pageNumber=1&pageSize=10');
      req.flush(nullResult);

      expect(store.entities()).toEqual([]);
      expect(store.isLoaded()).toBe(true);
    });

    it('should set error state on failure', () => {
      store.loadPage({ pageNumber: 1, pageSize: 10 });
      const req = httpTesting.expectOne('https://api.test.com/v1/stores?pageNumber=1&pageSize=10');
      req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

      expect(store.hasError()).toBe(true);
      expect(store.error()).toBeTruthy();
    });
  });

  describe('search', () => {
    it('should search entities and set pagination', () => {
      store.search({ params: { pageNumber: 1, pageSize: 10 }, body: { name: 'Test' } });

      expect(store.isLoading()).toBe(true);

      const req = httpTesting.expectOne('https://api.test.com/v1/stores/search?pageNumber=1&pageSize=10');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ name: 'Test' });
      req.flush(mockSearchResult);

      expect(store.entities()).toEqual([mockStore]);
      expect(store.isLoaded()).toBe(true);
    });
  });

  describe('loadById', () => {
    it('should load a single entity', () => {
      store.loadById(1);

      expect(store.isLoading()).toBe(true);

      const req = httpTesting.expectOne('https://api.test.com/v1/stores/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockStore);

      expect(store.entities()).toEqual([mockStore]);
      expect(store.isLoaded()).toBe(true);
    });

    it('should set error on failure', () => {
      store.loadById(999);
      const req = httpTesting.expectOne('https://api.test.com/v1/stores/999');
      req.flush('Not Found', { status: 404, statusText: 'Not Found' });

      expect(store.hasError()).toBe(true);
    });
  });

  describe('create', () => {
    it('should add created entity to store', () => {
      store.create({ name: 'New Store' });

      expect(store.isLoading()).toBe(true);

      const req = httpTesting.expectOne('https://api.test.com/v1/stores');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ name: 'New Store' });
      req.flush({ ...mockStore, id: 2, name: 'New Store' });

      expect(store.entities().length).toBe(1);
      expect(store.entities()[0].name).toBe('New Store');
      expect(store.isLoaded()).toBe(true);
    });
  });

  describe('update', () => {
    it('should update existing entity in store', () => {
      // First load the entity
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/stores/1');
      loadReq.flush(mockStore);

      // Then update
      const updateModel = { id: 1, name: 'Updated Store' };
      store.update({ id: 1, model: updateModel });

      const updateReq = httpTesting.expectOne('https://api.test.com/v1/stores/1');
      expect(updateReq.request.method).toBe('PUT');
      updateReq.flush({ ...mockStore, name: 'Updated Store' });

      expect(store.entities()[0].name).toBe('Updated Store');
      expect(store.isLoaded()).toBe(true);
    });
  });
});
