import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Chart } from 'chart.js';
import { HeadcountChartComponent } from './headcount-chart';
import type { DepartmentHeadcountSummary } from '@adventureworks-web/hr/data-access';

vi.mock('chart.js', () => {
  const Chart = vi.fn().mockImplementation(function () { return { destroy: vi.fn() }; });
  (Chart as unknown as { register: () => void }).register = vi.fn();
  return { Chart, BarController: {}, BarElement: {}, LinearScale: {}, CategoryScale: {}, Tooltip: {} };
});

const mockDepartments: DepartmentHeadcountSummary[] = [
  { departmentId: 1, departmentName: 'Engineering', groupName: 'Research and Development', activeEmployeeCount: 6 },
  { departmentId: 2, departmentName: 'Marketing', groupName: 'Sales and Marketing', activeEmployeeCount: 0 },
];

describe('HeadcountChartComponent', () => {
  let fixture: ComponentFixture<HeadcountChartComponent>;

  beforeEach(async () => {
    vi.mocked(Chart).mockClear();

    await TestBed.configureTestingModule({
      imports: [HeadcountChartComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(HeadcountChartComponent);
    fixture.componentRef.setInput('data', mockDepartments);
  });

  it('renders a canvas element', () => {
    fixture.detectChanges();
    const canvas = fixture.nativeElement.querySelector('canvas');
    expect(canvas).toBeTruthy();
  });

  it('builds a horizontal bar chart with one label/value per department, including zero-headcount ones', async () => {
    fixture.detectChanges();
    await vi.waitFor(() => expect(vi.mocked(Chart)).toHaveBeenCalled(), { timeout: 5000 });

    const chartConfig = (vi.mocked(Chart) as ReturnType<typeof vi.fn>).mock.calls[0]?.[1];

    expect(chartConfig.type).toBe('bar');
    expect(chartConfig.options.indexAxis).toBe('y');
    expect(chartConfig.data.labels).toEqual(['Engineering', 'Marketing']);
    expect(chartConfig.data.datasets[0].data).toEqual([6, 0]);
  });

  it('scales container height proportionally with department count', () => {
    fixture.detectChanges();
    const container = fixture.nativeElement.querySelector('div') as HTMLElement;
    // Math.max(320, 2 * 32) = 320
    expect(container.style.height).toBe('320px');
  });

  it('destroys the Chart.js instance on ngOnDestroy', async () => {
    fixture.detectChanges();
    await vi.waitFor(() => expect(vi.mocked(Chart)).toHaveBeenCalled(), { timeout: 5000 });

    const chartInstance = (vi.mocked(Chart) as ReturnType<typeof vi.fn>).mock.results[0]?.value;
    fixture.destroy();

    expect(chartInstance.destroy).toHaveBeenCalledTimes(1);
  });

  it('gives the canvas an accessible role and a text alternative naming every department', () => {
    fixture.detectChanges();

    const canvas = fixture.nativeElement.querySelector('canvas') as HTMLCanvasElement;
    expect(canvas.getAttribute('role')).toBe('img');

    const label = canvas.getAttribute('aria-label') ?? '';
    expect(label).toContain('Engineering: 6');
    expect(label).toContain('Marketing: 0');
  });
});
