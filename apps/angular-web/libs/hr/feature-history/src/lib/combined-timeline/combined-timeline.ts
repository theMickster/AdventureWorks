import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import type { EmployeeDepartmentHistory, EmployeePayHistory } from '@adventureworks-web/hr/data-access';
import { TimelineEntryComponent } from '../timeline-entry/timeline-entry';
import { buildCombinedTimeline, CombinedTimelineGroup } from './build-combined-timeline';

type TimelineFilter = 'all' | 'departments' | 'pay';

/** Merged department + pay history timeline with an All/Departments Only/Pay Only filter toggle (US-770). */
@Component({
  selector: 'aw-combined-timeline',
  standalone: true,
  imports: [DatePipe, CurrencyPipe, TimelineEntryComponent],
  templateUrl: './combined-timeline.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CombinedTimelineComponent {
  readonly departmentHistory = input.required<EmployeeDepartmentHistory[]>();
  readonly payHistory = input.required<EmployeePayHistory[]>();

  protected readonly filter = signal<TimelineFilter>('all');

  private readonly groups = computed(() => buildCombinedTimeline(this.departmentHistory(), this.payHistory()));

  /** Filters the already-built grouped structure by event kind and drops now-empty groups. */
  protected readonly filteredGroups = computed((): CombinedTimelineGroup[] => {
    const filter = this.filter();
    if (filter === 'all') {
      return this.groups();
    }
    const kind = filter === 'departments' ? 'department' : 'pay';
    return this.groups()
      .map((group) => ({ ...group, events: group.events.filter((e) => e.kind === kind) }))
      .filter((group) => group.events.length > 0);
  });

  protected onFilterChange(filter: TimelineFilter): void {
    this.filter.set(filter);
  }
}
