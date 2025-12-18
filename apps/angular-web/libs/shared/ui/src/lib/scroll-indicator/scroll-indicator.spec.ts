import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { ScrollIndicatorComponent } from './scroll-indicator';

function setDimensions(el: HTMLElement, { scrollLeft, clientWidth, scrollWidth }: { scrollLeft: number; clientWidth: number; scrollWidth: number }): void {
  Object.defineProperty(el, 'scrollLeft', { value: scrollLeft, configurable: true });
  Object.defineProperty(el, 'clientWidth', { value: clientWidth, configurable: true });
  Object.defineProperty(el, 'scrollWidth', { value: scrollWidth, configurable: true });
}

describe('ScrollIndicatorComponent', () => {
  let fixture: ComponentFixture<ScrollIndicatorComponent>;
  let component: ScrollIndicatorComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ScrollIndicatorComponent] }).compileComponents();
    fixture = TestBed.createComponent(ScrollIndicatorComponent);
    component = fixture.componentInstance;
  });

  it('shows neither gradient when content does not overflow', () => {
    fixture.detectChanges();

    expect(fixture.debugElement.query(By.css('.from-base-100'))).toBeNull();
  });

  it('makes the scroll container keyboard-focusable with a labeled region role', () => {
    fixture.detectChanges();

    const container = fixture.debugElement.query(By.css('[class*="overflow-x-auto"]')).nativeElement as HTMLElement;
    expect(container.getAttribute('tabindex')).toBe('0');
    expect(container.getAttribute('role')).toBe('region');
    expect(container.getAttribute('aria-label')).toBe('Scrollable content');
  });

  it('uses a custom ariaLabel input when provided', () => {
    fixture.componentRef.setInput('ariaLabel', 'Organization chart');
    fixture.detectChanges();

    const container = fixture.debugElement.query(By.css('[class*="overflow-x-auto"]')).nativeElement as HTMLElement;
    expect(container.getAttribute('aria-label')).toBe('Organization chart');
  });

  it('shows only the right gradient when scrolled to the start of overflowing content', () => {
    fixture.detectChanges();
    const container = fixture.debugElement.query(By.css('[class*="overflow-x-auto"]')).nativeElement as HTMLElement;
    setDimensions(container, { scrollLeft: 0, clientWidth: 200, scrollWidth: 600 });

    container.dispatchEvent(new Event('scroll'));
    fixture.detectChanges();

    expect(component['canScrollLeft']()).toBe(false);
    expect(component['canScrollRight']()).toBe(true);
  });

  it('shows only the left gradient when scrolled to the end of overflowing content', () => {
    fixture.detectChanges();
    const container = fixture.debugElement.query(By.css('[class*="overflow-x-auto"]')).nativeElement as HTMLElement;
    setDimensions(container, { scrollLeft: 400, clientWidth: 200, scrollWidth: 600 });

    container.dispatchEvent(new Event('scroll'));
    fixture.detectChanges();

    expect(component['canScrollLeft']()).toBe(true);
    expect(component['canScrollRight']()).toBe(false);
  });
});
