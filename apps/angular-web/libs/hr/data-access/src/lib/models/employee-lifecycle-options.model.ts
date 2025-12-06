/** Hardcoded shift options — no lookup endpoint exists. Mirrors `HumanResourcesConstants.MinimumShiftId`/`MaximumShiftId` (1-3). */
export const SHIFT_OPTIONS = [
  { value: 1, label: 'Day' },
  { value: 2, label: 'Evening' },
  { value: 3, label: 'Night' },
] as const;

/** Hardcoded pay frequency options — no lookup endpoint exists. Mirrors `HumanResourcesConstants.PayFrequencyMonthly`/`PayFrequencyBiWeekly`. */
export const PAY_FREQUENCY_OPTIONS = [
  { value: 1, label: 'Monthly' },
  { value: 2, label: 'Bi-weekly' },
] as const;

/** Hardcoded termination types — no lookup endpoint exists; mirrors `EmployeeTerminate.terminationType` on the API. */
export const TERMINATION_TYPE_OPTIONS = ['Voluntary', 'Involuntary', 'Retirement', 'Layoff'] as const;
