import { describe, it, expect, beforeEach } from 'vitest';
import { Component, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { TextareaFieldComponent } from './textarea-field';

@Component({
  standalone: true,
  imports: [TextareaFieldComponent, ReactiveFormsModule],
  template: `<aw-textarea-field
    [fieldId]="'test-textarea'"
    [formControl]="control"
    [label]="'Description'"
    [hint]="'Enter description'"
    [errors]="errors()"
  />`,
})
class TestHostComponent {
  control = new FormControl('');
  errors = signal<Record<string, string> | null>(null);
}

describe('TextareaFieldComponent', () => {
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
    const textarea = el.querySelector('textarea') as HTMLTextAreaElement;
    expect(label).toBeTruthy();
    expect(label.getAttribute('for')).toBe('test-textarea-textarea');
    expect(textarea.id).toBe('test-textarea-textarea');
  });

  it('should bind value from FormControl', () => {
    host.control.setValue('Some text');
    fixture.detectChanges();
    const textarea = el.querySelector('textarea') as HTMLTextAreaElement;
    expect(textarea.value).toBe('Some text');
  });

  it('should update FormControl when user types', () => {
    const textarea = el.querySelector('textarea') as HTMLTextAreaElement;
    textarea.value = 'New text';
    textarea.dispatchEvent(new Event('input'));
    expect(host.control.value).toBe('New text');
  });

  it('should show hint when no errors', () => {
    const hint = el.querySelector('#test-textarea-hint') as HTMLElement;
    expect(hint).toBeTruthy();
    expect(hint.textContent).toContain('Enter description');
  });

  it('should show errors and hide hint', () => {
    host.errors.set({ required: 'Required field' });
    fixture.detectChanges();
    expect(el.querySelector('#test-textarea-hint')).toBeNull();
    const error = el.querySelector('#test-textarea-error-required') as HTMLElement;
    expect(error).toBeTruthy();
    expect(error.textContent).toContain('Required field');
  });

  it('should set aria-invalid when errors exist', () => {
    host.errors.set({ required: 'Required' });
    fixture.detectChanges();
    const textarea = el.querySelector('textarea') as HTMLTextAreaElement;
    expect(textarea.getAttribute('aria-invalid')).toBe('true');
  });

  it('should set aria-describedby to hint ID when no errors', () => {
    const textarea = el.querySelector('textarea') as HTMLTextAreaElement;
    expect(textarea.getAttribute('aria-describedby')).toBe('test-textarea-hint');
  });

  it('should propagate disabled state', () => {
    host.control.disable();
    fixture.detectChanges();
    const textarea = el.querySelector('textarea') as HTMLTextAreaElement;
    expect(textarea.disabled).toBe(true);
  });

  it('should render with default rows of 3', () => {
    const textarea = el.querySelector('textarea') as HTMLTextAreaElement;
    expect(textarea.rows).toBe(3);
  });
});
