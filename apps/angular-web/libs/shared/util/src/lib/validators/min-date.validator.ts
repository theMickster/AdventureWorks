import { ValidatorFn } from '@angular/forms';

/**
 * Rejects a date control value earlier than `minDate` (UTC calendar date comparison).
 * Returns `null` (passes) for an empty value. On failure, sets `{ minDate: true }`.
 */
export function minDateValidator(minDate: Date): ValidatorFn {
  return (ctrl) => {
    if (!ctrl.value) {
      return null;
    }
    const value = new Date(ctrl.value as string);
    const minUtc = Date.UTC(minDate.getUTCFullYear(), minDate.getUTCMonth(), minDate.getUTCDate());
    return value.getTime() >= minUtc ? null : { minDate: true };
  };
}
