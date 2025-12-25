import { ComponentFixture, TestBed } from '@angular/core/testing';
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
    await TestBed.configureTestingModule({
      imports: [TenureChartComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TenureChartComponent);
    fixture.componentRef.setInput('data', mockDistribution);
  });

  afterEach(() => TestBed.resetTestingModule());

  it('renders a canvas element', () => {
    fixture.detectChanges();
    const canvas = fixture.nativeElement.querySelector('canvas');
    expect(canvas).toBeTruthy();
  });

  it('assigns a distinct color to each of the 5 bucket datasets', () => {
    fixture.detectChanges();

    const bucketSummary = (
      fixture.componentInstance as unknown as { bucketSummary: () => { color: string }[] }
    ).bucketSummary();
    const colors = bucketSummary.map((b) => b.color);

    expect(new Set(colors).size).toBe(5);
  });

  it('the 5 bucket values sum to the total active employee count passed in', () => {
    fixture.detectChanges();

    const bucketSummary = (
      fixture.componentInstance as unknown as { bucketSummary: () => { value: number }[] }
    ).bucketSummary();
    const sum = bucketSummary.reduce((total, b) => total + b.value, 0);

    const expectedTotal =
      mockDistribution.underOneYear +
      mockDistribution.oneToThreeYears +
      mockDistribution.threeToFiveYears +
      mockDistribution.fiveToTenYears +
      mockDistribution.tenPlusYears;

    expect(sum).toBe(expectedTotal);
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
