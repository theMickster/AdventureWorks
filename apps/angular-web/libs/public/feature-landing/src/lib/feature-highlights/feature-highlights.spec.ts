import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FeatureHighlightsComponent } from './feature-highlights';

describe('FeatureHighlightsComponent', () => {
  let fixture: ComponentFixture<FeatureHighlightsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeatureHighlightsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FeatureHighlightsComponent);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('renders a placeholder card per feature', () => {
    fixture.detectChanges();

    const cards = fixture.nativeElement.querySelectorAll('.card');
    expect(cards.length).toBe(fixture.componentInstance['features'].length);
  });
});
