import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { patchState } from '@ngrx/signals';
import { unprotected } from '@ngrx/signals/testing';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { setError, setLoaded, setLoading } from '@adventureworks-web/shared/data-access';
import { DashboardStore } from '@adventureworks-web/sales/data-access';
import { DashboardComponent } from './dashboard';

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
  topPerformers: [],
  territoryBreakdown: [],
  monthlySalesTrend: [],
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

  it('shows 3 skeleton blocks while isLoading', () => {
    patchState(unprotected(dashboardStore), setLoading());
    fixture.detectChanges();

    const skeletons = fixture.nativeElement.querySelectorAll('aw-skeleton');
    expect(skeletons.length).toBe(9); // 3 blocks × 3 skeletons each
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
});
