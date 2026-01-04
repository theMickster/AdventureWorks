import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { Subject, of, throwError } from 'rxjs';
import { WorkOrderApiService } from '@adventureworks-web/manufacturing/data-access';
import type {
  ManufacturingKpisDto,
  ManufacturingQualityScorecard,
} from '@adventureworks-web/manufacturing/data-access';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { ManufacturingKpiDashboardComponent } from './manufacturing-kpi-dashboard';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockKpis: ManufacturingKpisDto = {
  totalWorkOrders: 72591,
  totalOrdered: 100000,
  totalStocked: 99000,
  totalScrapped: 1000,
  overallYieldPct: 99,
  overallScrapPct: 1,
};

const mockScorecard: ManufacturingQualityScorecard = {
  top5ByScrapped: [
    {
      productId: 747,
      productName: 'HL Road Frame - Black, 58',
      orderedQty: 100,
      stockedQty: 90,
      scrappedQty: 10,
      yieldPct: 90,
      scrapPct: 10,
    },
  ],
  bottom5ByYield: [
    {
      productId: 518,
      productName: 'ML Road Seat Assembly',
      orderedQty: 100,
      stockedQty: 80,
      scrappedQty: 20,
      yieldPct: 80,
      scrapPct: 20,
    },
  ],
  scrapReasonBreakdown: [
    {
      scrapReasonId: 7,
      scrapReasonName: 'Handling damage',
      scrappedQty: 20,
      scrapPct: 100,
    },
  ],
};

describe('ManufacturingKpiDashboardComponent', () => {
  let component: ManufacturingKpiDashboardComponent;
  let fixture: ComponentFixture<ManufacturingKpiDashboardComponent>;
  let workOrderApi: WorkOrderApiService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManufacturingKpiDashboardComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    }).compileComponents();

    workOrderApi = TestBed.inject(WorkOrderApiService);
    vi.spyOn(workOrderApi, 'getKpis').mockReturnValue(of(mockKpis));
    vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(of(mockScorecard));

    fixture = TestBed.createComponent(ManufacturingKpiDashboardComponent);
    component = fixture.componentInstance;
  });

  it('shows loading skeletons before the API resolves', () => {
    const subject = new Subject<ManufacturingKpisDto>();
    vi.spyOn(workOrderApi, 'getKpis').mockReturnValue(subject.asObservable());

    fixture.detectChanges();

    expect(component['isLoading']()).toBe(true);
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-dashboard-loading')).toBeTruthy();
    expect(fixture.nativeElement.querySelectorAll('aw-skeleton')).toHaveLength(4);

    subject.next(mockKpis);
    subject.complete();
    fixture.detectChanges();

    expect(component['isLoading']()).toBe(false);
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-dashboard-loading')).toBeNull();
  });

  it('renders all KPI tiles on success', () => {
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-total-work-orders-value').textContent).toContain(
      '72591',
    );
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-yield-rate-value').textContent).toContain('99%');
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-scrap-rate-value').textContent).toContain('1%');
    expect(
      fixture.nativeElement.querySelector('#aw-manufacturing-kpi-total-units-scrapped-value').textContent,
    ).toContain('1000');
  });

  it('loads the scorecard once and shares its collections across all sections', () => {
    fixture.detectChanges();

    expect(workOrderApi.getQualityScorecard).toHaveBeenCalledTimes(1);
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-quality-scorecard-top-scrapped')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-quality-scorecard-bottom-yield')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-quality-scorecard-scrap-reasons')).toBeTruthy();
    expect(fixture.nativeElement.querySelectorAll('#aw-manufacturing-quality-scorecard tbody tr')).toHaveLength(3);
  });

  it('renders product and scrap-reason links with existing work-order filters', () => {
    fixture.detectChanges();

    const links = [...fixture.nativeElement.querySelectorAll('#aw-manufacturing-quality-scorecard a')].map(
      (link: HTMLAnchorElement) => link.getAttribute('href'),
    );

    expect(links).toContain('/manufacturing/work-orders?productId=747');
    expect(links).toContain('/manufacturing/work-orders?productId=518');
    expect(links).toContain('/manufacturing/work-orders?scrapReasonId=7');
  });

  it('shows the error empty state when the KPI API fails', () => {
    vi.spyOn(workOrderApi, 'getKpis').mockReturnValue(throwError(() => new Error('request failed')));

    fixture.detectChanges();

    expect(component['hasError']()).toBe(true);
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-dashboard-error')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-dashboard-error').textContent).toContain(
      'Manufacturing data unavailable',
    );
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-dashboard-tiles')).toBeNull();
  });

  it('renders zero values when the KPI API returns no orders', () => {
    vi.spyOn(workOrderApi, 'getKpis').mockReturnValue(
      of({
        ...mockKpis,
        totalWorkOrders: 0,
        totalOrdered: 0,
        totalStocked: 0,
        totalScrapped: 0,
        overallYieldPct: 0,
        overallScrapPct: 0,
      }),
    );

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-total-work-orders-value').textContent).toContain(
      '0',
    );
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-yield-rate-value').textContent).toContain('0%');
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-scrap-rate-value').textContent).toContain('0%');
    expect(
      fixture.nativeElement.querySelector('#aw-manufacturing-kpi-total-units-scrapped-value').textContent,
    ).toContain('0');
  });

  it('shows scorecard loading skeletons while the scorecard request is pending', () => {
    const subject = new Subject<ManufacturingQualityScorecard>();
    vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(subject.asObservable());

    fixture.detectChanges();

    expect(component['isScorecardLoading']()).toBe(true);
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-quality-scorecard-loading')).toBeTruthy();

    subject.next(mockScorecard);
    subject.complete();
    fixture.detectChanges();

    expect(component['isScorecardLoading']()).toBe(false);
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-quality-scorecard')).toBeTruthy();
  });

  it('shows the scorecard error state when the scorecard API fails', () => {
    vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(
      throwError(() => new Error('scorecard request failed')),
    );

    fixture.detectChanges();

    expect(component['hasScorecardError']()).toBe(true);
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-quality-scorecard-error')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-quality-scorecard')).toBeNull();
  });

  it('renders empty states for scorecard collections with no scrap data', () => {
    vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(
      of({ ...mockScorecard, top5ByScrapped: [], scrapReasonBreakdown: [] }),
    );

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#aw-manufacturing-quality-scorecard-top-scrapped-empty')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-quality-scorecard-scrap-reasons-empty')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-quality-scorecard-bottom-yield-empty')).toBeNull();
  });
});
