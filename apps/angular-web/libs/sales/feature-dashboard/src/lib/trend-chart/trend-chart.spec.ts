import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TrendChartComponent } from './trend-chart';

vi.mock('chart.js', () => {
  const Chart = vi.fn().mockImplementation(function() { return {}; });
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
});
