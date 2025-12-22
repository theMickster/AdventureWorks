import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Chart } from 'chart.js';
import { TenureChartComponent } from './tenure-chart';
import type { TenureDistribution } from '@adventureworks-web/hr/data-access';

vi.mock('chart.js', () => {
  const Chart = vi.fn().mockImplementation(function () { return { destroy: vi.fn() }; });
  (Chart as unknown as { register: () => void }).register = vi.fn();
  return { Chart, BarController: {}, BarElement: {}, LinearScale: {}, CategoryScale: {}, Tooltip: {} };
});

const mockDistribution: TenureDistribution = {
  underOneYear: 32,
  oneToThreeYears: 58,
  threeToFiveYears: 71,
  fiveToTenYears: 89,
  tenPlusYears: 40,
};

describe('TenureChartComponent', () => {
  let fixture: ComponentFixture<TenureChartComponent>;

  beforeEach(async () => {
    vi.mocked(Chart).mockClear();

    await TestBed.configureTestingModule({
      imports: [TenureChartComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TenureChartComponent);
    fixture.componentRef.setInput('data', mockDistribution);
  });

  it('renders a canvas element', () => {
    fixture.detectChanges();
    const canvas = fixture.nativeElement.querySelector('canvas');
    expect(canvas).toBeTruthy();
  });

  it('builds a stacked horizontal bar with 5 datasets, one per tenure bucket', async () => {
    fixture.detectChanges();
    await vi.waitFor(() => expect(vi.mocked(Chart)).toHaveBeenCalled());

    const chartConfig = (vi.mocked(Chart) as ReturnType<typeof vi.fn>).mock.calls[0]?.[1];

    expect(chartConfig.type).toBe('bar');
    expect(chartConfig.options.indexAxis).toBe('y');
    expect(chartConfig.options.scales.x.stacked).toBe(true);
    expect(chartConfig.data.datasets).toHaveLength(5);
    // The Chart.js legend plugin is intentionally disabled (draws on canvas pixels, invisible to
    // screen readers) — a real DOM legend in the template replaces it. See the accessibility tests below.
    expect(chartConfig.options.plugins.legend.display).toBe(false);
  });

  it('assigns a distinct color to each of the 5 bucket datasets', async () => {
    fixture.detectChanges();
    await vi.waitFor(() => expect(vi.mocked(Chart)).toHaveBeenCalled());

    const chartConfig = (vi.mocked(Chart) as ReturnType<typeof vi.fn>).mock.calls[0]?.[1];
    const colors = chartConfig.data.datasets.map((d: { backgroundColor: string }) => d.backgroundColor);

    expect(new Set(colors).size).toBe(5);
  });

  it('the 5 bucket values sum to the total active employee count passed in', async () => {
    fixture.detectChanges();
    await vi.waitFor(() => expect(vi.mocked(Chart)).toHaveBeenCalled());

    const chartConfig = (vi.mocked(Chart) as ReturnType<typeof vi.fn>).mock.calls[0]?.[1];
    const sum = chartConfig.data.datasets.reduce((total: number, d: { data: number[] }) => total + d.data[0], 0);

    const expectedTotal =
      mockDistribution.underOneYear +
      mockDistribution.oneToThreeYears +
      mockDistribution.threeToFiveYears +
      mockDistribution.fiveToTenYears +
      mockDistribution.tenPlusYears;

    expect(sum).toBe(expectedTotal);
  });

  it('destroys the Chart.js instance on ngOnDestroy', async () => {
    fixture.detectChanges();
    await vi.waitFor(() => expect(vi.mocked(Chart)).toHaveBeenCalled());

    const chartInstance = (vi.mocked(Chart) as ReturnType<typeof vi.fn>).mock.results[0]?.value;
    fixture.destroy();

    expect(chartInstance.destroy).toHaveBeenCalledTimes(1);
  });

  it('gives the canvas an accessible role and a text alternative summarizing every bucket', () => {
    fixture.detectChanges();

    const canvas = fixture.nativeElement.querySelector('canvas') as HTMLCanvasElement;
    expect(canvas.getAttribute('role')).toBe('img');

    const label = canvas.getAttribute('aria-label') ?? '';
    expect(label).toContain('0-1 yr: 32');
    expect(label).toContain('1-3 yrs: 58');
    expect(label).toContain('3-5 yrs: 71');
    expect(label).toContain('5-10 yrs: 89');
    expect(label).toContain('10+ yrs: 40');
  });

  it('renders a real DOM legend (not just the canvas-drawn one) with all 5 buckets and their colors', () => {
    fixture.detectChanges();

    const legendItems = fixture.nativeElement.querySelectorAll('ul[aria-label="Tenure bucket legend"] li');
    expect(legendItems.length).toBe(5);

    const text = (fixture.nativeElement.querySelector('ul[aria-label="Tenure bucket legend"]') as HTMLElement).textContent;
    expect(text).toContain('0-1 yr (32)');
    expect(text).toContain('10+ yrs (40)');
  });
});
