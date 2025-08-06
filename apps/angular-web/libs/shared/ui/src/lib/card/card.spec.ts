import { describe, it, expect, beforeEach } from 'vitest';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { CardComponent } from './card';

describe('CardComponent', () => {
  let fixture: ComponentFixture<CardComponent>;
  let component: CardComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CardComponent],
      providers: [provideTranslateService()],
    }).compileComponents();

    fixture = TestBed.createComponent(CardComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render with DaisyUI card classes', () => {
    const card = fixture.nativeElement.querySelector('#aw-card') as HTMLElement;
    expect(card).toBeTruthy();
    expect(card.classList.contains('card')).toBe(true);
    expect(card.classList.contains('bg-base-200')).toBe(true);
    expect(card.classList.contains('shadow-md')).toBe(true);
  });

  it('should render title when provided', () => {
    fixture.componentRef.setInput('title', 'Test Title');
    fixture.detectChanges();
    const title = fixture.nativeElement.querySelector('#aw-card-title') as HTMLElement;
    expect(title).toBeTruthy();
    expect(title.textContent).toContain('Test Title');
  });

  it('should hide title when empty', () => {
    const title = fixture.nativeElement.querySelector('#aw-card-title');
    expect(title).toBeNull();
  });

  it('should show skeleton divs when loading is true', () => {
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();
    const skeleton = fixture.nativeElement.querySelector('#aw-card-skeleton') as HTMLElement;
    expect(skeleton).toBeTruthy();
    const skeletonDivs = skeleton.querySelectorAll('.skeleton');
    expect(skeletonDivs.length).toBe(2);
  });

  it('should hide body content when loading is true', () => {
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();
    const body = fixture.nativeElement.querySelector('#aw-card-body');
    expect(body).toBeNull();
  });
});

@Component({
  selector: 'aw-test-host',
  imports: [CardComponent],
  template: `
    <aw-card fieldId="aw-test-card" title="Card Title">
      <p id="aw-test-body">Body content</p>
      <div card-actions>
        <button id="aw-test-action">Action</button>
      </div>
    </aw-card>
  `,
})
class TestHostComponent {}

describe('CardComponent content projection', () => {
  let fixture: ComponentFixture<TestHostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
      providers: [provideTranslateService()],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    await fixture.whenStable();
  });

  it('should project body content via default slot', () => {
    const body = fixture.nativeElement.querySelector('#aw-test-body') as HTMLElement;
    expect(body).toBeTruthy();
    expect(body.textContent).toContain('Body content');
  });

  it('should project actions content into card-actions div', () => {
    const actionsContainer = fixture.nativeElement.querySelector('#aw-test-card-actions') as HTMLElement;
    expect(actionsContainer).toBeTruthy();
    expect(actionsContainer.classList.contains('card-actions')).toBe(true);
    const actionButton = actionsContainer.querySelector('#aw-test-action') as HTMLElement;
    expect(actionButton).toBeTruthy();
    expect(actionButton.textContent).toContain('Action');
  });
});
