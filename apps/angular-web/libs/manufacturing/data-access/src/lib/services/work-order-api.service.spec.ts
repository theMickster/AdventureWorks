import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import type { WorkOrder } from '../models/work-order.model';
import type { WorkOrderDetail } from '../models/work-order-detail.model';
import { WorkOrderApiService } from './work-order-api.service';

const mockEnvironment = {
  production: false,
  defaultLocale: 'en',
  api: {
    primary: { baseUrl: 'https://localhost:44369/api', name: 'Test API' },
  },
};

const BASE_URL = 'https://localhost:44369/api';

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

describe('WorkOrderApiService', () => {
  let service: WorkOrderApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: ENVIRONMENT, useValue: mockEnvironment }],
    });
    service = TestBed.inject(WorkOrderApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should be injectable', () => {
    expect(service).toBeTruthy();
  });

  it('should GET work orders without params', () => {
    const mockData: SearchResult<WorkOrder> = {
      pageNumber: 1,
      pageSize: 25,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
      totalRecords: 1,
      results: [mockWorkOrder],
    };

    service.getWorkOrders().subscribe((result) => {
      expect(result).toEqual(mockData);
    });

    const req = httpTesting.expectOne(`${BASE_URL}/v1/work-orders`);
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should GET work orders with pagination and sort params', () => {
    const mockData: SearchResult<WorkOrder> = {
      pageNumber: 2,
      pageSize: 25,
      totalPages: 3,
      hasPreviousPage: true,
      hasNextPage: true,
      totalRecords: 60,
      results: [],
    };

    service.getWorkOrders({ pageNumber: 2, pageSize: 25, orderBy: 'dueDate', sortOrder: 'asc' }).subscribe((result) => {
      expect(result).toEqual(mockData);
    });

    const req = httpTesting.expectOne(`${BASE_URL}/v1/work-orders?pageNumber=2&pageSize=25&orderBy=dueDate&sortOrder=asc`);
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should GET work orders with all filter params in the query string', () => {
    const mockData: SearchResult<WorkOrder> = {
      pageNumber: 1,
      pageSize: 25,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
      totalRecords: 1,
      results: [mockWorkOrder],
    };

    service
      .getWorkOrders({
        pageNumber: 1,
        pageSize: 25,
        orderBy: 'workOrderId',
        sortOrder: 'asc',
        productId: 747,
        startDate: '2011-01-01',
        endDate: '2011-12-31',
        hasScrapped: true,
        scrapReasonId: 7,
      })
      .subscribe((result) => {
        expect(result).toEqual(mockData);
      });

    const req = httpTesting.expectOne(
      `${BASE_URL}/v1/work-orders?pageNumber=1&pageSize=25&orderBy=workOrderId&sortOrder=asc&productId=747&startDate=2011-01-01&endDate=2011-12-31&hasScrapped=true&scrapReasonId=7`,
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should GET a single work order by id', () => {
    // Real fixture: WorkOrderID 41, ProductID 518 (ML Road Seat Assembly), ScrapReasonID 7 (Handling damage)
    const mockDetail: WorkOrderDetail = {
      workOrderId: 41,
      productId: 518,
      productName: 'ML Road Seat Assembly',
      orderedQty: 98,
      stockedQty: 97,
      scrappedQty: 1,
      yieldRate: 98.98,
      startDate: '2011-06-03T00:00:00',
      endDate: '2011-06-19T00:00:00',
      dueDate: '2011-06-14T00:00:00',
      isCompletedLate: true,
      daysLate: 5,
      scrapReasonId: 7,
      scrapReasonName: 'Handling damage',
    };

    service.getWorkOrder(41).subscribe((result) => {
      expect(result).toEqual(mockDetail);
    });

    const req = httpTesting.expectOne(`${BASE_URL}/v1/work-orders/41`);
    expect(req.request.method).toBe('GET');
    req.flush(mockDetail);
  });

  it('should pass through a 404 error when the work order does not exist', () => {
    // Confirmed absent via database query
    let capturedStatus: number | undefined;
    service.getWorkOrder(9999999).subscribe({
      next: () => expect.fail('expected an error'),
      error: (err) => {
        capturedStatus = err.status;
      },
    });

    const req = httpTesting.expectOne(`${BASE_URL}/v1/work-orders/9999999`);
    req.flush('Not Found', { status: 404, statusText: 'Not Found' });

    expect(capturedStatus).toBe(404);
  });
});
