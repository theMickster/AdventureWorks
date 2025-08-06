import { describe, it, expect, beforeEach } from 'vitest';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { EmptyStateComponent } from './empty-state';

describe('EmptyStateComponent', () => {
  let fixture: ComponentFixture<EmptyStateComponent>;
  let component: EmptyStateComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmptyStateComponent],
      providers: [provideTranslateService()],
    }).compileComponents();

    fixture = TestBed.createComponent(EmptyStateComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('title', 'No items found');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render icon with correct class', () => {
    fixture.componentRef.setInput('icon', 'fa-solid fa-box');
    fixture.detectChanges();
    const icon = fixture.nativeElement.querySelector('i') as HTMLElement;
    expect(icon.classList.contains('fa-solid')).toBe(true);
    expect(icon.classList.contains('fa-box')).toBe(true);
    expect(icon.getAttribute('aria-hidden')).toBe('true');
  });

  it('should render title text', () => {
    const title = fixture.nativeElement.querySelector('h3') as HTMLElement;
    expect(title.textContent).toContain('No items found');
  });

  it('should render description when provided', () => {
    fixture.componentRef.setInput('description', 'Try adding some items');
    fixture.detectChanges();
    const description = fixture.nativeElement.querySelector('p') as HTMLElement;
    expect(description).toBeTruthy();
    expect(description.textContent).toContain('Try adding some items');
  });

  it('should hide description when empty string', () => {
    fixture.componentRef.setInput('description', '');
    fixture.detectChanges();
    const description = fixture.nativeElement.querySelector('p');
    expect(description).toBeNull();
  });

  it('should use default icon when none provided', () => {
    const icon = fixture.nativeElement.querySelector('i') as HTMLElement;
    expect(icon.classList.contains('fa-solid')).toBe(true);
    expect(icon.classList.contains('fa-inbox')).toBe(true);
  });
});

@Component({
  selector: 'aw-test-host',
  imports: [EmptyStateComponent],
  template: `<aw-empty-state title="No data"><button id="aw-test-action">Add Item</button></aw-empty-state>`,
})
class TestHostComponent {}

describe('EmptyStateComponent content projection', () => {
  let fixture: ComponentFixture<TestHostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
      providers: [provideTranslateService()],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    await fixture.whenStable();
  });

  it('should project child content', () => {
    const button = fixture.nativeElement.querySelector('#aw-test-action') as HTMLElement;
    expect(button).toBeTruthy();
    expect(button.textContent).toContain('Add Item');
  });
});
