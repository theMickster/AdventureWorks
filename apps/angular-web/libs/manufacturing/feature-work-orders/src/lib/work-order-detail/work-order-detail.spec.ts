import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { HttpErrorResponse } from '@angular/common/http';
import { of, Subject, throwError } from 'rxjs';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { WorkOrderApiService } from '@adventureworks-web/manufacturing/data-access';
import type { WorkOrderDetail } from '@adventureworks-web/manufacturing/data-access';
import { WorkOrderDetailComponent } from './work-order-detail';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

// Real fixture: WorkOrderID 41, ProductID 518 (ML Road Seat Assembly), ScrapReasonID 7 (Handling damage)
const mockWorkOrder: WorkOrderDetail = {
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

// Real fixture: WorkOrderID 1, ProductID 722 (LL Road Frame - Black, 58), on-time, no scrap
const mockOnTimeWorkOrder: WorkOrderDetail = {
  workOrderId: 1,
  productId: 722,
  productName: 'LL Road Frame - Black, 58',
  orderedQty: 8,
  stockedQty: 8,
  scrappedQty: 0,
  yieldRate: 100,
  startDate: '2011-06-03T00:00:00',
  endDate: '2011-06-13T00:00:00',
  dueDate: '2011-06-14T00:00:00',
  isCompletedLate: false,
  daysLate: null,
  scrapReasonId: null,
  scrapReasonName: null,
};

function buildRoute(id = '41') {
  return {
    snapshot: {
      paramMap: { get: vi.fn().mockReturnValue(id) },
    },
  };
}

describe('WorkOrderDetailComponent', () => {
  let component: WorkOrderDetailComponent;
  let fixture: ComponentFixture<WorkOrderDetailComponent>;
  let workOrderApiService: WorkOrderApiService;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [WorkOrderDetailComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    workOrderApiService = TestBed.inject(WorkOrderApiService);

    vi.spyOn(workOrderApiService, 'getWorkOrder').mockReturnValue(of(mockWorkOrder));
    vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(WorkOrderDetailComponent);
    component = fixture.componentInstance;
  });

  it('renders work order id from API response', () => {
    fixture.detectChanges();

    const el = fixture.nativeElement.querySelector('#aw-work-order-detail-work-order-id') as HTMLElement;
    expect(el.textContent).toContain('41');
  });

  it('shows loading skeleton before API resolves', () => {
    const subject = new Subject<WorkOrderDetail>();
    vi.spyOn(workOrderApiService, 'getWorkOrder').mockReturnValue(subject.asObservable());

    fixture.detectChanges();

    expect(component['isLoading']()).toBe(true);
    const skeleton = fixture.nativeElement.querySelector('#aw-work-order-detail-loading') as HTMLElement;
    expect(skeleton).toBeTruthy();

    subject.next(mockWorkOrder);
    subject.complete();
    fixture.detectChanges();

    expect(component['isLoading']()).toBe(false);
  });

  it('renders the product link with correct href and text', () => {
    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('#aw-work-order-detail-product-link') as HTMLAnchorElement;
    expect(link).toBeTruthy();
    expect(link.textContent?.trim()).toBe('ML Road Seat Assembly');
    expect(link.getAttribute('href')).toBe('/products/518');
  });

  it('renders the completed-late badge and days-late text when completed late', () => {
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('#aw-work-order-detail-status') as HTMLElement;
    const daysLate = fixture.nativeElement.querySelector('#aw-work-order-detail-days-late') as HTMLElement;
    expect(badge).toBeTruthy();
    expect(badge.classList.contains('badge-error')).toBe(true);
    expect(daysLate.textContent).toContain('5 days late');
  });

  it('does not render the completed-late badge when on time', () => {
    vi.spyOn(workOrderApiService, 'getWorkOrder').mockReturnValue(of(mockOnTimeWorkOrder));
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('#aw-work-order-detail-status');
    const daysLate = fixture.nativeElement.querySelector('#aw-work-order-detail-days-late');
    expect(badge).toBeNull();
    expect(daysLate).toBeNull();
  });

  it('renders metric tile values', () => {
    fixture.detectChanges();

    const ordered = fixture.nativeElement.querySelector('#aw-work-order-detail-ordered-qty-body') as HTMLElement;
    const stocked = fixture.nativeElement.querySelector('#aw-work-order-detail-stocked-qty-body') as HTMLElement;
    const scrapped = fixture.nativeElement.querySelector('#aw-work-order-detail-scrapped-qty-body') as HTMLElement;
    const yieldRate = fixture.nativeElement.querySelector('#aw-work-order-detail-yield-rate-body') as HTMLElement;

    expect(ordered.textContent).toContain('98');
    expect(stocked.textContent).toContain('97');
    expect(scrapped.textContent).toContain('1');
    expect(yieldRate.textContent).toContain('98.98%');
  });

  it('renders a dash for end date when null', () => {
    vi.spyOn(workOrderApiService, 'getWorkOrder').mockReturnValue(of({ ...mockWorkOrder, endDate: null }));
    fixture.detectChanges();

    const endDate = fixture.nativeElement.querySelector('#aw-work-order-detail-end-date') as HTMLElement;
    expect(endDate.textContent?.trim()).toBe('—');
  });

  it('renders the scrap reason row when scrapReasonName is present', () => {
    fixture.detectChanges();

    const scrapReason = fixture.nativeElement.querySelector('#aw-work-order-detail-scrap-reason-name') as HTMLElement;
    expect(scrapReason).toBeTruthy();
    expect(scrapReason.textContent).toContain('Handling damage');
  });

  it('omits the scrap reason row when scrapReasonName is absent', () => {
    vi.spyOn(workOrderApiService, 'getWorkOrder').mockReturnValue(of(mockOnTimeWorkOrder));
    fixture.detectChanges();

    const scrapReason = fixture.nativeElement.querySelector('#aw-work-order-detail-scrap-reason');
    expect(scrapReason).toBeNull();
  });

  it('shows not-found state on 404 response from API', () => {
    vi.spyOn(workOrderApiService, 'getWorkOrder').mockReturnValue(throwError(() => new HttpErrorResponse({ status: 404 })));
    fixture.detectChanges();

    expect(component['notFound']()).toBe(true);
    const notFoundEl = fixture.nativeElement.querySelector('#aw-work-order-detail-not-found') as HTMLElement;
    expect(notFoundEl).toBeTruthy();
  });

  it('shows error state and notifies on non-404 error response from API', () => {
    const notificationService = TestBed.inject(NotificationService);
    const errorSpy = vi.spyOn(notificationService, 'error');
    vi.spyOn(workOrderApiService, 'getWorkOrder').mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500 })));

    fixture.detectChanges();

    expect(component['hasError']()).toBe(true);
    const errorEl = fixture.nativeElement.querySelector('#aw-work-order-detail-error') as HTMLElement;
    expect(errorEl).toBeTruthy();
    expect(errorSpy).toHaveBeenCalledWith('Failed to load work order. Please try again.');
  });

  it('navigates away when the route id is invalid', async () => {
    TestBed.resetTestingModule();
    route = buildRoute('not-a-number');

    await TestBed.configureTestingModule({
      imports: [WorkOrderDetailComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    vi.spyOn(TestBed.inject(WorkOrderApiService), 'getWorkOrder').mockReturnValue(of(mockWorkOrder));
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(WorkOrderDetailComponent);
    fixture.detectChanges();

    expect(router.navigate).toHaveBeenCalledWith(['/manufacturing/work-orders']);
  });
});
