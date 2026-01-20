import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
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

  const query = (selector: string): HTMLElement | null => fixture.nativeElement.querySelector(selector);
  const queryAll = (selector: string): HTMLElement[] => [...fixture.nativeElement.querySelectorAll(selector)];

  // The scorecard request starts at construction (toSignal field initializer), so spies must be
  // configured before the component is created.
  const render = (): void => {
    fixture = TestBed.createComponent(ManufacturingKpiDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  };

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
  });

  it('shows loading skeletons before the API resolves', () => {
    const subject = new Subject<ManufacturingKpisDto>();
    vi.spyOn(workOrderApi, 'getKpis').mockReturnValue(subject.asObservable());

    render();

    expect(component['isLoading']()).toBe(true);
    expect(query('#aw-manufacturing-kpi-dashboard-loading')).toBeTruthy();
    expect(queryAll('#aw-manufacturing-kpi-dashboard-loading aw-skeleton')).toHaveLength(4);

    subject.next(mockKpis);
    subject.complete();
    fixture.detectChanges();

    expect(component['isLoading']()).toBe(false);
    expect(query('#aw-manufacturing-kpi-dashboard-loading')).toBeNull();
  });

  it('renders all KPI tiles on success', () => {
    render();

    expect(query('#aw-manufacturing-kpi-total-work-orders-value')?.textContent).toContain('72591');
    expect(query('#aw-manufacturing-kpi-yield-rate-value')?.textContent).toContain('99%');
    expect(query('#aw-manufacturing-kpi-scrap-rate-value')?.textContent).toContain('1%');
    expect(query('#aw-manufacturing-kpi-total-units-scrapped-value')?.textContent).toContain('1000');
  });

  it('loads the scorecard once and shares its collections across all sections', () => {
    render();

    expect(workOrderApi.getQualityScorecard).toHaveBeenCalledTimes(1);
    expect(query('#aw-manufacturing-quality-scorecard-top-scrapped')).toBeTruthy();
    expect(query('#aw-manufacturing-quality-scorecard-bottom-yield')).toBeTruthy();
    expect(query('#aw-manufacturing-quality-scorecard-scrap-reasons')).toBeTruthy();
    expect(queryAll('#aw-manufacturing-quality-scorecard tbody tr')).toHaveLength(3);
  });

  it('lays the product tables out side by side with the story wording and columns', () => {
    render();

    expect(query('#aw-manufacturing-product-scorecard')?.className).toContain('lg:grid-cols-2');
    expect(query('#aw-manufacturing-quality-scorecard-top-scrapped')?.textContent).toContain('Most Scrapped Products');
    expect(query('#aw-manufacturing-quality-scorecard-bottom-yield')?.textContent).toContain('Lowest Yield Products');

    const headers = (selector: string): string[] =>
      queryAll(`${selector} th`).map((th) => th.textContent?.trim() ?? '');
    expect(headers('#aw-manufacturing-quality-scorecard-top-scrapped')).toEqual(['Product', 'Total Scrapped']);
    expect(headers('#aw-manufacturing-quality-scorecard-bottom-yield')).toEqual(['Product', 'Yield %']);
    expect(query('#aw-manufacturing-scrap-breakdown #aw-manufacturing-quality-scorecard-scrap-reasons')).toBeTruthy();
  });

  it('renders product name and formatted values in both product tables', () => {
    vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(
      of({
        ...mockScorecard,
        top5ByScrapped: [{ ...mockScorecard.top5ByScrapped[0], scrappedQty: 12345 }],
        bottom5ByYield: [{ ...mockScorecard.bottom5ByYield[0], yieldPct: 82.5 }],
      }),
    );

    render();

    const cells = (selector: string): (string | undefined)[][] =>
      queryAll(`${selector} tbody tr`).map((row) =>
        Array.from(row.querySelectorAll('td')).map((cell) => cell.textContent?.trim()),
      );
    expect(cells('#aw-manufacturing-quality-scorecard-top-scrapped')).toEqual([
      ['HL Road Frame - Black, 58', '12,345'],
    ]);
    expect(cells('#aw-manufacturing-quality-scorecard-bottom-yield')).toEqual([['ML Road Seat Assembly', '82.5%']]);
  });

  it('renders product links with existing work-order filters', () => {
    render();

    const links = queryAll('#aw-manufacturing-product-scorecard a').map((link) => link.getAttribute('href'));

    expect(links).toEqual(['/manufacturing/work-orders?productId=747', '/manufacturing/work-orders?productId=518']);
  });

  describe('scrap reason breakdown', () => {
    const rankedBreakdown: ManufacturingQualityScorecard = {
      ...mockScorecard,
      scrapReasonBreakdown: [
        { scrapReasonId: 7, scrapReasonName: 'Handling damage', scrappedQty: 1200, scrapPct: 80.5 },
        { scrapReasonId: 3, scrapReasonName: 'Drill size too small', scrappedQty: 5, scrapPct: 19.5 },
      ],
    };
    const rowSelector = '#aw-manufacturing-scrap-breakdown tbody tr';
    let navigateSpy: ReturnType<typeof vi.spyOn>;

    beforeEach(() => {
      navigateSpy = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
      vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(of(rankedBreakdown));
    });

    it('renders a full-width card titled "Scrap Reason Breakdown" below the product scorecard', () => {
      render();

      const card = query('#aw-manufacturing-quality-scorecard-scrap-reasons');
      const productScorecard = query('#aw-manufacturing-product-scorecard');
      expect(card?.textContent).toContain('Scrap Reason Breakdown');
      expect(productScorecard?.contains(card)).toBe(false);
      expect(productScorecard?.parentElement?.lastElementChild?.id).toBe('aw-manufacturing-scrap-breakdown');
      expect(productScorecard?.nextElementSibling?.id).toBe('aw-manufacturing-scrap-breakdown');
    });

    it('renders the story column headers', () => {
      render();

      const headers = queryAll('#aw-manufacturing-scrap-breakdown th').map((th) => th.textContent?.trim());
      expect(headers).toEqual(['Reason Name', 'Units Scrapped', '% of Total']);
    });

    it('renders rows in API order with name, formatted quantity and percentage', () => {
      render();

      const rows = queryAll(rowSelector).map((row) =>
        Array.from(row.querySelectorAll('td')).map((cell) => cell.textContent?.trim()),
      );
      expect(rows).toEqual([
        ['Handling damage', '1,200', '80.5%'],
        ['Drill size too small', '5', '19.5%'],
      ]);
    });

    it('gives every row link semantics, keyboard focus, an accessible name and no nested anchor', () => {
      render();

      const rows = queryAll(rowSelector);
      expect(rows).toHaveLength(2);
      expect(rows.map((row) => row.getAttribute('role'))).toEqual(['link', 'link']);
      expect(rows.map((row) => row.getAttribute('tabindex'))).toEqual(['0', '0']);
      expect(rows.map((row) => row.getAttribute('aria-label'))).toEqual([
        'View work orders scrapped for Handling damage',
        'View work orders scrapped for Drill size too small',
      ]);
      expect(queryAll('#aw-manufacturing-scrap-breakdown a')).toHaveLength(0);
    });

    it('navigates to the filtered work-order list when a row is clicked', () => {
      render();

      queryAll(rowSelector)[0].click();

      expect(navigateSpy).toHaveBeenCalledTimes(1);
      expect(navigateSpy).toHaveBeenCalledWith(['/manufacturing/work-orders'], { queryParams: { scrapReasonId: 7 } });
    });

    it('navigates to the filtered work-order list when Enter is pressed on a row', () => {
      render();

      queryAll(rowSelector)[1].dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }));

      expect(navigateSpy).toHaveBeenCalledTimes(1);
      expect(navigateSpy).toHaveBeenCalledWith(['/manufacturing/work-orders'], { queryParams: { scrapReasonId: 3 } });
    });

    it('does not navigate on other keys', () => {
      render();

      queryAll(rowSelector)[0].dispatchEvent(new KeyboardEvent('keydown', { key: 'a', bubbles: true }));

      expect(navigateSpy).not.toHaveBeenCalled();
    });

    it('shows "No scrap data available" when the breakdown is empty', () => {
      vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(of({ ...mockScorecard, scrapReasonBreakdown: [] }));

      render();

      expect(query('#aw-manufacturing-quality-scorecard-scrap-reasons-empty')?.textContent).toContain(
        'No scrap data available',
      );
      expect(query('#aw-manufacturing-quality-scorecard-scrap-reasons-empty-description')).toBeNull();
      expect(queryAll(rowSelector)).toHaveLength(0);
    });

    it('shows a titled card with skeleton rows while the scorecard request is pending', () => {
      const subject = new Subject<ManufacturingQualityScorecard>();
      vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(subject.asObservable());

      render();

      const loadingCard = query('#aw-manufacturing-quality-scorecard-scrap-reasons-loading');
      expect(loadingCard?.textContent).toContain('Scrap Reason Breakdown');

      const skeletons = queryAll('#aw-manufacturing-quality-scorecard-scrap-reasons-loading aw-skeleton');
      expect(skeletons).toHaveLength(5);
      expect(new Set(skeletons.map((skeleton) => skeleton.querySelector('div')?.id)).size).toBe(5);

      subject.next(rankedBreakdown);
      subject.complete();
      fixture.detectChanges();

      expect(query('#aw-manufacturing-quality-scorecard-scrap-reasons-loading')).toBeNull();
      expect(queryAll(rowSelector)).toHaveLength(2);
      expect(workOrderApi.getQualityScorecard).toHaveBeenCalledTimes(1);
    });
  });

  it('shows the error empty state when the KPI API fails', () => {
    vi.spyOn(workOrderApi, 'getKpis').mockReturnValue(throwError(() => new Error('request failed')));

    render();

    expect(component['hasError']()).toBe(true);
    expect(query('#aw-manufacturing-kpi-dashboard-error')?.textContent).toContain('Manufacturing data unavailable');
    expect(query('#aw-manufacturing-kpi-dashboard-tiles')).toBeNull();
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

    render();

    expect(query('#aw-manufacturing-kpi-total-work-orders-value')?.textContent).toContain('0');
    expect(query('#aw-manufacturing-kpi-yield-rate-value')?.textContent).toContain('0%');
    expect(query('#aw-manufacturing-kpi-scrap-rate-value')?.textContent).toContain('0%');
    expect(query('#aw-manufacturing-kpi-total-units-scrapped-value')?.textContent).toContain('0');
  });

  it('shows titled product cards with skeleton rows while the scorecard request is pending', () => {
    const subject = new Subject<ManufacturingQualityScorecard>();
    vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(subject.asObservable());

    render();

    expect(query('#aw-manufacturing-quality-scorecard-loading')).toBeTruthy();
    expect(query('#aw-manufacturing-quality-scorecard-top-scrapped-loading')?.textContent).toContain(
      'Most Scrapped Products',
    );
    expect(query('#aw-manufacturing-quality-scorecard-bottom-yield-loading')?.textContent).toContain(
      'Lowest Yield Products',
    );

    const topSkeletons = queryAll('#aw-manufacturing-quality-scorecard-top-scrapped-loading aw-skeleton');
    const yieldSkeletons = queryAll('#aw-manufacturing-quality-scorecard-bottom-yield-loading aw-skeleton');
    expect(topSkeletons).toHaveLength(5);
    expect(yieldSkeletons).toHaveLength(5);

    const ids = [...topSkeletons, ...yieldSkeletons].map((skeleton) => skeleton.querySelector('div')?.id);
    expect(new Set(ids).size).toBe(10);

    subject.next(mockScorecard);
    subject.complete();
    fixture.detectChanges();

    expect(query('#aw-manufacturing-quality-scorecard-loading')).toBeNull();
    expect(query('#aw-manufacturing-quality-scorecard')).toBeTruthy();
    expect(workOrderApi.getQualityScorecard).toHaveBeenCalledTimes(1);
  });

  it('shows the scorecard error state when the scorecard API fails', () => {
    vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(
      throwError(() => new Error('scorecard request failed')),
    );

    render();

    expect(query('#aw-manufacturing-quality-scorecard-error')).toBeTruthy();
    expect(query('#aw-manufacturing-quality-scorecard')).toBeNull();
  });

  it('keeps the KPI tiles when only the scorecard API fails', () => {
    vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(
      throwError(() => new Error('scorecard request failed')),
    );

    render();

    expect(query('#aw-manufacturing-kpi-dashboard-tiles')).toBeTruthy();
    expect(query('#aw-manufacturing-kpi-dashboard-error')).toBeNull();
    expect(query('#aw-manufacturing-quality-scorecard-error')).toBeTruthy();
  });

  it('keeps the scorecard when only the KPI API fails', () => {
    vi.spyOn(workOrderApi, 'getKpis').mockReturnValue(throwError(() => new Error('request failed')));

    render();

    expect(query('#aw-manufacturing-kpi-dashboard-error')).toBeTruthy();
    expect(query('#aw-manufacturing-quality-scorecard-error')).toBeNull();
    expect(query('#aw-manufacturing-quality-scorecard')).toBeTruthy();
    expect(queryAll('#aw-manufacturing-quality-scorecard tbody tr')).toHaveLength(3);
  });

  it('renders "No data available" in every section when nothing was scrapped', () => {
    vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(
      of({ top5ByScrapped: [], bottom5ByYield: [], scrapReasonBreakdown: [] }),
    );

    render();

    expect(query('#aw-manufacturing-quality-scorecard-top-scrapped-empty')?.textContent).toContain('No data available');
    expect(query('#aw-manufacturing-quality-scorecard-bottom-yield-empty')?.textContent).toContain('No data available');
    expect(query('#aw-manufacturing-quality-scorecard-scrap-reasons-empty')).toBeTruthy();
    expect(query('#aw-manufacturing-quality-scorecard-top-scrapped-empty-description')).toBeNull();
    expect(query('#aw-manufacturing-quality-scorecard-bottom-yield-empty-description')).toBeNull();
    expect(queryAll('#aw-manufacturing-quality-scorecard tbody tr')).toHaveLength(0);
  });

  it('renders "No data available" for most scrapped products while the other lists are populated', () => {
    vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(of({ ...mockScorecard, top5ByScrapped: [] }));

    render();

    expect(query('#aw-manufacturing-quality-scorecard-top-scrapped-empty')?.textContent).toContain('No data available');
    expect(query('#aw-manufacturing-quality-scorecard-bottom-yield-empty')).toBeNull();
    expect(query('#aw-manufacturing-quality-scorecard-scrap-reasons-empty')).toBeNull();
  });

  it('renders "No data available" for lowest yield products while the other lists are populated', () => {
    vi.spyOn(workOrderApi, 'getQualityScorecard').mockReturnValue(of({ ...mockScorecard, bottom5ByYield: [] }));

    render();

    expect(query('#aw-manufacturing-quality-scorecard-bottom-yield-empty')?.textContent).toContain('No data available');
    expect(query('#aw-manufacturing-quality-scorecard-top-scrapped-empty')).toBeNull();
    expect(query('#aw-manufacturing-quality-scorecard-scrap-reasons-empty')).toBeNull();
  });
});
