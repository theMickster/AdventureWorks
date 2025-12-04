import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PublicFooterComponent } from './public-footer';

describe('PublicFooterComponent', () => {
  let fixture: ComponentFixture<PublicFooterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PublicFooterComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PublicFooterComponent);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('renders the current year in the copyright line', () => {
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain(String(new Date().getFullYear()));
  });
});
