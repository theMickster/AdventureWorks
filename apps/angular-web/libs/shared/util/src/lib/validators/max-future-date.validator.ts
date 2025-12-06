import { ValidatorFn } from '@angular/forms';

/**
 * Rejects a date control value more than `days` days after today (UTC calendar date).
 * Returns `null` (passes) for an empty value. On failure, sets `{ maxFutureDate: true }`.
 *
 * Mirrors the server's `MaximumFutureHireDays`/`MaximumFutureTerminationDays` window checks
 * in `HumanResourcesConstants.cs`.
 */
export function maxFutureDateValidator(days: number): ValidatorFn {
  return (ctrl) => {
    if (!ctrl.value) {
      return null;
    }
    const value = new Date(ctrl.value as string);
    const today = new Date();
    const todayUtc = Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate());
    const maxUtc = todayUtc + days * 24 * 60 * 60 * 1000;
    return value.getTime() <= maxUtc ? null : { maxFutureDate: true };
  };
}
