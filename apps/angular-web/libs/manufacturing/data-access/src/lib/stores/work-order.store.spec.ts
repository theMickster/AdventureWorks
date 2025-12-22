import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { of, Subject, throwError } from 'rxjs';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { ApiEmptyResultError } from '@adventureworks-web/shared/data-access';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import type { WorkOrder } from '../models/work-order.model';
import { WorkOrderApiService } from '../services/work-order-api.service';
import { WorkOrderStore } from './work-order.store';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockWorkOrder: WorkOrder = {
  workOrderId: 72591,
  productId: 747,
  productName: 'HL Road Frame - Black, 58',
  orderedQty: 4,
  stockedQty: 4,
  scrappedQty: 0,
  yieldRate: 100,
  startDate: '2011-06-03T00:00:00',
  endDate: '2011-06-10T00:00:00',
  dueDate: '2011-06-10T00:00:00',
  isCompletedLate: false,
};

const mockSearchResult: SearchResult<WorkOrder> = {
  pageNumber: 1,
  pageSize: 25,
  totalPages: 1,
  totalRecords: 1,
  hasPreviousPage: false,
  hasNextPage: false,
  results: [mockWorkOrder],
};

describe('WorkOrderStore', () => {
  let store: InstanceType<typeof WorkOrderStore>;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        WorkOrderStore,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    });
    store = TestBed.inject(WorkOrderStore);
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

      const req = httpTesting.expectOne('https://api.test.com/v1/work-orders?pageNumber=1&pageSize=25');
      expect(req.request.method).toBe('GET');
      req.flush(mockSearchResult);

      expect(store.entities()).toEqual([mockWorkOrder]);
      expect(store.isLoaded()).toBe(true);
      expect(store.totalRecords()).toBe(1);
    });

    // Story #967's AC literally asks for `requestStatus() === 'failed'`. The real `RequestStatus`
    // union (`with-request-status.ts`) is `'idle' | 'loading' | 'loaded' | 'error'` — there is no
    // `'failed'` member. This store reuses the shared `withRequestStatus()` feature rather than
    // inventing a parallel status union for one lib, so this spec asserts against the real
    // `'error'` value, which is the terminal failure state the AC is describing.
    it('sets error state on failure', () => {
      store.loadPage({ pageNumber: 1, pageSize: 25 });
      const req = httpTesting.expectOne('https://api.test.com/v1/work-orders?pageNumber=1&pageSize=25');
      req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

      expect(store.requestStatus()).toBe('error');
      expect(store.hasError()).toBe(true);
      expect(store.error()).toBeTruthy();
    });

    it('treats ApiEmptyResultError as an empty loaded result, not an error', () => {
      const workOrderApi = TestBed.inject(WorkOrderApiService);
      vi.spyOn(workOrderApi, 'getWorkOrders').mockReturnValue(
        throwError(() => new ApiEmptyResultError('No results')),
      );

      store.loadPage({ pageNumber: 1, pageSize: 25 });

      expect(store.entities()).toEqual([]);
      expect(store.isLoaded()).toBe(true);
      expect(store.hasError()).toBe(false);
    });

    it('forwards all filter params to the work-orders API', () => {
      store.loadPage({
        pageNumber: 2,
        pageSize: 25,
        orderBy: 'dueDate',
        sortOrder: 'asc',
        productId: 747,
        startDate: '2011-01-01',
        endDate: '2011-12-31',
        hasScrapped: true,
        scrapReasonId: 7,
      });

      const req = httpTesting.expectOne(
        'https://api.test.com/v1/work-orders?pageNumber=2&pageSize=25&orderBy=dueDate&sortOrder=asc&productId=747&startDate=2011-01-01&endDate=2011-12-31&hasScrapped=true&scrapReasonId=7',
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockSearchResult);

      expect(store.isLoaded()).toBe(true);
    });

    it('switchMap cancellation: only the second call result populates state', () => {
      const workOrderApi = TestBed.inject(WorkOrderApiService);
      const firstSubject = new Subject<SearchResult<WorkOrder>>();
      const secondResult: SearchResult<WorkOrder> = {
        ...mockSearchResult,
        results: [{ ...mockWorkOrder, workOrderId: 99999 }],
      };

      let callCount = 0;
      vi.spyOn(workOrderApi, 'getWorkOrders').mockImplementation(() => {
        callCount++;
        if (callCount === 1) {
          // First call: slow observable that has not emitted yet
          return firstSubject.asObservable();
        }
        return of(secondResult);
      });

      store.loadPage({ pageNumber: 1, pageSize: 25 });
      store.loadPage({ pageNumber: 2, pageSize: 25 });

      // switchMap cancels the first in-flight call; only the second result populates state.
      expect(store.entities()).toEqual(secondResult.results);
      expect(store.isLoaded()).toBe(true);

      firstSubject.complete();
    });
  });

  describe('applyFilters', () => {
    // Does NOT override pageNumber, matching SalesOrderStore.applyFilters — callers are
    // responsible for passing the correct page (e.g. WorkOrderListComponent resets to 1 in the
    // URL on a genuine filter change, but must pass through whatever pageNumber the URL specifies
    // on first load).
    it('delegates to loadPage without overriding pageNumber', () => {
      const workOrderApi = TestBed.inject(WorkOrderApiService);
      vi.spyOn(workOrderApi, 'getWorkOrders').mockReturnValue(
        throwError(() => new ApiEmptyResultError('no results')),
      );

      store.applyFilters({ pageNumber: 4, pageSize: 25, productId: 747 });

      expect(workOrderApi.getWorkOrders).toHaveBeenCalledWith(
        expect.objectContaining({ pageNumber: 4, pageSize: 25, productId: 747 }),
      );
    });
  });
});
