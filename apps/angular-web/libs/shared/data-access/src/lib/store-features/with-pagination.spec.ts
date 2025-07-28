import { TestBed } from '@angular/core/testing';
import { patchState, signalStore } from '@ngrx/signals';
import { unprotected } from '@ngrx/signals/testing';
import { withPagination, setPaginationFromResult } from './with-pagination';
import { SearchResult } from '../models/search-result.model';

const TestStore = signalStore({ providedIn: 'root' }, withPagination(10));

const TestStore25 = signalStore({ providedIn: 'root' }, withPagination(25));

describe('withPagination', () => {
  let store: InstanceType<typeof TestStore>;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    store = TestBed.inject(TestStore);
  });

  it('should have correct initial state', () => {
    expect(store.pageNumber()).toBe(1);
    expect(store.pageSize()).toBe(10);
    expect(store.totalPages()).toBe(0);
    expect(store.totalRecords()).toBe(0);
  });

  it('should compute navigation flags as false initially', () => {
    expect(store.hasPreviousPage()).toBe(false);
    expect(store.hasNextPage()).toBe(false);
  });

  it('should accept custom default page size', () => {
    const store25 = TestBed.inject(TestStore25);
    expect(store25.pageSize()).toBe(25);
  });

  it('should update from search result', () => {
    const result: SearchResult<string> = {
      pageNumber: 2,
      pageSize: 10,
      totalPages: 5,
      totalRecords: 50,
      hasPreviousPage: true,
      hasNextPage: true,
      results: [],
    };

    patchState(unprotected(store), setPaginationFromResult(result));

    expect(store.pageNumber()).toBe(2);
    expect(store.totalPages()).toBe(5);
    expect(store.totalRecords()).toBe(50);
    expect(store.hasPreviousPage()).toBe(true);
    expect(store.hasNextPage()).toBe(true);
  });

  it('should compute false for single page', () => {
    const result: SearchResult<string> = {
      pageNumber: 1,
      pageSize: 10,
      totalPages: 1,
      totalRecords: 5,
      hasPreviousPage: false,
      hasNextPage: false,
      results: [],
    };

    patchState(unprotected(store), setPaginationFromResult(result));

    expect(store.hasPreviousPage()).toBe(false);
    expect(store.hasNextPage()).toBe(false);
  });

  it('should compute hasNextPage false on last page', () => {
    const result: SearchResult<string> = {
      pageNumber: 5,
      pageSize: 10,
      totalPages: 5,
      totalRecords: 50,
      hasPreviousPage: true,
      hasNextPage: false,
      results: [],
    };

    patchState(unprotected(store), setPaginationFromResult(result));

    expect(store.hasPreviousPage()).toBe(true);
    expect(store.hasNextPage()).toBe(false);
  });
});
