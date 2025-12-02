import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { SalesPerson, SalesPersonPerformance } from '@adventureworks-web/sales/data-access';
import { CardComponent, EmptyStateComponent, SkeletonComponent } from '@adventureworks-web/shared/ui';
import { extractListNavParams, NotificationService } from '@adventureworks-web/shared/util';

type ActiveTab = 'profile' | 'performance';

/**
 * Detail view for a single sales person at /sales/persons/:id.
 * Profile tab shows static person data; Performance tab is lazy-loaded on first activation.
 */
@Component({
  selector: 'aw-sales-person-detail',
  standalone: true,
  imports: [RouterLink, CardComponent, SkeletonComponent, EmptyStateComponent],
  templateUrl: './sales-person-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesPersonDetailComponent implements OnInit {
  // Uses SalesApiService directly (no NgRx store) — one-shot single-entity read with no shared state
  private readonly salesApi = inject(SalesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  private personId = 0;

  protected readonly person = signal<SalesPerson | null>(null);
  protected readonly performance = signal<SalesPersonPerformance | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly isLoadingPerformance = signal(false);
  protected readonly hasPerformanceError = signal(false);
  protected readonly activeTab = signal<ActiveTab>('profile');

  protected readonly backQueryParams = computed(() =>
    // snapshot read is intentional: captures list nav state once at navigation time
    extractListNavParams(this.route.snapshot.queryParams),
  );

  protected readonly fullName = computed(() => {
    const p = this.person();
    if (!p) { return ''; }
    return [p.firstName, p.middleName, p.lastName].filter(Boolean).join(' ');
  });

  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    const id = Math.trunc(Number(rawId));
    if (!id || id <= 0) {
      void this.router.navigate(['/sales/persons']);
      return;
    }
    this.personId = id;
    this.isLoading.set(true);
    this.salesApi
      .getSalesPerson(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (p) => {
          this.person.set(p);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.notificationService.error('Failed to load sales person. Please try again.');
        },
      });
  }

  protected onTabChange(tab: ActiveTab): void {
    this.activeTab.set(tab);
    // Guard: skip if data already loaded (no re-fetch) or a request is in flight (no duplicate request)
    if (tab === 'performance' && !this.performance() && !this.isLoadingPerformance()) {
      this.loadPerformance();
    }
  }

  private loadPerformance(): void {
    this.hasPerformanceError.set(false);
    this.isLoadingPerformance.set(true);
    this.salesApi
      .getSalesPersonPerformance(this.personId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (perf) => {
          this.performance.set(perf);
          this.isLoadingPerformance.set(false);
        },
        error: () => {
          this.isLoadingPerformance.set(false);
          this.hasPerformanceError.set(true);
          this.notificationService.error('Failed to load performance data. Please try again.');
        },
      });
  }
}
