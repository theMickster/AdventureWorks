import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SharedUiComponent } from './shared-ui';

describe('SharedUiComponent', () => {
  let component: SharedUiComponent;
  let fixture: ComponentFixture<SharedUiComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SharedUiComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SharedUiComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
