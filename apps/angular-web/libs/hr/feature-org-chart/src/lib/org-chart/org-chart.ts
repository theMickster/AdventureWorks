import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { debounceTime, Subject } from 'rxjs';
import { ScrollIndicatorComponent, SkeletonComponent } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';
import { OrgNodeComponent } from '../org-node/org-node';
import { OrgChartStore } from '../stores/org-chart.store';

const SEARCH_DEBOUNCE_MS = 300;

/** Interactive org chart at `/hr/org-chart`: search/click-through, expand/collapse, department color-coding, summary stats. */
@Component({
  selector: 'aw-org-chart',
  imports: [DecimalPipe, ScrollIndicatorComponent, SkeletonComponent, OrgNodeComponent],
  templateUrl: './org-chart.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrgChartComponent implements OnInit {
  private readonly store = inject(OrgChartStore);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly searchInput$ = new Subject<string>();

  protected readonly isLoading = this.store.isLoading;
  protected readonly hasError = this.store.hasError;
  protected readonly tree = this.store.tree;
  protected readonly stats = this.store.stats;
  protected readonly expandedIds = this.store.expandedIds;
  protected readonly highlightedId = this.store.highlightedId;
  protected readonly searchTerm = signal('');

  constructor() {
    // Constructor placement ensures Angular evaluates this effect after ngOnInit's setLoading()
    // clears any stale error, preventing a spurious toast on revisit (mirrors DashboardComponent).
    effect(() => {
      if (this.hasError()) {
        this.notificationService.error('Failed to load the organization chart. Please try again.');
      }
    });
  }

  ngOnInit(): void {
    this.store.load();

    this.searchInput$.pipe(debounceTime(SEARCH_DEBOUNCE_MS), takeUntilDestroyed(this.destroyRef)).subscribe((term) => {
      this.store.searchAndExpand(term);
    });
  }

  protected onSearchInput(value: string): void {
    this.searchTerm.set(value);
    this.searchInput$.next(value);
  }

  protected onToggle(employeeId: number): void {
    this.store.toggleExpanded(employeeId);
  }

  protected onNodeClick(employeeId: number): void {
    this.router.navigate(['/hr/employees', employeeId]);
  }
}
