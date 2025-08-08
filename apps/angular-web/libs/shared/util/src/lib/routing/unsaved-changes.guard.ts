import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { ConfirmService } from '../confirm/confirm.service';

/** Implement this interface on routed components that have forms with unsaved changes. */
export interface HasUnsavedChanges {
  hasUnsavedChanges(): boolean;
}

/** Functional canDeactivate guard that prompts the user via ConfirmService before navigating away from dirty forms. */
export const unsavedChangesGuard: CanDeactivateFn<HasUnsavedChanges> = (component) => {
  if (!component.hasUnsavedChanges()) {
    return true;
  }

  return inject(ConfirmService).confirm({
    title: 'Unsaved Changes',
    message: 'You have unsaved changes. Are you sure you want to leave this page?',
    confirmLabel: 'Discard Changes',
    cancelLabel: 'Stay',
    variant: 'error',
  });
};
