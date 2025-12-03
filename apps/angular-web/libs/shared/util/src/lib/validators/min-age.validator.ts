import { ValidatorFn } from '@angular/forms';

/**
 * Rejects a date-of-birth control value that does not satisfy a minimum age in whole years,
 * measured against today's date. Returns `null` (passes) for an empty value — pair with
 * `Validators.required` separately when the field is mandatory. On failure, sets `{ minAge: true }`.
 *
 * Extracted from `sales-person-create.ts` for reuse by `employee-create.ts` (both enforce an
 * 18-year minimum age, mirroring server-side `Rule-20`/`Rule-22` FluentValidation rules).
 */
export function minAgeValidator(years: number): ValidatorFn {
  return (ctrl) => {
    if (!ctrl.value) {
      return null;
    }
    const dob = new Date(ctrl.value as string);
    const cutoff = new Date();
    cutoff.setFullYear(cutoff.getFullYear() - years);
    return dob <= cutoff ? null : { minAge: true };
  };
}
