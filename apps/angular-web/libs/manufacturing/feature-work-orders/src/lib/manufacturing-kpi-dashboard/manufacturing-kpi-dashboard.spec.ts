import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { Subject, of, throwError } from 'rxjs';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { WorkOrderApiService } from '@adventureworks-web/manufacturing/data-access';
import type { ManufacturingKpisDto } from '@adventureworks-web/manufacturing/data-access';
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
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    }).compileComponents();

    workOrderApi = TestBed.inject(WorkOrderApiService);
    vi.spyOn(workOrderApi, 'getKpis').mockReturnValue(of(mockKpis));

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

  it('shows the error empty state when the API fails', () => {
    vi.spyOn(workOrderApi, 'getKpis').mockReturnValue(throwError(() => new Error('request failed')));

    fixture.detectChanges();

    expect(component['hasError']()).toBe(true);
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-dashboard-error')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-dashboard-error').textContent).toContain(
      'Manufacturing data unavailable',
    );
    expect(fixture.nativeElement.querySelector('#aw-manufacturing-kpi-dashboard-tiles')).toBeNull();
  });

  it('renders zero values when the API returns no orders', () => {
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
});
