import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { ParetoChartComponent, type ParetoChartDataPoint } from './pareto-chart';

// Chart.js must be mocked: the real library needs a live canvas 2D context, and the component
// builds its instance inside `afterNextRender`, where Angular swallows any throw into the plain
// ErrorHandler. The module-scope `Chart.register(...)` call also needs the mock to exist.
// Per the workspace convention, no assertion in this file touches the Chart constructor or
// `chart.destroy()` — only the DOM and the component's own state. Precedent: 6cf98cb, d900348.
vi.mock('chart.js', () => {
  const Chart = vi.fn().mockImplementation(function () {
    return { destroy: vi.fn() };
  });
  (Chart as unknown as { register: () => void }).register = vi.fn();
  return {
    Chart,
    BarController: {},
    BarElement: {},
    LineController: {},
    LineElement: {},
    PointElement: {},
    LinearScale: {},
    CategoryScale: {},
    Tooltip: {},
  };
});

// Real AdventureWorks top vendors by purchase order TotalDue, with the running cumulative share.
const mockParetoData: ParetoChartDataPoint[] = [
  { vendorId: 1576, vendorName: 'Superior Bicycles', totalSpend: 5034266.74, cumulativePercent: 6.1 },
  { vendorId: 1602, vendorName: 'Vision Cycles, Inc', totalSpend: 4894060.4, cumulativePercent: 12.0 },
  { vendorId: 1650, vendorName: 'American Bicycles and Wheels', totalSpend: 4520000.12, cumulativePercent: 17.5 },
];

describe('ParetoChartComponent', () => {
  let component: ParetoChartComponent;
  let fixture: ComponentFixture<ParetoChartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ParetoChartComponent],
      providers: [provideTranslateService()],
    }).compileComponents();

    fixture = TestBed.createComponent(ParetoChartComponent);
    component = fixture.componentInstance;
  });

  it('renders a canvas with an accessible role and label when data is present', () => {
    fixture.componentRef.setInput('paretoData', mockParetoData);
    fixture.detectChanges();

    const canvas = fixture.nativeElement.querySelector('canvas') as HTMLCanvasElement;
    expect(canvas).toBeTruthy();
    expect(canvas.getAttribute('role')).toBe('img');
    expect(canvas.getAttribute('aria-label')).toContain('Pareto chart of vendor spend');
  });

  it('sets hasData true when data is present', () => {
    fixture.componentRef.setInput('paretoData', mockParetoData);
    fixture.detectChanges();

    expect(component['hasData']()).toBe(true);
  });

  it('renders the empty state instead of a canvas when paretoData is null', () => {
    fixture.detectChanges();

    expect(component['hasData']()).toBe(false);
    expect(fixture.nativeElement.querySelector('canvas')).toBeNull();
    expect(fixture.nativeElement.querySelector('#aw-pareto-chart-empty')).toBeTruthy();
  });

  it('renders the empty state instead of a canvas when paretoData is an empty array', () => {
    fixture.componentRef.setInput('paretoData', []);
    fixture.detectChanges();

    expect(component['hasData']()).toBe(false);
    expect(fixture.nativeElement.querySelector('canvas')).toBeNull();
    expect(fixture.nativeElement.querySelector('#aw-pareto-chart-empty')).toBeTruthy();
  });

  it("maps a clicked chart index to that index's vendorId and emits it", async () => {
    fixture.componentRef.setInput('paretoData', mockParetoData);
    fixture.detectChanges();
    // The index -> vendorId array is built in `afterNextRender`, alongside the chart labels.
    await vi.waitFor(() => expect(component['vendorIdsByIndex']).toHaveLength(mockParetoData.length));

    const emitted: number[] = [];
    component.vendorClick.subscribe((id) => emitted.push(id));

    component['emitVendorAtIndex'](1);

    expect(emitted).toEqual([mockParetoData[1].vendorId]);
  });

  it('resolves the vendorId through the Chart.js interaction elements on click', async () => {
    fixture.componentRef.setInput('paretoData', mockParetoData);
    fixture.detectChanges();
    await vi.waitFor(() => expect(component['vendorIdsByIndex']).toHaveLength(mockParetoData.length));

    const emitted: number[] = [];
    component.vendorClick.subscribe((id) => emitted.push(id));

    const fakeChart = {
      getElementsAtEventForMode: vi.fn().mockReturnValue([{ index: 2, datasetIndex: 0, element: {} }]),
    };
    component['handleChartClick']({ type: 'click', native: new MouseEvent('click'), x: 10, y: 10 }, fakeChart as never);

    expect(fakeChart.getElementsAtEventForMode).toHaveBeenCalledWith(
      expect.any(MouseEvent),
      'index',
      { intersect: true },
      false,
    );
    expect(emitted).toEqual([mockParetoData[2].vendorId]);
  });

  it('does not emit when the click resolves to no chart element', async () => {
    fixture.componentRef.setInput('paretoData', mockParetoData);
    fixture.detectChanges();
    await vi.waitFor(() => expect(component['vendorIdsByIndex']).toHaveLength(mockParetoData.length));

    const emitted: number[] = [];
    component.vendorClick.subscribe((id) => emitted.push(id));

    const fakeChart = { getElementsAtEventForMode: vi.fn().mockReturnValue([]) };
    component['handleChartClick']({ type: 'click', native: new MouseEvent('click'), x: 10, y: 10 }, fakeChart as never);

    expect(emitted).toHaveLength(0);
  });

  it('does not emit for an index with no mapped vendor', async () => {
    fixture.componentRef.setInput('paretoData', mockParetoData);
    fixture.detectChanges();
    await vi.waitFor(() => expect(component['vendorIdsByIndex']).toHaveLength(mockParetoData.length));

    const emitted: number[] = [];
    component.vendorClick.subscribe((id) => emitted.push(id));

    component['emitVendorAtIndex'](99);

    expect(emitted).toHaveLength(0);
  });
});
