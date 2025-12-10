import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Chart } from 'chart.js';
import { OrderAnalyticsTrendChartComponent } from './order-analytics-trend-chart';
import type { SalesOrderMonthlyTrend } from '@adventureworks-web/sales/data-access';

const mockDestroy = vi.fn();

vi.mock('chart.js', () => {
  const Chart = vi.fn().mockImplementation(function () {
    return { destroy: mockDestroy };
  });
  (Chart as unknown as { register: () => void }).register = vi.fn();
  return {
    Chart,
    LineController: {},
    LineElement: {},
    PointElement: {},
    LinearScale: {},
    CategoryScale: {},
    Tooltip: {},
  };
});

const singlePoint: SalesOrderMonthlyTrend[] = [{ year: 2013, month: 6, revenue: 120000, isPartialMonth: false }];

const multiPoint: SalesOrderMonthlyTrend[] = [
  { year: 2013, month: 6, revenue: 120000, isPartialMonth: false },
  { year: 2013, month: 7, revenue: 95000, isPartialMonth: true },
];

describe('OrderAnalyticsTrendChartComponent', () => {
  let fixture: ComponentFixture<OrderAnalyticsTrendChartComponent>;

  beforeEach(async () => {
    vi.mocked(Chart).mockClear();

    await TestBed.configureTestingModule({
      imports: [OrderAnalyticsTrendChartComponent],
    }).compileComponents();
  });

  afterEach(() => {
    TestBed.resetTestingModule();
    mockDestroy.mockReset();
  });

  it('renders a canvas element when given a single data point', () => {
    fixture = TestBed.createComponent(OrderAnalyticsTrendChartComponent);
    fixture.componentRef.setInput('monthlyTrend', singlePoint);
    fixture.detectChanges();

    const canvas = fixture.nativeElement.querySelector('canvas');
    expect(canvas).not.toBeNull();
  });

  it('renders a canvas element when given multiple data points', () => {
    fixture = TestBed.createComponent(OrderAnalyticsTrendChartComponent);
    fixture.componentRef.setInput('monthlyTrend', multiPoint);
    fixture.detectChanges();

    const canvas = fixture.nativeElement.querySelector('canvas');
    expect(canvas).not.toBeNull();
  });

  it('renders without error when monthlyTrend is an empty array', () => {
    fixture = TestBed.createComponent(OrderAnalyticsTrendChartComponent);
    fixture.componentRef.setInput('monthlyTrend', []);
    expect(() => fixture.detectChanges()).not.toThrow();
  });

  it('calls chart.destroy() on ngOnDestroy', async () => {
    fixture = TestBed.createComponent(OrderAnalyticsTrendChartComponent);
    fixture.componentRef.setInput('monthlyTrend', singlePoint);
    fixture.detectChanges();

    await vi.waitFor(() => expect(vi.mocked(Chart)).toHaveBeenCalled());

    const chartInstance = (vi.mocked(Chart) as ReturnType<typeof vi.fn>).mock.results[0]?.value;

    fixture.destroy();

    expect(chartInstance.destroy).toHaveBeenCalledTimes(1);
  });
});
