import { describe, it, expect, beforeEach } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { StatusBadgeComponent } from './status-badge';

describe('StatusBadgeComponent', () => {
  let fixture: ComponentFixture<StatusBadgeComponent>;
  let component: StatusBadgeComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatusBadgeComponent],
      providers: [provideTranslateService()],
    }).compileComponents();

    fixture = TestBed.createComponent(StatusBadgeComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('status', 'active');
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should map active to badge-success class', () => {
    const span = fixture.nativeElement.querySelector('span') as HTMLElement;
    expect(span.className).toBe('badge badge-success');
  });

  it('should map error to badge-error class', () => {
    fixture.componentRef.setInput('status', 'error');
    fixture.detectChanges();
    const span = fixture.nativeElement.querySelector('span') as HTMLElement;
    expect(span.className).toBe('badge badge-error');
  });

  it('should allow custom statusMap to override defaults', () => {
    fixture.componentRef.setInput('status', 'active');
    fixture.componentRef.setInput('statusMap', { active: 'badge-info' });
    fixture.detectChanges();
    const span = fixture.nativeElement.querySelector('span') as HTMLElement;
    expect(span.className).toBe('badge badge-info');
  });

  it('should fall back to badge-outline for unknown status', () => {
    fixture.componentRef.setInput('status', 'unknown-status');
    fixture.detectChanges();
    const span = fixture.nativeElement.querySelector('span') as HTMLElement;
    expect(span.className).toBe('badge badge-outline');
  });

  it('should render status text', () => {
    fixture.componentRef.setInput('status', 'pending');
    fixture.detectChanges();
    const span = fixture.nativeElement.querySelector('span') as HTMLElement;
    expect(span.textContent).toContain('pending');
  });
});
