import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { TerritoryBreakdownComponent } from './territory-breakdown';

const mockTerritories = [
  { territoryId: 4, name: 'Southwest', group: 'North America', countryCode: 'US', revenue: 22000000, orderCount: 3421 },
  { territoryId: 6, name: 'France', group: 'Europe', countryCode: 'FR', revenue: 5000000, orderCount: 800 },
  { territoryId: 7, name: 'Germany', group: 'Europe', countryCode: 'DE', revenue: 1000000, orderCount: 0 },
];

describe('TerritoryBreakdownComponent', () => {
  let fixture: ComponentFixture<TerritoryBreakdownComponent>;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TerritoryBreakdownComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate');

    fixture = TestBed.createComponent(TerritoryBreakdownComponent);
    fixture.componentRef.setInput('territories', mockTerritories);
    fixture.detectChanges();
  });

  it('renders both group headings', () => {
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Europe');
    expect(text).toContain('North America');
  });

  it('navigates to orders filtered by territory on row click', () => {
    const rows = fixture.nativeElement.querySelectorAll('tr.cursor-pointer');
    rows[0].click();
    expect(router.navigate).toHaveBeenCalledWith(
      ['/sales/orders'],
      { queryParams: { territoryId: 6 } }
    );
  });

  it('navigates to orders filtered by territory on Enter keydown', () => {
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('tr.cursor-pointer');
    const event = new KeyboardEvent('keydown', { key: 'Enter', bubbles: true });
    rows[0].dispatchEvent(event);

    expect(router.navigate).toHaveBeenCalledWith(
      ['/sales/orders'],
      { queryParams: { territoryId: 6 } }
    );
  });

  it('drillable row has title attribute "View orders for Southwest"', () => {
    fixture.detectChanges();
    const drillableRows = fixture.nativeElement.querySelectorAll('tr.cursor-pointer');
    const southwestRow = Array.from(drillableRows as NodeListOf<HTMLElement>).find(
      (row) => row.getAttribute('title') === 'View orders for Southwest',
    );
    expect(southwestRow).toBeTruthy();
  });

  it('drillable row has role="link" for screen reader accessibility', () => {
    fixture.detectChanges();
    const drillableRows = fixture.nativeElement.querySelectorAll('tr.cursor-pointer') as NodeListOf<HTMLElement>;
    expect(drillableRows.length).toBeGreaterThan(0);
    for (const row of Array.from(drillableRows)) {
      expect(row.getAttribute('role')).toBe('link');
    }
  });

  it('zero-orderCount row does not have cursor-pointer class', () => {
    fixture.detectChanges();
    const allRows = fixture.nativeElement.querySelectorAll('tr') as NodeListOf<HTMLElement>;
    const germanyRow = Array.from(allRows).find((row) => row.textContent?.includes('Germany'));
    expect(germanyRow).toBeTruthy();
    expect(germanyRow!.classList.contains('cursor-pointer')).toBe(false);
  });

  it('zero-orderCount row does not trigger navigate when clicked', () => {
    fixture.detectChanges();
    const allRows = fixture.nativeElement.querySelectorAll('tr') as NodeListOf<HTMLElement>;
    const germanyRow = Array.from(allRows).find((row) => row.textContent?.includes('Germany'));
    expect(germanyRow).toBeTruthy();
    germanyRow!.click();
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
