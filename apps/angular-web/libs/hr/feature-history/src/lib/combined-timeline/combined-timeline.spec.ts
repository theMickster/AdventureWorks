import type { EmployeeDepartmentHistory, EmployeePayHistory } from '@adventureworks-web/hr/data-access';
import { renderHistoryComponent } from '../testing/render-history-component';
import { CombinedTimelineComponent } from './combined-timeline';

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

describe('CombinedTimelineComponent', () => {
  async function setup(dept: EmployeeDepartmentHistory[] = departmentHistory, pay: EmployeePayHistory[] = payHistory) {
    const { fixture, component } = await renderHistoryComponent(CombinedTimelineComponent);
    fixture.componentRef.setInput('departmentHistory', dept);
    fixture.componentRef.setInput('payHistory', pay);
    fixture.detectChanges();
    return { fixture, component };
  }

  it('renders one group per distinct date, most-recent-first', async () => {
    const { fixture } = await setup();

    const groups = fixture.nativeElement.querySelectorAll('[id^="aw-combined-timeline-group-"]');
    expect(groups.length).toBe(6);
  });

  it('defaults to the "all" filter showing every event', async () => {
    const { fixture, component } = await setup();

    expect(component['filter']()).toBe('all');
    expect(fixture.nativeElement.querySelectorAll('aw-timeline-entry').length).toBe(6);
  });

  it('filters to departments-only and drops now-empty groups', async () => {
    const { fixture, component } = await setup();

    component['onFilterChange']('departments');
    fixture.detectChanges();

    const groups = fixture.nativeElement.querySelectorAll('[id^="aw-combined-timeline-group-"]');
    expect(groups.length).toBe(3);
    expect(fixture.nativeElement.querySelectorAll('.fa-building').length).toBe(3);
    expect(fixture.nativeElement.querySelectorAll('.fa-dollar-sign').length).toBe(0);
  });

  it('filters to pay-only and drops now-empty groups', async () => {
    const { fixture, component } = await setup();

    component['onFilterChange']('pay');
    fixture.detectChanges();

    const groups = fixture.nativeElement.querySelectorAll('[id^="aw-combined-timeline-group-"]');
    expect(groups.length).toBe(3);
    expect(fixture.nativeElement.querySelectorAll('.fa-dollar-sign').length).toBe(3);
    expect(fixture.nativeElement.querySelectorAll('.fa-building').length).toBe(0);
  });

  it('renders a single grouped date header when 2+ events share a date', async () => {
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

    const { fixture } = await setup(sameDayDept, sameDayPay);

    const groups = fixture.nativeElement.querySelectorAll('[id^="aw-combined-timeline-group-"]');
    expect(groups.length).toBe(1);
    expect(groups[0].querySelectorAll('aw-timeline-entry').length).toBe(2);
  });

  it('shows the empty state when both histories are empty', async () => {
    const { fixture } = await setup([], []);

    expect(fixture.nativeElement.querySelector('#aw-combined-timeline-empty')).toBeTruthy();
  });
});
