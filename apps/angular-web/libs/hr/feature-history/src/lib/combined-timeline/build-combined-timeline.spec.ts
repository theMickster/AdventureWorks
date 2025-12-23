import type { EmployeeDepartmentHistory, EmployeePayHistory } from '@adventureworks-web/hr/data-access';
import { buildCombinedTimeline, computePayDeltas } from './build-combined-timeline';

// Employee 250 (Sheela Word) — verified against the local AdventureWorks DB
const departmentHistory: EmployeeDepartmentHistory[] = [
  {
    departmentId: 4,
    departmentName: 'Marketing',
    shiftId: 1,
    shiftName: 'Day',
    startDate: '2011-02-25',
    endDate: '2011-07-30',
  },
  {
    departmentId: 13,
    departmentName: 'Quality Assurance',
    shiftId: 1,
    shiftName: 'Day',
    startDate: '2011-07-31',
    endDate: '2012-07-14',
  },
  {
    departmentId: 5,
    departmentName: 'Purchasing',
    shiftId: 1,
    shiftName: 'Day',
    startDate: '2012-07-15',
    endDate: null,
  },
];

const payHistory: EmployeePayHistory[] = [
  { rateChangeDate: '2011-02-21', rate: 9.86, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
  { rateChangeDate: '2011-07-30', rate: 22.5, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
  { rateChangeDate: '2012-07-14', rate: 30.0, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
];

describe('computePayDeltas', () => {
  it('marks the oldest entry as initial', () => {
    const deltas = computePayDeltas(payHistory);
    expect(deltas[0]).toEqual({ kind: 'initial' });
  });

  it('marks a higher rate than the previous entry as an increase with the correct amount', () => {
    const deltas = computePayDeltas(payHistory);
    expect(deltas[1]).toEqual({ kind: 'increase', amount: 12.64 });
    expect(deltas[2]).toEqual({ kind: 'increase', amount: 7.5 });
  });

  it('marks a lower rate than the previous entry as a decrease with a positive amount', () => {
    const decreasing: EmployeePayHistory[] = [
      { rateChangeDate: '2020-01-01', rate: 30, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
      { rateChangeDate: '2020-06-01', rate: 25, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
    ];
    const deltas = computePayDeltas(decreasing);
    expect(deltas[1]).toEqual({ kind: 'decrease', amount: 5 });
  });

  it('marks an equal rate as unchanged', () => {
    const flat: EmployeePayHistory[] = [
      { rateChangeDate: '2020-01-01', rate: 30, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
      { rateChangeDate: '2020-06-01', rate: 30, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
    ];
    const deltas = computePayDeltas(flat);
    expect(deltas[1]).toEqual({ kind: 'unchanged' });
  });

  it('returns an empty array for empty input', () => {
    expect(computePayDeltas([])).toEqual([]);
  });

  it('marks a single entry as initial', () => {
    const single: EmployeePayHistory[] = [
      { rateChangeDate: '2020-01-01', rate: 30, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
    ];
    expect(computePayDeltas(single)).toEqual([{ kind: 'initial' }]);
  });

  it('computes correct deltas regardless of input order — defensively re-sorts before comparing', () => {
    const outOfOrder = [payHistory[2], payHistory[0], payHistory[1]];
    const deltas = computePayDeltas(outOfOrder);

    // outOfOrder[1] is the oldest (2011-02-21)
    expect(deltas[1]).toEqual({ kind: 'initial' });
    // outOfOrder[2] is 2011-07-30, an increase over 2011-02-21
    expect(deltas[2]).toEqual({ kind: 'increase', amount: 12.64 });
    // outOfOrder[0] is 2012-07-14, an increase over 2011-07-30
    expect(deltas[0]).toEqual({ kind: 'increase', amount: 7.5 });
  });
});

describe('buildCombinedTimeline', () => {
  it('merges and sorts all events descending by date', () => {
    const groups = buildCombinedTimeline(departmentHistory, payHistory);
    const dates = groups.map((g) => g.date);
    expect(dates).toEqual(['2012-07-15', '2012-07-14', '2011-07-31', '2011-07-30', '2011-02-25', '2011-02-21']);
  });

  it('puts department events before pay events when they share the same date', () => {
    const sameDayDept: EmployeeDepartmentHistory[] = [
      {
        departmentId: 1,
        departmentName: 'Engineering',
        shiftId: 1,
        shiftName: 'Day',
        startDate: '2020-06-01',
        endDate: null,
      },
    ];
    const sameDayPay: EmployeePayHistory[] = [
      { rateChangeDate: '2020-06-01', rate: 40, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
    ];

    const groups = buildCombinedTimeline(sameDayDept, sameDayPay);

    expect(groups).toHaveLength(1);
    expect(groups[0].date).toBe('2020-06-01');
    expect(groups[0].events.map((e) => e.kind)).toEqual(['department', 'pay']);
  });

  it('groups consecutive same-date events into a single CombinedTimelineGroup', () => {
    const sameDayDept: EmployeeDepartmentHistory[] = [
      {
        departmentId: 1,
        departmentName: 'Engineering',
        shiftId: 1,
        shiftName: 'Day',
        startDate: '2020-06-01',
        endDate: null,
      },
    ];
    const sameDayPay: EmployeePayHistory[] = [
      { rateChangeDate: '2020-06-01', rate: 40, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
    ];

    const groups = buildCombinedTimeline(sameDayDept, sameDayPay);

    expect(groups).toHaveLength(1);
    expect(groups[0].events).toHaveLength(2);
  });

  it('sets isCurrent true only for the department entry with a null endDate', () => {
    const groups = buildCombinedTimeline(departmentHistory, []);
    const deptEvents = groups.flatMap((g) => g.events).filter((e) => e.kind === 'department');

    expect(deptEvents.find((e) => e.departmentId === 5)?.isCurrent).toBe(true);
    expect(deptEvents.find((e) => e.departmentId === 4)?.isCurrent).toBe(false);
    expect(deptEvents.find((e) => e.departmentId === 13)?.isCurrent).toBe(false);
  });

  it('returns an empty array when both inputs are empty', () => {
    expect(buildCombinedTimeline([], [])).toEqual([]);
  });

  it('handles an empty department history with non-empty pay history', () => {
    const groups = buildCombinedTimeline([], payHistory);
    expect(groups).toHaveLength(3);
    expect(groups.every((g) => g.events.every((e) => e.kind === 'pay'))).toBe(true);
  });

  it('handles an empty pay history with non-empty department history', () => {
    const groups = buildCombinedTimeline(departmentHistory, []);
    expect(groups).toHaveLength(3);
    expect(groups.every((g) => g.events.every((e) => e.kind === 'department'))).toBe(true);
  });
});
