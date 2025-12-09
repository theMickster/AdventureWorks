import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OrderAnalyticsPanelComponent } from './order-analytics-panel';
import type { SalesOrderAnalytics } from '@adventureworks-web/sales/data-access';

vi.mock('chart.js', () => {
  const Chart = vi.fn().mockImplementation(function () { return { destroy: vi.fn() }; });
  (Chart as unknown as { register: () => void }).register = vi.fn();
  return { Chart, LineController: {}, LineElement: {}, PointElement: {}, LinearScale: {}, CategoryScale: {}, Tooltip: {} };
});

const mockAnalytics: SalesOrderAnalytics = {
  totalRevenue: 1234567.89,
  orderCount: 42,
  percentageOfTotal: 15.5,
  monthlyTrend: [
    { year: 2013, month: 1, revenue: 500000, isPartialMonth: false },
    { year: 2013, month: 2, revenue: 734567.89, isPartialMonth: true },
  ],
};

describe('OrderAnalyticsPanelComponent', () => {
  let fixture: ComponentFixture<OrderAnalyticsPanelComponent>;

  async function createComponent(
    analytics: SalesOrderAnalytics | null | undefined,
    status: 'idle' | 'loading' | 'loaded' | 'error' | undefined,
  ): Promise<ComponentFixture<OrderAnalyticsPanelComponent>> {
    await TestBed.configureTestingModule({
      imports: [OrderAnalyticsPanelComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(OrderAnalyticsPanelComponent);
    if (analytics !== undefined) {
      fixture.componentRef.setInput('analytics', analytics);
    }
    if (status !== undefined) {
      fixture.componentRef.setInput('status', status);
    }
    fixture.detectChanges();
    return fixture;
  }

  afterEach(() => TestBed.resetTestingModule());

  it.skip('renders KPI tiles when analytics data is non-empty', async () => {
    await createComponent(mockAnalytics, 'loaded');

    const text = fixture.nativeElement.textContent as string;
    // Total revenue tile: $1,234,568 (1.0-0 format rounds to whole number)
    expect(text).toContain('$1,234,568');
    // Order count tile
    expect(text).toContain('42');
    // % of total
    expect(text).toContain('15.5%');
  });

  it.skip('shows skeleton placeholders when status is loading', async () => {
    await createComponent(null, 'loading');

    const skeletons = fixture.nativeElement.querySelectorAll('.skeleton');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it.skip('shows skeleton placeholders when status is idle', async () => {
    await createComponent(null, 'idle');

    const skeletons = fixture.nativeElement.querySelectorAll('.skeleton');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it.skip('shows empty state when orderCount is 0', async () => {
    const emptyAnalytics: SalesOrderAnalytics = { ...mockAnalytics, orderCount: 0 };
    await createComponent(emptyAnalytics, 'loaded');

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('No orders match the current filters');
  });

  it.skip('shows empty state when totalRevenue is NaN', async () => {
    const nanAnalytics: SalesOrderAnalytics = { ...mockAnalytics, totalRevenue: NaN };
    await createComponent(nanAnalytics, 'loaded');

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('No orders match the current filters');
  });

  it.skip('shows empty state when analytics is null', async () => {
    await createComponent(null, 'loaded');

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('No orders match the current filters');
  });

  it.skip('shows error state when status is error', async () => {
    await createComponent(null, 'error');

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Analytics unavailable');
  });

  it.skip('does not render the chart component when monthlyTrend is empty', async () => {
    const noTrendAnalytics: SalesOrderAnalytics = { ...mockAnalytics, monthlyTrend: [] };
    await createComponent(noTrendAnalytics, 'loaded');

    const chart = fixture.nativeElement.querySelector('aw-order-analytics-trend-chart');
    expect(chart).toBeNull();
  });

  it.skip('renders the chart component when monthlyTrend has entries', async () => {
    await createComponent(mockAnalytics, 'loaded');

    const chart = fixture.nativeElement.querySelector('aw-order-analytics-trend-chart');
    expect(chart).not.toBeNull();
  });
});
