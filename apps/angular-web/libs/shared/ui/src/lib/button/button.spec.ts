import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { ButtonComponent } from './button';

describe('ButtonComponent', () => {
  let fixture: ComponentFixture<ButtonComponent>;
  let component: ButtonComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ButtonComponent],
      providers: [provideTranslateService()],
    }).compileComponents();

    fixture = TestBed.createComponent(ButtonComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render with btn btn-primary classes by default', () => {
    const button = fixture.nativeElement.querySelector('button');
    expect(button.className).toBe('btn btn-primary');
  });

  it('should render btn-accent when variant is accent', () => {
    fixture.componentRef.setInput('variant', 'accent');
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    expect(button.className).toBe('btn btn-accent');
  });

  it('should render btn-sm when size is sm', () => {
    fixture.componentRef.setInput('size', 'sm');
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    expect(button.className).toContain('btn-sm');
  });

  it('should not add btn-md class when size is md', () => {
    const button = fixture.nativeElement.querySelector('button');
    expect(button.className).not.toContain('btn-md');
  });

  it('should show spinner element when loading is true', () => {
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();
    const spinner = fixture.nativeElement.querySelector('.loading-spinner');
    expect(spinner).toBeTruthy();
  });

  it('should disable button when disabled is true', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    expect(button.disabled).toBe(true);
  });

  it('should disable button when loading is true', () => {
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    expect(button.disabled).toBe(true);
  });

  it('should emit clicked on click', () => {
    const clickedSpy = vi.fn();
    component.clicked.subscribe(clickedSpy);

    const button: HTMLButtonElement = fixture.nativeElement.querySelector('button');
    button.click();

    expect(clickedSpy).toHaveBeenCalledOnce();
    expect(clickedSpy.mock.calls[0][0]).toBeInstanceOf(MouseEvent);
  });

  it('should not emit clicked when disabled', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();

    const clickedSpy = vi.fn();
    component.clicked.subscribe(clickedSpy);

    const button: HTMLButtonElement = fixture.nativeElement.querySelector('button');
    button.click();

    expect(clickedSpy).not.toHaveBeenCalled();
  });
});
