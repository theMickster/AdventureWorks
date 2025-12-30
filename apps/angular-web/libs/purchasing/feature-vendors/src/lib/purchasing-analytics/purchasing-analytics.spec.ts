import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { Subject, of, throwError } from 'rxjs';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { PurchasingApiService } from '@adventureworks-web/purchasing/data-access';
import type { PurchasingAnalyticsDto } from '@adventureworks-web/purchasing/data-access';
import { PurchasingAnalyticsComponent } from './purchasing-analytics';

// Chart.js is mocked because ParetoChartComponent renders inside this page's template — the real
// library needs a live canvas 2D context. No assertion here touches the Chart constructor.
vi.mock('chart.js', () => {
  const Chart = vi.fn().mockImplementation(function () {
    return { destroy: vi.fn() };
  });
  (Chart as unknown as { register: () => void }).register = vi.fn();
  return {
    Chart,
    BarController: {},
    BarElement: {},
    LineController: {},
    LineElement: {},
    PointElement: {},
    LinearScale: {},
    CategoryScale: {},
    Tooltip: {},
  };
});

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

// Real AdventureWorks vendors, spend-descending, plus the four always-present pipeline statuses.
const mockAnalytics: PurchasingAnalyticsDto = {
  paretoData: [
    { vendorId: 1576, vendorName: 'Superior Bicycles', totalSpend: 5034266.74, cumulativePercent: 6.1 },
    { vendorId: 1602, vendorName: 'Vision Cycles, Inc', totalSpend: 4894060.4, cumulativePercent: 12.0 },
    { vendorId: 1650, vendorName: 'American Bicycles and Wheels', totalSpend: 4520000.12, cumulativePercent: 100 },
  ],
  pipelineSummary: [
    { statusLabel: 'Pending', poCount: 4, totalValue: 1250.5 },
    { statusLabel: 'Approved', poCount: 12, totalValue: 98000.25 },
    { statusLabel: 'Rejected', poCount: 0, totalValue: 0 },
    { statusLabel: 'Complete', poCount: 3996, totalValue: 82000000 },
  ],
};

describe('PurchasingAnalyticsComponent', () => {
  let component: PurchasingAnalyticsComponent;
  let fixture: ComponentFixture<PurchasingAnalyticsComponent>;
  let purchasingApiService: PurchasingApiService;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PurchasingAnalyticsComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    }).compileComponents();

    purchasingApiService = TestBed.inject(PurchasingApiService);
    router = TestBed.inject(Router);

    vi.spyOn(purchasingApiService, 'getPurchasingAnalytics').mockReturnValue(of(mockAnalytics));
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(PurchasingAnalyticsComponent);
    component = fixture.componentInstance;
  });

  it('shows loading skeletons before the API resolves', () => {
    const subject = new Subject<PurchasingAnalyticsDto>();
    vi.spyOn(purchasingApiService, 'getPurchasingAnalytics').mockReturnValue(subject.asObservable());

    fixture.detectChanges();

    expect(component['isLoading']()).toBe(true);
    expect(fixture.nativeElement.querySelector('#aw-purchasing-analytics-loading')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-purchasing-analytics-chart-skeleton')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-purchasing-analytics-tile-skeleton-4')).toBeTruthy();

    subject.next(mockAnalytics);
    subject.complete();
    fixture.detectChanges();

    expect(component['isLoading']()).toBe(false);
    expect(fixture.nativeElement.querySelector('#aw-purchasing-analytics-loading')).toBeNull();
  });

  it('renders the chart host and the pareto chart on success', () => {
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#aw-purchasing-analytics-chart')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('aw-pareto-chart canvas')).toBeTruthy();
  });

  it('renders the four pipeline tiles with counts and values', () => {
    fixture.detectChanges();

    const pending = fixture.nativeElement.querySelector('#aw-purchasing-analytics-pending-count') as HTMLElement;
    const approved = fixture.nativeElement.querySelector('#aw-purchasing-analytics-approved-count') as HTMLElement;
    const rejected = fixture.nativeElement.querySelector('#aw-purchasing-analytics-rejected-count') as HTMLElement;
    const complete = fixture.nativeElement.querySelector('#aw-purchasing-analytics-complete-count') as HTMLElement;
    const pendingValue = fixture.nativeElement.querySelector('#aw-purchasing-analytics-pending-value') as HTMLElement;

    expect(pending.textContent).toContain('4');
    expect(approved.textContent).toContain('12');
    expect(rejected.textContent).toContain('0');
    expect(complete.textContent).toContain('3996');
    expect(pendingValue.textContent).toContain('1,250.50');
  });

  it('renders the pipeline tiles in the API response order, trusting the server-guaranteed order rather than re-deriving it', () => {
    const reversed = [...mockAnalytics.pipelineSummary].reverse();
    vi.spyOn(purchasingApiService, 'getPurchasingAnalytics').mockReturnValue(
      of({ ...mockAnalytics, pipelineSummary: reversed }),
    );

    fixture.detectChanges();

    const tileElements = (fixture.nativeElement as HTMLElement).querySelectorAll<HTMLElement>(
      '[id^="aw-purchasing-analytics-"][id$="-tile"]',
    );
    const tileIds = Array.from(tileElements).map((el) => el.id);

    expect(tileIds).toEqual(
      reversed.map((item) => `aw-purchasing-analytics-${item.statusLabel.toLowerCase()}-tile`),
    );
  });

  it('shows an error empty-state when the API fails', () => {
    vi.spyOn(purchasingApiService, 'getPurchasingAnalytics').mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 })),
    );

    fixture.detectChanges();

    expect(component['hasError']()).toBe(true);
    expect(fixture.nativeElement.querySelector('#aw-purchasing-analytics-error')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-purchasing-analytics-pipeline')).toBeNull();
  });

  it('still renders the pipeline tiles when paretoData is empty, delegating the empty chart to the chart component', () => {
    vi.spyOn(purchasingApiService, 'getPurchasingAnalytics').mockReturnValue(of({ ...mockAnalytics, paretoData: [] }));

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#aw-purchasing-analytics-pipeline')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-purchasing-analytics-complete-count')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('aw-pareto-chart canvas')).toBeNull();
    expect(fixture.nativeElement.querySelector('#aw-pareto-chart-empty')).toBeTruthy();
  });

  it('navigates to the vendor detail page when the chart emits a vendorClick', () => {
    fixture.detectChanges();

    component['onVendorClick'](1602);

    expect(router.navigate).toHaveBeenCalledWith(['/purchasing/vendors', 1602]);
  });
});
