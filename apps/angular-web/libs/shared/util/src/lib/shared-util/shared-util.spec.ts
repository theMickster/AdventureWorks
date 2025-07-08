import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SharedUtilComponent } from './shared-util';

describe('SharedUtilComponent', () => {
  let component: SharedUtilComponent;
  let fixture: ComponentFixture<SharedUtilComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SharedUtilComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SharedUtilComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
