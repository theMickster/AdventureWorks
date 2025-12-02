import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Chart } from 'chart.js';
import { TrendChartComponent } from './trend-chart';

vi.mock('chart.js', () => {
  const Chart = vi.fn().mockImplementation(function() { return { destroy: vi.fn() }; });
  (Chart as unknown as { register: () => void }).register = vi.fn();
  return { Chart, LineController: {}, LineElement: {}, PointElement: {}, LinearScale: {}, CategoryScale: {}, Tooltip: {} };
});

const mockTrend = [
  { year: 2012, month: 7, revenue: 800000, orderCount: 210 },
  { year: 2012, month: 8, revenue: 850000, orderCount: 230 },
];

describe('TrendChartComponent', () => {
  let fixture: ComponentFixture<TrendChartComponent>;

  beforeEach(async () => {
    vi.mocked(Chart).mockClear();

    await TestBed.configureTestingModule({
      imports: [TrendChartComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TrendChartComponent);
    fixture.componentRef.setInput('data', mockTrend);
  });

  it('renders a canvas element', () => {
    fixture.detectChanges();
    const canvas = fixture.nativeElement.querySelector('canvas');
    expect(canvas).toBeTruthy();
  });

  it('emits dataPointClick with year and month when a data point is clicked', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    const emitted: { year: number; month: number }[] = [];
    fixture.componentInstance.dataPointClick.subscribe((e) => emitted.push(e));

    const chartConfig = (vi.mocked(Chart) as ReturnType<typeof vi.fn>).mock.calls[0]?.[1];
    const onClick = chartConfig?.options?.onClick;
    expect(onClick).toBeDefined();

    onClick({} as unknown, [{ index: 0 } as unknown]);

    expect(emitted).toEqual([{ year: 2012, month: 7 }]);
  });

  it('does not emit dataPointClick when clicking empty chart area', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    const emitted: { year: number; month: number }[] = [];
    fixture.componentInstance.dataPointClick.subscribe((e) => emitted.push(e));

    const chartConfig = (vi.mocked(Chart) as ReturnType<typeof vi.fn>).mock.calls[0]?.[1];
    const onClick = chartConfig?.options?.onClick;

    onClick({} as unknown, []);

    expect(emitted).toHaveLength(0);
  });

  it('destroys the Chart.js instance on ngOnDestroy', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    const chartInstance = (vi.mocked(Chart) as ReturnType<typeof vi.fn>).mock.results[0]?.value;
    fixture.destroy();

    expect(chartInstance.destroy).toHaveBeenCalledTimes(1);
  });
});
