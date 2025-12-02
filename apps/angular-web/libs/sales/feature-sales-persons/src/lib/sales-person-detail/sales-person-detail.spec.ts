import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { of, Subject, throwError } from 'rxjs';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { SalesPerson, SalesPersonPerformance } from '@adventureworks-web/sales/data-access';
import { SalesPersonDetailComponent } from './sales-person-detail';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

// BusinessEntityID=275 Michael Blythe — highest SalesYTD among top 3
const mockPerson: SalesPerson = {
  id: 275,
  title: null,
  firstName: 'Michael',
  middleName: null,
  lastName: 'Blythe',
  suffix: null,
  jobTitle: 'Sales Representative',
  emailAddress: 'michael.blythe@adventure-works.com',
  territoryId: 3,
  salesQuota: 250000,
  bonus: 4100,
  commissionPct: 0.012,
  salesYtd: 3763178.1787,
  territoryName: 'Central',
  modifiedDate: '2023-01-01T00:00:00',
};

// Performance data based on DB queries for BusinessEntityID=275
const mockPerformance: SalesPersonPerformance = {
  salesYtd: 3763178.1787,
  salesLastYear: 1750406.4674,
  salesQuota: 250000,
  bonus: 4100,
  commissionPct: 0.012,
  orderCount: 450,
  totalRevenue: 9293903.0024,
  quotaHistory: [
    { quotaDate: '2011-05-31T00:00:00', salesQuota: 28000 },
    { quotaDate: '2011-08-31T00:00:00', salesQuota: 7000 },
    { quotaDate: '2012-05-31T00:00:00', salesQuota: 250000 },
    { quotaDate: '2013-05-31T00:00:00', salesQuota: 300000 },
    { quotaDate: '2014-05-31T00:00:00', salesQuota: 250000 },
  ],
  territoryHistory: [
    { territoryId: 3, territoryName: 'Central', startDate: '2009-11-01T00:00:00', endDate: null },
  ],
};

function buildRoute(id = '275', queryParams: Record<string, string> = {}) {
  return {
    snapshot: {
      paramMap: { get: vi.fn().mockReturnValue(id) },
      queryParams,
    },
  };
}

describe('SalesPersonDetailComponent', () => {
  let component: SalesPersonDetailComponent;
  let fixture: ComponentFixture<SalesPersonDetailComponent>;
  let salesApiService: SalesApiService;
  let notificationService: NotificationService;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [SalesPersonDetailComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
        { provide: ActivatedRoute, useValue: route },
      ],
    }).compileComponents();

    salesApiService = TestBed.inject(SalesApiService);
    notificationService = TestBed.inject(NotificationService);
    router = TestBed.inject(Router);

    vi.spyOn(salesApiService, 'getSalesPerson').mockReturnValue(of(mockPerson));
    vi.spyOn(salesApiService, 'getSalesPersonPerformance').mockReturnValue(of(mockPerformance));
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(SalesPersonDetailComponent);
    component = fixture.componentInstance;
  });

  it("id='0' navigates to /sales/persons", () => {
    route.snapshot.paramMap.get = vi.fn().mockReturnValue('0');
    fixture.detectChanges();
    expect(router.navigate).toHaveBeenCalledWith(['/sales/persons']);
  });

  it("id='-1' navigates to /sales/persons", () => {
    route.snapshot.paramMap.get = vi.fn().mockReturnValue('-1');
    fixture.detectChanges();
    expect(router.navigate).toHaveBeenCalledWith(['/sales/persons']);
  });

  it("id='abc' (NaN) navigates to /sales/persons", () => {
    route.snapshot.paramMap.get = vi.fn().mockReturnValue('abc');
    fixture.detectChanges();
    expect(router.navigate).toHaveBeenCalledWith(['/sales/persons']);
  });

  it('getSalesPerson NOT called for invalid id', () => {
    route.snapshot.paramMap.get = vi.fn().mockReturnValue('0');
    fixture.detectChanges();
    expect(salesApiService.getSalesPerson).not.toHaveBeenCalled();
  });

  it('isLoading is true after ngOnInit, before response', () => {
    const subject = new Subject<SalesPerson>();
    vi.spyOn(salesApiService, 'getSalesPerson').mockReturnValue(subject.asObservable());
    fixture.detectChanges();
    expect(component['isLoading']()).toBe(true);
  });

  it('isLoading false, person() set after success', () => {
    fixture.detectChanges();
    expect(component['isLoading']()).toBe(false);
    expect(component['person']()).toEqual(mockPerson);
  });

  it('fullName() with null middleName returns "FirstName LastName"', () => {
    fixture.detectChanges();
    expect(component['fullName']()).toBe('Michael Blythe');
  });

  it('fullName() with non-null middleName returns "First M Last"', () => {
    const personWithMiddle: SalesPerson = { ...mockPerson, middleName: 'G' };
    vi.spyOn(salesApiService, 'getSalesPerson').mockReturnValue(of(personWithMiddle));
    fixture.detectChanges();
    expect(component['fullName']()).toBe('Michael G Blythe');
  });

  it('Person load error: isLoading false, NotificationService.error called with hardcoded message', () => {
    vi.spyOn(notificationService, 'error');
    vi.spyOn(salesApiService, 'getSalesPerson').mockReturnValue(throwError(() => new Error('Network error')));
    fixture.detectChanges();
    expect(component['isLoading']()).toBe(false);
    expect(notificationService.error).toHaveBeenCalledWith('Failed to load sales person. Please try again.');
  });

  it('getSalesPersonPerformance NOT called on initial load', () => {
    fixture.detectChanges();
    expect(salesApiService.getSalesPersonPerformance).not.toHaveBeenCalled();
  });

  it('First Performance tab click calls getSalesPersonPerformance(id) exactly once', () => {
    fixture.detectChanges();
    component['onTabChange']('performance');
    expect(salesApiService.getSalesPersonPerformance).toHaveBeenCalledTimes(1);
    expect(salesApiService.getSalesPersonPerformance).toHaveBeenCalledWith(275);
  });

  it('isLoadingPerformance true after tab click, before response', () => {
    const subject = new Subject<SalesPersonPerformance>();
    vi.spyOn(salesApiService, 'getSalesPersonPerformance').mockReturnValue(subject.asObservable());
    fixture.detectChanges();
    component['onTabChange']('performance');
    expect(component['isLoadingPerformance']()).toBe(true);
  });

  it('shows performance skeleton and not error state while loading', () => {
    const subject = new Subject<SalesPersonPerformance>();
    vi.spyOn(salesApiService, 'getSalesPersonPerformance').mockReturnValue(subject.asObservable());
    fixture.detectChanges();
    component['onTabChange']('performance');
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#aw-sales-person-detail-performance-loading')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('#aw-sales-person-detail-performance-error')).toBeNull();
  });

  it('isLoadingPerformance false, performance() set after success', () => {
    fixture.detectChanges();
    component['onTabChange']('performance');
    expect(component['isLoadingPerformance']()).toBe(false);
    expect(component['performance']()).toEqual(mockPerformance);
  });

  it('Second Performance tab click after success does NOT call getSalesPersonPerformance again', () => {
    fixture.detectChanges();
    component['onTabChange']('performance');
    component['onTabChange']('profile');
    component['onTabChange']('performance');
    expect(salesApiService.getSalesPersonPerformance).toHaveBeenCalledTimes(1);
  });

  it('Performance load error: hasPerformanceError() true, isLoadingPerformance() false, NotificationService.error called', () => {
    vi.spyOn(notificationService, 'error');
    vi.spyOn(salesApiService, 'getSalesPersonPerformance').mockReturnValue(throwError(() => new Error('fail')));
    fixture.detectChanges();
    component['onTabChange']('performance');
    expect(component['hasPerformanceError']()).toBe(true);
    expect(component['isLoadingPerformance']()).toBe(false);
    expect(notificationService.error).toHaveBeenCalledWith('Failed to load performance data. Please try again.');
  });

  it('After performance error, re-clicking Performance tab fires getSalesPersonPerformance again (retry)', () => {
    vi.spyOn(salesApiService, 'getSalesPersonPerformance').mockReturnValue(throwError(() => new Error('fail')));
    fixture.detectChanges();
    component['onTabChange']('performance');
    expect(salesApiService.getSalesPersonPerformance).toHaveBeenCalledTimes(1);

    // After error, performance() is null and isLoadingPerformance() is false → retry fires
    component['onTabChange']('profile');
    component['onTabChange']('performance');
    expect(salesApiService.getSalesPersonPerformance).toHaveBeenCalledTimes(2);
  });

  it('hasPerformanceError() resets to false when retry starts', () => {
    const errorSpy = vi.spyOn(salesApiService, 'getSalesPersonPerformance').mockReturnValue(
      throwError(() => new Error('fail')),
    );
    fixture.detectChanges();
    component['onTabChange']('performance');
    expect(component['hasPerformanceError']()).toBe(true);

    // On retry, use a subject that stays pending so we can check the reset before completion
    const subject = new Subject<SalesPersonPerformance>();
    errorSpy.mockReturnValue(subject.asObservable());
    component['onTabChange']('profile');
    component['onTabChange']('performance');
    expect(component['hasPerformanceError']()).toBe(false);
  });

  it('Null salesQuota: progress bar element is NOT in DOM', async () => {
    const perfNoQuota: SalesPersonPerformance = { ...mockPerformance, salesQuota: null };
    vi.spyOn(salesApiService, 'getSalesPersonPerformance').mockReturnValue(of(perfNoQuota));
    fixture.detectChanges();
    component['onTabChange']('performance');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const progressBar = fixture.nativeElement.querySelector('#aw-sales-person-detail-quota-progress');
    expect(progressBar).toBeNull();
  });

  it('Non-null salesQuota: progress bar element IS in DOM', async () => {
    fixture.detectChanges();
    component['onTabChange']('performance');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const progressBar = fixture.nativeElement.querySelector('#aw-sales-person-detail-quota-progress');
    expect(progressBar).not.toBeNull();
  });

  it('"View Orders" link is absent while isLoading() is true', () => {
    const subject = new Subject<SalesPerson>();
    vi.spyOn(salesApiService, 'getSalesPerson').mockReturnValue(subject.asObservable());
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('#aw-sales-person-detail-view-orders')).toBeNull();
  });

  it('"View Orders" link is present after successful person load', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('#aw-sales-person-detail-view-orders')).not.toBeNull();
  });

  it('"View Orders" link href is /sales/orders?salesPersonId=275', () => {
    fixture.detectChanges();
    const link = fixture.nativeElement.querySelector('#aw-sales-person-detail-view-orders') as HTMLAnchorElement;
    expect(link).not.toBeNull();
    expect(link.getAttribute('href')).toBe('/sales/orders?salesPersonId=275');
  });

  it('backQueryParams() round-trips search, orderBy, sortOrder from snapshot', () => {
    route.snapshot.queryParams = {
      search: 'blythe',
      orderBy: 'salesYtd',
      sortOrder: 'desc',
    };
    fixture.detectChanges();

    expect(component['backQueryParams']()).toEqual({
      search: 'blythe',
      orderBy: 'salesYtd',
      sortOrder: 'desc',
    });
  });
});
