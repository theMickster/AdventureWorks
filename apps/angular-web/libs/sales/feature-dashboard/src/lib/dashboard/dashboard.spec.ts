import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { By } from '@angular/platform-browser';
import { provideTranslateService } from '@ngx-translate/core';
import { patchState } from '@ngrx/signals';
import { unprotected } from '@ngrx/signals/testing';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { setError, setLoaded, setLoading } from '@adventureworks-web/shared/data-access';
import { DashboardStore } from '@adventureworks-web/sales/data-access';
import { TrendChartComponent } from '../trend-chart/trend-chart';
import { DashboardComponent } from './dashboard';

vi.mock('chart.js', () => {
  const Chart = vi.fn().mockImplementation(function() { return { destroy: vi.fn() }; });
  (Chart as unknown as { register: () => void }).register = vi.fn();
  return { Chart, LineController: {}, LineElement: {}, PointElement: {}, LinearScale: {}, CategoryScale: {}, Tooltip: {} };
});

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockDashboard = {
  totalRevenue: 123216786.12,
  orderCount: 31465,
  averageOrderValue: 3915.99,
  topPerformers: [
    { salesPersonId: 275, name: 'Michael Blythe', territory: 'Northwest', revenue: 9293903, orderCount: 450 },
    { salesPersonId: 276, name: 'Linda Mitchell', territory: 'Southwest', revenue: 8845979, orderCount: 418 },
  ],
  territoryBreakdown: [
    { territoryId: 4, name: 'Southwest', group: 'North America', countryCode: 'US', revenue: 22000000, orderCount: 3421 },
    { territoryId: 6, name: 'France', group: 'Europe', countryCode: 'FR', revenue: 5000000, orderCount: 800 },
  ],
  monthlySalesTrend: [
    { year: 2012, month: 7, revenue: 800000, orderCount: 210 },
    { year: 2012, month: 8, revenue: 850000, orderCount: 230 },
  ],
};

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let dashboardStore: InstanceType<typeof DashboardStore>;
  let notificationService: NotificationService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideTranslateService(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    }).compileComponents();

    dashboardStore = TestBed.inject(DashboardStore);
    notificationService = TestBed.inject(NotificationService);

    vi.spyOn(dashboardStore, 'load');

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
  });

  it('renders without errors in default idle state', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('shows skeleton elements while isLoading', () => {
    patchState(unprotected(dashboardStore), setLoading());
    fixture.detectChanges();

    const skeletons = fixture.nativeElement.querySelectorAll('aw-skeleton');
    // stats row: 3 blocks × 3 = 9, trend chart: 1, grid: 2 — total 12
    expect(skeletons.length).toBe(12);
  });

  it('renders 3 stat cards with formatted values when loaded', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(dashboardStore), { dashboard: mockDashboard }, setLoaded());
    fixture.detectChanges();
    await fixture.whenStable();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('$123,216,786');
    expect(text).toContain('31,465');
    expect(text).toContain('$3,916');
  });

  it('calls dashboardStore.load() once on ngOnInit', () => {
    fixture.detectChanges();
    expect(dashboardStore.load).toHaveBeenCalledTimes(1);
  });

  it('fires error notification when hasError becomes true', async () => {
    vi.spyOn(notificationService, 'error');
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(dashboardStore), setError('Failed to load dashboard KPIs'));
    fixture.detectChanges();
    await fixture.whenStable();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to load dashboard KPIs. Please try again.');
  });

  it('renders trend chart when loaded', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(dashboardStore), { dashboard: mockDashboard }, setLoaded());
    fixture.detectChanges();
    await fixture.whenStable();

    const trendChart = fixture.nativeElement.querySelector('aw-trend-chart');
    expect(trendChart).toBeTruthy();
  });

  it('renders top performers when loaded', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(dashboardStore), { dashboard: mockDashboard }, setLoaded());
    fixture.detectChanges();
    await fixture.whenStable();

    const topPerformers = fixture.nativeElement.querySelector('aw-top-performers');
    expect(topPerformers).toBeTruthy();
  });

  it('renders territory breakdown when loaded', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(dashboardStore), { dashboard: mockDashboard }, setLoaded());
    fixture.detectChanges();
    await fixture.whenStable();

    const territoryBreakdown = fixture.nativeElement.querySelector('aw-territory-breakdown');
    expect(territoryBreakdown).toBeTruthy();
  });

  it('navigates to orders with correct month date range on trend chart click', async () => {
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(dashboardStore), { dashboard: mockDashboard }, setLoaded());
    fixture.detectChanges();
    await fixture.whenStable();

    const trendChartDe = fixture.debugElement.query(By.directive(TrendChartComponent));
    trendChartDe.componentInstance.dataPointClick.emit({ year: 2014, month: 3 });

    expect(router.navigate).toHaveBeenCalledWith(
      ['/sales/orders'],
      { queryParams: { orderDateFrom: '2014-03-01', orderDateTo: '2014-03-31' } }
    );
  });

  it('navigates with correct last day for February in a leap year', async () => {
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(dashboardStore), { dashboard: mockDashboard }, setLoaded());
    fixture.detectChanges();
    await fixture.whenStable();

    const trendChartDe = fixture.debugElement.query(By.directive(TrendChartComponent));
    trendChartDe.componentInstance.dataPointClick.emit({ year: 2012, month: 2 });

    expect(router.navigate).toHaveBeenCalledWith(
      ['/sales/orders'],
      { queryParams: { orderDateFrom: '2012-02-01', orderDateTo: '2012-02-29' } }
    );
  });
});
