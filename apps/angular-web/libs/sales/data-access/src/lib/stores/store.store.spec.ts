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

  describe('refresh', () => {
    it('replays the last load page request', () => {
      store.loadPage({ pageNumber: 2, pageSize: 5 });
      const firstReq = httpTesting.expectOne('https://api.test.com/v1/stores?pageNumber=2&pageSize=5');
      firstReq.flush({ ...mockSearchResult, pageNumber: 2, pageSize: 5 });

      store.refresh();
      const refreshReq = httpTesting.expectOne('https://api.test.com/v1/stores?pageNumber=2&pageSize=5');
      expect(refreshReq.request.method).toBe('GET');
      refreshReq.flush({ ...mockSearchResult, pageNumber: 2, pageSize: 5 });
    });

    it('captures the latest list/query request and replays it', () => {
      store.loadPage({ pageNumber: 1, pageSize: 10 });
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/stores?pageNumber=1&pageSize=10');
      loadReq.flush(mockSearchResult);

      store.search({ params: { pageNumber: 3, pageSize: 20 }, body: { name: 'Newest' } });
      const searchReq = httpTesting.expectOne('https://api.test.com/v1/stores/search?pageNumber=3&pageSize=20');
      searchReq.flush({ ...mockSearchResult, pageNumber: 3, pageSize: 20 });

      store.refresh();
      const refreshReq = httpTesting.expectOne('https://api.test.com/v1/stores/search?pageNumber=3&pageSize=20');
      expect(refreshReq.request.method).toBe('POST');
      expect(refreshReq.request.body).toEqual({ name: 'Newest' });
      refreshReq.flush({ ...mockSearchResult, pageNumber: 3, pageSize: 20 });
    });

    it('does not issue a request when no prior list/query request exists', () => {
      store.refresh();
      httpTesting.expectNone((request) => request.url.includes('/v1/stores'));
    });
  });

  describe('applySignalrStoreUpdated', () => {
    it('patches an existing entity when payload contains a numeric id', () => {
      store.loadById(1);
      const req = httpTesting.expectOne('https://api.test.com/v1/stores/1');
      req.flush(mockStore);

      store.applySignalrStoreUpdated({ id: 1, name: 'Updated from event' });

      expect(store.entities()[0].name).toBe('Updated from event');
    });

   it('ignores malformed payloads (string id)', () => {
      store.loadById(1);
      const req = httpTesting.expectOne('https://api.test.com/v1/stores/1');
      req.flush(mockStore);

      store.applySignalrStoreUpdated({ id: '1', name: 'Should not apply' });

      expect(store.entities()[0].name).toBe('Test Store');
    });

   it('ignores payload with missing id', () => {
     store.loadById(1);
     const req = httpTesting.expectOne('https://api.test.com/v1/stores/1');
     req.flush(mockStore);

     store.applySignalrStoreUpdated({ name: 'No id' });

     expect(store.entities()[0].name).toBe('Test Store');
   });

   it('ignores null/undefined payload', () => {
     store.loadById(1);
     const req = httpTesting.expectOne('https://api.test.com/v1/stores/1');
     req.flush(mockStore);

     store.applySignalrStoreUpdated(null);
     store.applySignalrStoreUpdated(undefined);

     expect(store.entities()[0].name).toBe('Test Store');
   });

   it('ignores non-object payload', () => {
     store.loadById(1);
     const req = httpTesting.expectOne('https://api.test.com/v1/stores/1');
     req.flush(mockStore);

     store.applySignalrStoreUpdated(123);
     store.applySignalrStoreUpdated('string');
     store.applySignalrStoreUpdated([1,2,3]);

     expect(store.entities()[0].name).toBe('Test Store');
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
