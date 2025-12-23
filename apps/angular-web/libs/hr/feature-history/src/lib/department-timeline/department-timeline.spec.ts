import { provideTranslateService } from '@ngx-translate/core';
import type { EmployeeDepartmentHistory } from '@adventureworks-web/hr/data-access';
import { renderHistoryComponent } from '../testing/render-history-component';
import { DepartmentTimelineComponent } from './department-timeline';

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

describe('DepartmentTimelineComponent', () => {
  async function setup(history: EmployeeDepartmentHistory[] = departmentHistory) {
    const { fixture, component } = await renderHistoryComponent(DepartmentTimelineComponent, [
      provideTranslateService(),
    ]);
    fixture.componentRef.setInput('departmentHistory', history);
    fixture.detectChanges();
    return { fixture, component };
  }

  it('renders most-recent-first, regardless of input order', async () => {
    const shuffled = [departmentHistory[1], departmentHistory[2], departmentHistory[0]];
    const { fixture } = await setup(shuffled);

    const titles = Array.from<Element>(
      fixture.nativeElement.querySelectorAll('#aw-department-timeline p.font-medium'),
    ).map((el) => el.textContent?.trim());
    expect(titles).toEqual(['Purchasing', 'Quality Assurance', 'Marketing']);
  });

  it('shows the "Current" badge only for the entry with a null endDate', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('#aw-department-timeline-current-badge-0')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-department-timeline-current-badge-1')).toBeFalsy();
    expect(fixture.nativeElement.querySelector('#aw-department-timeline-current-badge-2')).toBeFalsy();
  });

  it('gives the current entry a distinct border', async () => {
    const { fixture } = await setup();

    const currentEntry = fixture.nativeElement.querySelector('#aw-department-timeline-entry-0');
    expect(currentEntry.className).toContain('border-success');
  });

  it('renders nothing for an empty history', async () => {
    const { fixture } = await setup([]);

    expect(fixture.nativeElement.querySelector('#aw-department-timeline').children.length).toBe(0);
  });
});
