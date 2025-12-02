import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { TerritoryBreakdownComponent } from './territory-breakdown';

const mockTerritories = [
  { territoryId: 4, name: 'Southwest', group: 'North America', countryCode: 'US', revenue: 22000000, orderCount: 3421 },
  { territoryId: 6, name: 'France', group: 'Europe', countryCode: 'FR', revenue: 5000000, orderCount: 800 },
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
});
