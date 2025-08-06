import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ConfirmService } from './confirm.service';

describe('ConfirmService', () => {
  let service: ConfirmService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ConfirmService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have initial state with visible false', () => {
    expect(service.state().visible).toBe(false);
    expect(service.state().options.title).toBe('');
    expect(service.state().options.message).toBe('');
  });

  it('should set state to visible with options when confirm is called', () => {
    service.confirm({ title: 'Delete', message: 'Are you sure?' });

    expect(service.state().visible).toBe(true);
    expect(service.state().options.title).toBe('Delete');
    expect(service.state().options.message).toBe('Are you sure?');
  });

  it('should resolve promise with true when resolve(true) is called', async () => {
    const promise = service.confirm({ title: 'Test', message: 'Confirm?' });
    service.resolve(true);

    const result = await promise;
    expect(result).toBe(true);
  });

  it('should resolve promise with false when resolve(false) is called', async () => {
    const promise = service.confirm({ title: 'Test', message: 'Confirm?' });
    service.resolve(false);

    const result = await promise;
    expect(result).toBe(false);
  });

  it('should reset state when resolve is called', () => {
    service.confirm({ title: 'Test', message: 'Confirm?' });
    service.resolve(true);

    expect(service.state().visible).toBe(false);
    expect(service.state().options.title).toBe('');
    expect(service.state().options.message).toBe('');
  });

  it('should have undefined confirmLabel and cancelLabel by default', () => {
    service.confirm({ title: 'Test', message: 'Confirm?' });

    expect(service.state().options.confirmLabel).toBeUndefined();
    expect(service.state().options.cancelLabel).toBeUndefined();
  });

  it('should preserve custom labels in options', () => {
    service.confirm({
      title: 'Delete',
      message: 'Are you sure?',
      confirmLabel: 'Yes, delete',
      cancelLabel: 'No, keep',
      variant: 'error',
    });

    expect(service.state().options.confirmLabel).toBe('Yes, delete');
    expect(service.state().options.cancelLabel).toBe('No, keep');
    expect(service.state().options.variant).toBe('error');
  });
});
