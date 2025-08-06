import { describe, it, expect, beforeEach } from 'vitest';
import { Component, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { SelectFieldComponent } from './select-field';

@Component({
  standalone: true,
  imports: [SelectFieldComponent, ReactiveFormsModule],
  template: `<aw-select-field
    [fieldId]="'test-select'"
    [formControl]="control"
    [label]="'Country'"
    [hint]="'Pick one'"
    [options]="options"
    [emptyOptionLabel]="'-- Select --'"
    [errors]="errors()"
  />`,
})
class TestHostComponent {
  control = new FormControl('');
  options = [
    { value: 'us', label: 'United States' },
    { value: 'ca', label: 'Canada' },
  ];
  errors = signal<Record<string, string> | null>(null);
}

describe('SelectFieldComponent', () => {
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

  it('should render options from input', () => {
    const options = el.querySelectorAll('option:not([disabled])');
    expect(options.length).toBe(2);
    expect(options[0].textContent).toContain('United States');
    expect(options[1].textContent).toContain('Canada');
  });

  it('should render empty option when emptyOptionLabel provided', () => {
    const emptyOpt = el.querySelector('option[disabled]') as HTMLOptionElement;
    expect(emptyOpt).toBeTruthy();
    expect(emptyOpt.textContent).toContain('-- Select --');
  });

  it('should bind to FormControl', () => {
    host.control.setValue('ca');
    fixture.detectChanges();
    const select = el.querySelector('select') as HTMLSelectElement;
    expect(select.value).toBe('ca');
  });

  it('should update FormControl on change', () => {
    const select = el.querySelector('select') as HTMLSelectElement;
    select.value = 'us';
    select.dispatchEvent(new Event('change'));
    expect(host.control.value).toBe('us');
  });

  it('should propagate disabled state', () => {
    host.control.disable();
    fixture.detectChanges();
    const select = el.querySelector('select') as HTMLSelectElement;
    expect(select.disabled).toBe(true);
  });

  it('should show error messages when errors provided', () => {
    host.errors.set({ required: 'Selection required' });
    fixture.detectChanges();
    const error = el.querySelector('#test-select-error-required') as HTMLElement;
    expect(error).toBeTruthy();
    expect(error.textContent).toContain('Selection required');
  });

  it('should set aria-invalid when errors exist', () => {
    host.errors.set({ required: 'Required' });
    fixture.detectChanges();
    const select = el.querySelector('select') as HTMLSelectElement;
    expect(select.getAttribute('aria-invalid')).toBe('true');
  });

  it('should show hint when no errors', () => {
    const hint = el.querySelector('#test-select-hint') as HTMLElement;
    expect(hint).toBeTruthy();
    expect(hint.textContent).toContain('Pick one');
  });
});
