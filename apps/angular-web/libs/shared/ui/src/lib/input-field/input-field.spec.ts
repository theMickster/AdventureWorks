import { describe, it, expect, beforeEach } from 'vitest';
import { Component, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { InputFieldComponent } from './input-field';

@Component({
  standalone: true,
  imports: [InputFieldComponent, ReactiveFormsModule],
  template: `<aw-input-field
    [fieldId]="'test-input'"
    [formControl]="control"
    [label]="'Name'"
    [hint]="'Enter name'"
    [errors]="errors()"
  />`,
})
class TestHostComponent {
  control = new FormControl('');
  errors = signal<Record<string, string> | null>(null);
}

describe('InputFieldComponent', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;
  let el: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
      providers: [provideTranslateService()],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
    el = fixture.nativeElement as HTMLElement;
  });

  it('should render label linked via for/id', () => {
    const label = el.querySelector('label') as HTMLLabelElement;
    const input = el.querySelector('input') as HTMLInputElement;
    expect(label).toBeTruthy();
    expect(label.getAttribute('for')).toBe('test-input-input');
    expect(input.id).toBe('test-input-input');
  });

  it('should bind value from FormControl', () => {
    host.control.setValue('Hello');
    fixture.detectChanges();
    const input = el.querySelector('input') as HTMLInputElement;
    expect(input.value).toBe('Hello');
  });

  it('should update FormControl when user types', () => {
    const input = el.querySelector('input') as HTMLInputElement;
    input.value = 'World';
    input.dispatchEvent(new Event('input'));
    expect(host.control.value).toBe('World');
  });

  it('should show hint text when no errors', () => {
    const hint = el.querySelector('#test-input-hint') as HTMLElement;
    expect(hint).toBeTruthy();
    expect(hint.textContent).toContain('Enter name');
  });

  it('should show error messages and hide hint when errors provided', () => {
    host.errors.set({ required: 'Field is required' });
    fixture.detectChanges();

    const hint = el.querySelector('#test-input-hint');
    expect(hint).toBeNull();

    const error = el.querySelector('#test-input-error-required') as HTMLElement;
    expect(error).toBeTruthy();
    expect(error.textContent).toContain('Field is required');
  });

  it('should set aria-invalid to true when errors exist', () => {
    host.errors.set({ required: 'Required' });
    fixture.detectChanges();
    const input = el.querySelector('input') as HTMLInputElement;
    expect(input.getAttribute('aria-invalid')).toBe('true');
  });

  it('should set aria-invalid to false when no errors', () => {
    const input = el.querySelector('input') as HTMLInputElement;
    expect(input.getAttribute('aria-invalid')).toBe('false');
  });

  it('should set aria-describedby to error IDs when errors present', () => {
    host.errors.set({ required: 'Required', minlength: 'Too short' });
    fixture.detectChanges();
    const input = el.querySelector('input') as HTMLInputElement;
    expect(input.getAttribute('aria-describedby')).toBe('test-input-error-required test-input-error-minlength');
  });

  it('should set aria-describedby to hint ID when hint and no errors', () => {
    const input = el.querySelector('input') as HTMLInputElement;
    expect(input.getAttribute('aria-describedby')).toBe('test-input-hint');
  });

  it('should propagate disabled state from FormControl', () => {
    host.control.disable();
    fixture.detectChanges();
    const input = el.querySelector('input') as HTMLInputElement;
    expect(input.disabled).toBe(true);
  });
});
