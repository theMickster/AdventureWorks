import { describe, it, expect, beforeEach } from 'vitest';
import { Component } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { ToggleFieldComponent } from './toggle-field';

@Component({
  standalone: true,
  imports: [ToggleFieldComponent, ReactiveFormsModule],
  template: `<aw-toggle-field [fieldId]="'test-toggle'" [formControl]="control" [label]="'Active'" />`,
})
class TestHostComponent {
  control = new FormControl(false);
}

describe('ToggleFieldComponent', () => {
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

  it('should render toggle with label', () => {
    const input = el.querySelector('input[type="checkbox"]') as HTMLInputElement;
    const span = el.querySelector('.label-text') as HTMLElement;
    expect(input).toBeTruthy();
    expect(span).toBeTruthy();
    expect(span.textContent).toContain('Active');
  });

  it('should bind boolean value from FormControl', () => {
    host.control.setValue(true);
    fixture.detectChanges();
    const input = el.querySelector('input[type="checkbox"]') as HTMLInputElement;
    expect(input.checked).toBe(true);
  });

  it('should update FormControl when toggled', () => {
    const input = el.querySelector('input[type="checkbox"]') as HTMLInputElement;
    input.checked = true;
    input.dispatchEvent(new Event('change'));
    expect(host.control.value).toBe(true);
  });

  it('should propagate disabled state', () => {
    host.control.disable();
    fixture.detectChanges();
    const input = el.querySelector('input[type="checkbox"]') as HTMLInputElement;
    expect(input.disabled).toBe(true);
  });

  it('should have linked label and input via for/id', () => {
    const label = el.querySelector('label') as HTMLLabelElement;
    const input = el.querySelector('input[type="checkbox"]') as HTMLInputElement;
    expect(label.getAttribute('for')).toBe('test-toggle-toggle');
    expect(input.id).toBe('test-toggle-toggle');
  });
});
