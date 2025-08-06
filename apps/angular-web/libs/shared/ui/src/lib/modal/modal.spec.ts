import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { ModalComponent } from './modal';

describe('ModalComponent', () => {
  let fixture: ComponentFixture<ModalComponent>;
  let component: ModalComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModalComponent],
      providers: [provideTranslateService()],
    }).compileComponents();

    fixture = TestBed.createComponent(ModalComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render dialog element with modal class', () => {
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    expect(dialog).toBeTruthy();
    expect(dialog.classList.contains('modal')).toBe(true);
  });

  it('should use default fieldId as dialog id', () => {
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    expect(dialog.id).toBe('aw-modal');
  });

  it('should use custom fieldId as dialog id', () => {
    fixture.componentRef.setInput('fieldId', 'aw-custom-modal');
    fixture.detectChanges();
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    expect(dialog.id).toBe('aw-custom-modal');
  });

  it('should open dialog when open is set to true', async () => {
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    dialog.showModal = vi.fn(() => {
      Object.defineProperty(dialog, 'open', { value: true, writable: true, configurable: true });
    });

    fixture.componentRef.setInput('open', true);
    await fixture.whenStable();

    expect(dialog.showModal).toHaveBeenCalled();
  });

  it('should close dialog when open is set to false', async () => {
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    dialog.showModal = vi.fn(() => {
      Object.defineProperty(dialog, 'open', { value: true, writable: true, configurable: true });
    });
    dialog.close = vi.fn(() => {
      Object.defineProperty(dialog, 'open', { value: false, writable: true, configurable: true });
    });

    fixture.componentRef.setInput('open', true);
    await fixture.whenStable();

    fixture.componentRef.setInput('open', false);
    await fixture.whenStable();

    expect(dialog.close).toHaveBeenCalled();
  });

  it('should emit closed output when dialog closes', async () => {
    const closedSpy = vi.fn();
    component.closed.subscribe(closedSpy);

    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    dialog.dispatchEvent(new Event('close'));

    expect(closedSpy).toHaveBeenCalledOnce();
  });

  it('should render title when provided', () => {
    fixture.componentRef.setInput('title', 'Test Title');
    fixture.detectChanges();
    const heading = fixture.nativeElement.querySelector('h3');
    expect(heading).toBeTruthy();
    expect(heading.textContent).toContain('Test Title');
  });

  it('should not render title when not provided', () => {
    const heading = fixture.nativeElement.querySelector('h3');
    expect(heading).toBeFalsy();
  });

  it('should set aria-labelledby when title is provided', () => {
    fixture.componentRef.setInput('title', 'Test Title');
    fixture.detectChanges();
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    expect(dialog.getAttribute('aria-labelledby')).toBe('aw-modal-title');
  });

  it('should not set aria-labelledby when title is empty', () => {
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    expect(dialog.getAttribute('aria-labelledby')).toBeNull();
  });
});
