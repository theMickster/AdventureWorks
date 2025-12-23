import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import type { EmployeeDepartmentHistory } from '@adventureworks-web/hr/data-access';
import { StatusBadgeComponent } from '@adventureworks-web/shared/ui';
import { TimelineEntryComponent } from '../timeline-entry/timeline-entry';

const CURRENT_BADGE_MAP: Record<string, string> = { current: 'badge-success' };

/** Department assignment history rendered most-recent-first (US-768). */
@Component({
  selector: 'aw-department-timeline',
  standalone: true,
  imports: [DatePipe, TimelineEntryComponent, StatusBadgeComponent],
  templateUrl: './department-timeline.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DepartmentTimelineComponent {
  readonly departmentHistory = input.required<EmployeeDepartmentHistory[]>();

  protected readonly currentBadgeMap = CURRENT_BADGE_MAP;

  /** Defensive sort by startDate descending — API order is not contractually guaranteed. */
  protected readonly sortedHistory = computed(() =>
    [...this.departmentHistory()].sort((a, b) => b.startDate.localeCompare(a.startDate)),
  );
}
