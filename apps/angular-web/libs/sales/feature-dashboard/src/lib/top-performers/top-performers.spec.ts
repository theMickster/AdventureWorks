import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TopPerformersComponent } from './top-performers';

const mockPerformers = [
  { salesPersonId: 275, name: 'Michael Blythe', territory: 'Northwest', revenue: 9293903, orderCount: 450 },
  { salesPersonId: 276, name: 'Linda Mitchell', territory: 'Southwest', revenue: 8845979, orderCount: 418 },
];

describe('TopPerformersComponent', () => {
  let fixture: ComponentFixture<TopPerformersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TopPerformersComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(TopPerformersComponent);
    fixture.componentRef.setInput('performers', mockPerformers);
    fixture.detectChanges();
  });

  it('renders first performer name as a link', () => {
    const link = fixture.nativeElement.querySelector('a[href*="/sales/persons/275"]');
    expect(link).toBeTruthy();
    expect(link.textContent.trim()).toBe('Michael Blythe');
  });

  it('renders a progress bar for each performer', () => {
    const bars = fixture.nativeElement.querySelectorAll('progress');
    expect(bars.length).toBe(2);
  });

  it('top performer progress bar is at 100%', () => {
    const firstBar = fixture.nativeElement.querySelector('progress');
    expect(Number(firstBar.getAttribute('value'))).toBe(100);
  });

  it('renders formatted revenue', () => {
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('$9,293,903');
  });
});
