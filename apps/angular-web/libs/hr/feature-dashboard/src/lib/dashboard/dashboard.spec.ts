import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, input } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { patchState } from '@ngrx/signals';
import { unprotected } from '@ngrx/signals/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { setError, setLoaded, setLoading } from '@adventureworks-web/shared/data-access';
import type { DepartmentHeadcountSummary, EmployeeAggregates, TenureDistribution } from '@adventureworks-web/hr/data-access';
import { HrDashboardStore } from '../stores/hr-dashboard.store';
import { HeadcountChartComponent } from '../headcount-chart/headcount-chart';
import { TenureChartComponent } from '../tenure-chart/tenure-chart';
import { HrDashboardComponent } from './dashboard';

/**
 * dashboard.ts imports HeadcountChartComponent/TenureChartComponent, whose module scope calls
 * the real Chart.register(...) regardless of instantiation — mirrors sales/feature-dashboard's
 * dashboard.spec.ts mocking rationale.
 */
vi.mock('chart.js', () => {
  const Chart = vi.fn().mockImplementation(function () { return { destroy: vi.fn() }; });
  (Chart as unknown as { register: () => void }).register = vi.fn();
  return {
    Chart,
    BarController: {},
    BarElement: {},
    LinearScale: {},
    CategoryScale: {},
    Legend: {},
    Tooltip: {},
  };
});

@Component({ selector: 'aw-headcount-chart', standalone: true, template: '' })
class StubHeadcountChartComponent {
  readonly data = input.required<DepartmentHeadcountSummary[]>();
}

@Component({ selector: 'aw-tenure-chart', standalone: true, template: '' })
class StubTenureChartComponent {
  readonly data = input.required<TenureDistribution>();
}

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockAggregates: EmployeeAggregates = {
  totalEmployeeCount: 296,
  activeEmployeeCount: 290,
  terminatedEmployeeCount: 6,
  departmentCount: 16,
  departmentHeadcounts: [
    { departmentId: 1, departmentName: 'Engineering', groupName: 'Research and Development', activeEmployeeCount: 6 },
  ],
  tenureDistribution: {
    underOneYear: 32,
    oneToThreeYears: 58,
    threeToFiveYears: 71,
    fiveToTenYears: 89,
    tenPlusYears: 40,
  },
  payBandSummary: [{ departmentGroup: 'Research and Development', averageRate: 32.65, minRate: 8.62, maxRate: 50.48 }],
};

describe('HrDashboardComponent', () => {
  let component: HrDashboardComponent;
  let fixture: ComponentFixture<HrDashboardComponent>;
  let store: InstanceType<typeof HrDashboardStore>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HrDashboardComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: ENVIRONMENT, useValue: mockEnvironment }],
    })
      .overrideComponent(HrDashboardComponent, {
        remove: { imports: [HeadcountChartComponent, TenureChartComponent] },
        add: { imports: [StubHeadcountChartComponent, StubTenureChartComponent] },
      })
      .compileComponents();

    store = TestBed.inject(HrDashboardStore);
    vi.spyOn(store, 'load');

    fixture = TestBed.createComponent(HrDashboardComponent);
    component = fixture.componentInstance;
  });

  it('renders without errors in default idle state', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('calls store.load() once on ngOnInit', () => {
    fixture.detectChanges();
    expect(store.load).toHaveBeenCalledTimes(1);
  });

  it('shows skeleton elements while isLoading', () => {
    patchState(unprotected(store), setLoading());
    fixture.detectChanges();

    const skeletons = fixture.nativeElement.querySelectorAll('aw-skeleton');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it('renders 4 stat cards with formatted counts when loaded', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(store), { aggregates: mockAggregates, lastUpdated: new Date() }, setLoaded());
    fixture.detectChanges();
    await fixture.whenStable();

    const statEls = fixture.nativeElement.querySelectorAll('.stat');
    expect(statEls.length).toBe(4);

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('296');
    expect(text).toContain('290');
    expect(text).toContain('6');
    expect(text).toContain('16');
  });

  it('renders the headcount and tenure charts when loaded', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(store), { aggregates: mockAggregates, lastUpdated: new Date() }, setLoaded());
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('aw-headcount-chart')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('aw-tenure-chart')).toBeTruthy();
  });

  it('renders the pay band summary table with currency-formatted rates when loaded', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(store), { aggregates: mockAggregates, lastUpdated: new Date() }, setLoaded());
    fixture.detectChanges();
    await fixture.whenStable();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Research and Development');
    expect(text).toContain('$32.65');
    expect(text).toContain('$8.62');
    expect(text).toContain('$50.48');
  });

  it('shows an inline error alert with a Retry button on failure (not a toast)', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(store), setError('Failed to load HR dashboard data'));
    fixture.detectChanges();
    await fixture.whenStable();

    const alert = fixture.nativeElement.querySelector('#aw-hr-dashboard-error');
    expect(alert).toBeTruthy();

    const retryButton = alert.querySelector('button') as HTMLButtonElement;
    expect(retryButton).toBeTruthy();
    expect(retryButton.textContent).toContain('Retry');
  });

  it('clicking Retry calls store.load() again', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(store), setError('Failed to load HR dashboard data'));
    fixture.detectChanges();
    await fixture.whenStable();

    const retryButton = fixture.nativeElement.querySelector('#aw-hr-dashboard-error button') as HTMLButtonElement;
    retryButton.click();

    expect(store.load).toHaveBeenCalledTimes(2);
  });

  it('shows a "Last updated" label with relative time once loaded', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(store), { aggregates: mockAggregates, lastUpdated: new Date() }, setLoaded());
    fixture.detectChanges();
    await fixture.whenStable();

    const label = fixture.nativeElement.querySelector('#aw-hr-dashboard-last-updated');
    expect(label).toBeTruthy();
    expect(label.textContent).toContain('Last updated');
  });

  it('does not show a "Last updated" label before the first successful load', () => {
    fixture.detectChanges();

    const label = fixture.nativeElement.querySelector('#aw-hr-dashboard-last-updated');
    expect(label).toBeFalsy();
  });

  it('clicking Refresh calls store.load() again and re-shows skeletons while in flight', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    patchState(unprotected(store), { aggregates: mockAggregates, lastUpdated: new Date() }, setLoaded());
    fixture.detectChanges();
    await fixture.whenStable();

    const refreshButton = fixture.nativeElement.querySelector('#aw-hr-dashboard-refresh-btn') as HTMLButtonElement;
    refreshButton.click();
    expect(store.load).toHaveBeenCalledTimes(2);

    // Simulate the store transitioning back into loading state as a real refresh would.
    patchState(unprotected(store), setLoading());
    fixture.detectChanges();

    const skeletons = fixture.nativeElement.querySelectorAll('aw-skeleton');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it('disables the Refresh button while loading', () => {
    patchState(unprotected(store), setLoading());
    fixture.detectChanges();

    const refreshButton = fixture.nativeElement.querySelector('#aw-hr-dashboard-refresh-btn') as HTMLButtonElement;
    expect(refreshButton.disabled).toBe(true);
  });
});
