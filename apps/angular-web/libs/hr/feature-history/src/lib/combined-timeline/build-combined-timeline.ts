import type { EmployeeDepartmentHistory, EmployeePayHistory } from '@adventureworks-web/hr/data-access';

/** A department assignment rendered as a timeline event. */
export interface DepartmentTimelineEvent {
  readonly kind: 'department';
  readonly date: string;
  readonly departmentId: number;
  readonly departmentName: string;
  readonly shiftName: string;
  readonly isCurrent: boolean;
}

/** How a pay history entry's rate compares to the chronologically-previous entry. */
export type PayDelta =
  | { readonly kind: 'initial' }
  | { readonly kind: 'increase'; readonly amount: number }
  | { readonly kind: 'decrease'; readonly amount: number }
  | { readonly kind: 'unchanged' };

/** A pay rate change rendered as a timeline event. */
export interface PayTimelineEvent {
  readonly kind: 'pay';
  readonly date: string;
  readonly rate: number;
  readonly payFrequencyLabel: string;
  readonly delta: PayDelta;
}

export type CombinedTimelineEvent = DepartmentTimelineEvent | PayTimelineEvent;

/** One or more timeline events that fall on the same calendar date. */
export interface CombinedTimelineGroup {
  readonly date: string;
  readonly events: CombinedTimelineEvent[];
}

/** Normalizes an ISO date/date-time string to its calendar-date (YYYY-MM-DD) portion. */
function toDateKey(isoDate: string): string {
  return isoDate.slice(0, 10);
}

/**
 * Computes each pay history entry's delta versus the chronologically-previous (next-older) entry.
 * Defensively re-sorts the input descending by `rateChangeDate` first — callers must not assume
 * the array arrives in any particular order. The earliest (oldest) entry is always `{kind:'initial'}`.
 *
 * Assumes at most one rate change per calendar date per employee (an accepted schema-level
 * simplification) — this does not attempt to handle multiple same-day changes.
 */
export function computePayDeltas(payHistory: readonly EmployeePayHistory[]): PayDelta[] {
  const sorted = [...payHistory].sort((a, b) => b.rateChangeDate.localeCompare(a.rateChangeDate));
  const deltaByDate = new Map<string, PayDelta>();

  for (let i = 0; i < sorted.length; i++) {
    const olderEntry = sorted[i + 1];
    if (!olderEntry) {
      deltaByDate.set(sorted[i].rateChangeDate, { kind: 'initial' });
      continue;
    }
    const amount = sorted[i].rate - olderEntry.rate;
    if (amount > 0) {
      deltaByDate.set(sorted[i].rateChangeDate, { kind: 'increase', amount });
    } else if (amount < 0) {
      deltaByDate.set(sorted[i].rateChangeDate, { kind: 'decrease', amount: Math.abs(amount) });
    } else {
      deltaByDate.set(sorted[i].rateChangeDate, { kind: 'unchanged' });
    }
  }

  // Matched back to the original array's order and length by rateChangeDate.
  return payHistory.map((entry) => deltaByDate.get(entry.rateChangeDate) as PayDelta);
}

/**
 * Merges department and pay history into a single, date-grouped timeline. Both inputs are mapped
 * to typed events, merged, and sorted descending by date — ties on the same date put department
 * events before pay events. Consecutive same-date events are grouped into `CombinedTimelineGroup[]`.
 *
 * This does no filtering — the All/Departments Only/Pay Only toggle is applied later, at render
 * time, over this already-built grouped structure.
 */
export function buildCombinedTimeline(
  departmentHistory: readonly EmployeeDepartmentHistory[],
  payHistory: readonly EmployeePayHistory[],
): CombinedTimelineGroup[] {
  const departmentEvents: DepartmentTimelineEvent[] = departmentHistory.map((entry) => ({
    kind: 'department',
    date: toDateKey(entry.startDate),
    departmentId: entry.departmentId,
    departmentName: entry.departmentName,
    shiftName: entry.shiftName,
    isCurrent: entry.endDate === null,
  }));

  const deltas = computePayDeltas(payHistory);
  const payEvents: PayTimelineEvent[] = payHistory.map((entry, i) => ({
    kind: 'pay',
    date: toDateKey(entry.rateChangeDate),
    rate: entry.rate,
    payFrequencyLabel: entry.payFrequencyLabel,
    delta: deltas[i],
  }));

  const allEvents: CombinedTimelineEvent[] = [...departmentEvents, ...payEvents].sort((a, b) => {
    const dateCompare = b.date.localeCompare(a.date);
    if (dateCompare !== 0) {
      return dateCompare;
    }
    if (a.kind === b.kind) {
      return 0;
    }
    return a.kind === 'department' ? -1 : 1;
  });

  const groups: CombinedTimelineGroup[] = [];
  for (const event of allEvents) {
    const lastGroup = groups.at(-1);
    if (lastGroup && lastGroup.date === event.date) {
      lastGroup.events.push(event);
    } else {
      groups.push({ date: event.date, events: [event] });
    }
  }
  return groups;
}
