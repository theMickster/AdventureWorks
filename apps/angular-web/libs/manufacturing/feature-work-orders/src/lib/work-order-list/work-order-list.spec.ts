import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { BehaviorSubject } from 'rxjs';
import { patchState } from '@ngrx/signals';
import { setAllEntities } from '@ngrx/signals/entities';
import { unprotected } from '@ngrx/signals/testing';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { setError } from '@adventureworks-web/shared/data-access';
import { WorkOrderStore } from '@adventureworks-web/manufacturing/data-access';
import type { WorkOrder } from '@adventureworks-web/manufacturing/data-access';
import { StatusBadgeComponent } from '@adventureworks-web/shared/ui';
import { WorkOrderListComponent } from './work-order-list';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

let queryParamsSub: BehaviorSubject<Record<string, string>>;

const selectId = (o: WorkOrder) => o.workOrderId;

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

const lateWorkOrder: WorkOrder = {
  ...mockWorkOrder,
  workOrderId: 72592,
  endDate: '2011-06-15T00:00:00',
  dueDate: '2011-06-10T00:00:00',
  isCompletedLate: true,
};

function buildRoute(queryParams: Record<string, string> = {}) {
  queryParamsSub = new BehaviorSubject<Record<string, string>>(queryParams);
  return {
    queryParams: queryParamsSub,
  };
}

describe('WorkOrderListComponent', () => {
  let component: WorkOrderListComponent;
  let fixture: ComponentFixture<WorkOrderListComponent>;
  let workOrderStore: InstanceType<typeof WorkOrderStore>;
  let notificationService: NotificationService;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [WorkOrderListComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    workOrderStore = TestBed.inject(WorkOrderStore);
    notificationService = TestBed.inject(NotificationService);
    router = TestBed.inject(Router);

    vi.spyOn(workOrderStore, 'loadPage');
    vi.spyOn(workOrderStore, 'applyFilters');
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(WorkOrderListComponent);
    component = fixture.componentInstance;
  });

  it('renders without errors', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('loads page 1 with the default startDate desc sort when no URL params (AC1)', () => {
    fixture.detectChanges();

    expect(workOrderStore.applyFilters).toHaveBeenCalledWith({
      pageNumber: 1,
      pageSize: 25,
      orderBy: 'startDate',
      sortOrder: 'desc',
    });
  });

  it('respects a pageNumber > 1 on the first queryParams emission (no filters)', () => {
    queryParamsSub.next({ pageNumber: '5' });

    fixture.detectChanges();

    expect(workOrderStore.applyFilters).toHaveBeenCalledWith({
      pageNumber: 5,
      pageSize: 25,
      orderBy: 'startDate',
      sortOrder: 'desc',
    });
  });

  it('respects a pageNumber > 1 on the first queryParams emission alongside a filter', () => {
    queryParamsSub.next({ pageNumber: '5', productId: '747' });

    fixture.detectChanges();

    expect(workOrderStore.applyFilters).toHaveBeenCalledWith({
      pageNumber: 5,
      pageSize: 25,
      orderBy: 'startDate',
      sortOrder: 'desc',
      productId: 747,
    });
  });

  it('restores productId and scrapReasonId filters from the URL and forwards them to applyFilters (AC2)', () => {
    queryParamsSub.next({ productId: '747', scrapReasonId: '7' });

    fixture.detectChanges();

    expect(workOrderStore.applyFilters).toHaveBeenCalledWith({
      pageNumber: 1,
      pageSize: 25,
      orderBy: 'startDate',
      sortOrder: 'desc',
      productId: 747,
      scrapReasonId: 7,
    });
  });

  it('restores startDate/endDate/hasScrapped filters from the URL and forwards them to applyFilters', () => {
    queryParamsSub.next({ startDate: '2011-01-01', endDate: '2011-12-31', hasScrapped: 'true' });

    fixture.detectChanges();

    expect(workOrderStore.applyFilters).toHaveBeenCalledWith({
      pageNumber: 1,
      pageSize: 25,
      orderBy: 'startDate',
      sortOrder: 'desc',
      startDate: '2011-01-01',
      endDate: '2011-12-31',
      hasScrapped: true,
    });
    expect(component['filterForm'].getRawValue()).toMatchObject({
      startDate: '2011-01-01',
      endDate: '2011-12-31',
      hasScrapped: 'true',
    });
  });

  it('treats hasScrapped=false as an explicit filter, distinct from unset ("All")', () => {
    queryParamsSub.next({ hasScrapped: 'false' });

    fixture.detectChanges();

    expect(workOrderStore.applyFilters).toHaveBeenCalledWith(
      expect.objectContaining({ hasScrapped: false }),
    );
  });

  it('applies a date-range and scrap-status filter and writes them back to the URL with merge', () => {
    fixture.detectChanges();
    component['filterForm'].patchValue({ startDate: '2011-01-01', endDate: '2011-12-31', hasScrapped: 'true' });

    component['onApplyFilters']();

    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        queryParams: expect.objectContaining({
          startDate: '2011-01-01',
          endDate: '2011-12-31',
          hasScrapped: 'true',
          pageNumber: 1,
        }),
        queryParamsHandling: 'merge',
      }),
    );
  });

  it('drops a non-numeric productId URL param instead of forwarding NaN', () => {
    queryParamsSub.next({ productId: 'abc' });
    fixture.detectChanges();
    const lastCall = (workOrderStore.applyFilters as ReturnType<typeof vi.spyOn>).mock.calls.at(-1)?.[0] as Record<
      string,
      unknown
    >;
    expect('productId' in lastCall).toBe(false);
  });

  it('clamps junk pageNumber from the URL to 1', () => {
    queryParamsSub.next({ pageNumber: '-3' });

    fixture.detectChanges();

    expect(workOrderStore.applyFilters).toHaveBeenCalledWith(expect.objectContaining({ pageNumber: 1 }));
  });

  it('ignores an invalid orderBy from the URL and falls back to the default sort', () => {
    queryParamsSub.next({ orderBy: 'dropTable', sortOrder: 'asc' });

    fixture.detectChanges();

    expect(workOrderStore.applyFilters).toHaveBeenCalledWith(
      expect.objectContaining({ orderBy: 'startDate', sortOrder: 'desc' }),
    );
  });

  it('applies a filter, resets to page 1, and writes back to the URL with merge', () => {
    fixture.detectChanges();
    component['filterForm'].patchValue({ productId: '747' });

    component['onApplyFilters']();
    queryParamsSub.next({ productId: '747', pageNumber: '1' });
    fixture.detectChanges();

    expect(workOrderStore.applyFilters).toHaveBeenLastCalledWith(
      expect.objectContaining({ pageNumber: 1, pageSize: 25, productId: 747 }),
    );
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        queryParams: expect.objectContaining({ productId: 747, pageNumber: 1 }),
        queryParamsHandling: 'merge',
      }),
    );
  });

  it('omits a cleared filter from the API call and nulls its URL param', () => {
    fixture.detectChanges();

    component['filterForm'].patchValue({ productId: '747' });
    component['filterForm'].patchValue({ productId: '' });

    component['onApplyFilters']();
    queryParamsSub.next({ pageNumber: '1' });
    fixture.detectChanges();

    const lastCall = (workOrderStore.applyFilters as ReturnType<typeof vi.spyOn>).mock.calls.at(-1)?.[0] as Record<
      string,
      unknown
    >;
    expect('productId' in lastCall).toBe(false);
    expect('scrapReasonId' in lastCall).toBe(false);

    expect(router.navigate).toHaveBeenLastCalledWith(
      [],
      expect.objectContaining({
        queryParams: {
          productId: null,
          scrapReasonId: null,
          startDate: null,
          endDate: null,
          hasScrapped: null,
          pageNumber: 1,
        },
        queryParamsHandling: 'merge',
      }),
    );
  });

  it('resets all filters and sort, nulls URL params, and reloads the default view', () => {
    queryParamsSub.next({ productId: '747', orderBy: 'dueDate', sortOrder: 'asc' });
    fixture.detectChanges();

    component['onResetFilters']();
    queryParamsSub.next({});
    fixture.detectChanges();

    expect(workOrderStore.applyFilters).toHaveBeenLastCalledWith({
      pageNumber: 1,
      pageSize: 25,
      orderBy: 'startDate',
      sortOrder: 'desc',
    });
    expect(component['sortColumn']()).toBe('');
    expect(component['filterForm'].getRawValue()).toEqual({
      productId: '',
      scrapReasonId: '',
      startDate: '',
      endDate: '',
      hasScrapped: '',
    });
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({
        queryParams: {
          productId: null,
          scrapReasonId: null,
          startDate: null,
          endDate: null,
          hasScrapped: null,
          pageNumber: null,
          orderBy: null,
          sortOrder: null,
        },
        queryParamsHandling: 'merge',
      }),
    );
  });

  it('sorts on an allowed column and writes the sort to the URL', () => {
    fixture.detectChanges();

    component['onSortChange']({ column: 'dueDate', direction: 'asc' });
    queryParamsSub.next({ orderBy: 'dueDate', sortOrder: 'asc' });
    fixture.detectChanges();

    expect(workOrderStore.loadPage).toHaveBeenLastCalledWith(
      expect.objectContaining({ orderBy: 'dueDate', sortOrder: 'asc' }),
    );
  });

  it('does not call the store for a sort on a disallowed column key', () => {
    fixture.detectChanges();
    const loadPageCallsBefore = (workOrderStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.length;
    const applyFiltersCallsBefore = (workOrderStore.applyFilters as ReturnType<typeof vi.spyOn>).mock.calls.length;

    component['onSortChange']({ column: 'productName', direction: 'asc' });

    expect((workOrderStore.loadPage as ReturnType<typeof vi.spyOn>).mock.calls.length).toBe(loadPageCallsBefore);
    expect((workOrderStore.applyFilters as ReturnType<typeof vi.spyOn>).mock.calls.length).toBe(applyFiltersCallsBefore);
  });

  it('parses filters and a new page from the URL on page change (AC4 pagination)', () => {
    queryParamsSub.next({ productId: '747', pageNumber: '1' });
    fixture.detectChanges();

    component['onPageChange'](3);
    queryParamsSub.next({ productId: '747', pageNumber: '3' });
    fixture.detectChanges();

    expect(workOrderStore.loadPage).toHaveBeenLastCalledWith({
      pageNumber: 3,
      pageSize: 25,
      orderBy: 'startDate',
      sortOrder: 'desc',
      productId: 747,
    });
  });

  it('navigates to the work order detail on View button click', () => {
    fixture.detectChanges();

    component['onViewClick']({ workOrderId: 72591 });

    expect(router.navigate).toHaveBeenCalledWith(['/manufacturing/work-orders', 72591]);
  });

  it('shows an error toast when the store reports an error (AC error state)', async () => {
    vi.spyOn(notificationService, 'error');
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(workOrderStore), setError('load failed'));
    fixture.detectChanges();
    await fixture.whenStable();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to load work orders. Please try again.');
  });

  it('projects a blank statusKey for an on-time order and "completed late" for a late one (AC3 status)', () => {
    fixture.detectChanges();

    patchState(unprotected(workOrderStore), setAllEntities([mockWorkOrder, lateWorkOrder], { selectId }));
    fixture.detectChanges();

    const rows = component['rows']();
    expect(rows).toHaveLength(2);
    expect(rows[0]).toMatchObject({ workOrderId: 72591, statusKey: '' });
    expect(rows[1]).toMatchObject({ workOrderId: 72592, statusKey: 'completed late' });
  });

  it('projects a blank endDate for a still-in-progress work order', () => {
    fixture.detectChanges();

    const inProgress: WorkOrder = { ...mockWorkOrder, endDate: null };
    patchState(unprotected(workOrderStore), setAllEntities([inProgress], { selectId }));
    fixture.detectChanges();

    const rows = component['rows']();
    expect(rows[0]['endDate']).toBe('');
  });

  it('resolves the "completed late" badge through the real StatusBadgeComponent', () => {
    fixture.detectChanges();
    const map = component['statusBadgeMap'];

    const badgeFixture = TestBed.createComponent(StatusBadgeComponent);
    badgeFixture.componentRef.setInput('status', 'completed late');
    badgeFixture.componentRef.setInput('statusMap', map);
    badgeFixture.detectChanges();
    const span = badgeFixture.nativeElement.querySelector('span') as HTMLElement;
    expect(span.className).toBe('badge badge-warning');
  });

  it('shows an empty grid when the store loads zero results (AC5 empty state)', () => {
    fixture.detectChanges();

    patchState(unprotected(workOrderStore), setAllEntities([] as WorkOrder[], { selectId }));
    fixture.detectChanges();

    expect(component['rows']()).toEqual([]);
  });
});
