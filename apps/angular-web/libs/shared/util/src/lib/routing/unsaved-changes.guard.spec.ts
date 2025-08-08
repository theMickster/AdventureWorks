import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ConfirmService } from '../confirm/confirm.service';
import { HasUnsavedChanges, unsavedChangesGuard } from './unsaved-changes.guard';

describe('unsavedChangesGuard', () => {
  let confirmService: ConfirmService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    confirmService = TestBed.inject(ConfirmService);
  });

  it('should allow navigation when component has no unsaved changes', () => {
    const component: HasUnsavedChanges = { hasUnsavedChanges: () => false };

    const result = TestBed.runInInjectionContext(() => unsavedChangesGuard(component, null!, null!, null!));

    expect(result).toBe(true);
  });

  it('should prompt via ConfirmService when component has unsaved changes', async () => {
    const component: HasUnsavedChanges = { hasUnsavedChanges: () => true };

    const result = TestBed.runInInjectionContext(() =>
      unsavedChangesGuard(component, null!, null!, null!),
    ) as Promise<boolean>;

    // ConfirmService should now be visible with the unsaved changes dialog
    expect(confirmService.state().visible).toBe(true);
    expect(confirmService.state().options.title).toBe('Unsaved Changes');

    // Simulate user confirming
    confirmService.resolve(true);
    expect(await result).toBe(true);
  });

  it('should block navigation when user cancels the dialog', async () => {
    const component: HasUnsavedChanges = { hasUnsavedChanges: () => true };

    const result = TestBed.runInInjectionContext(() =>
      unsavedChangesGuard(component, null!, null!, null!),
    ) as Promise<boolean>;

    confirmService.resolve(false);
    expect(await result).toBe(false);
  });
});
