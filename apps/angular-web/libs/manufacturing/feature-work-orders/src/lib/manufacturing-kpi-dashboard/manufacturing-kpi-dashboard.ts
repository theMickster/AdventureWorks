import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { DecimalPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { catchError, map, of, startWith } from 'rxjs';
import { WorkOrderApiService } from '@adventureworks-web/manufacturing/data-access';
import type {
  ManufacturingKpisDto,
  ManufacturingQualityScorecard,
} from '@adventureworks-web/manufacturing/data-access';
import { CardComponent, EmptyStateComponent, SkeletonComponent } from '@adventureworks-web/shared/ui';

type ScorecardViewState =
  | { readonly status: 'loading' }
  | { readonly status: 'error' }
  | { readonly status: 'loaded'; readonly scorecard: ManufacturingQualityScorecard };

const SCORECARD_LOADING: ScorecardViewState = { status: 'loading' };
const SCORECARD_SKELETON_ROWS = [1, 2, 3, 4, 5] as const;
const SCORECARD_ERROR: ScorecardViewState = { status: 'error' };

@Component({
  selector: 'aw-manufacturing-kpi-dashboard',
  standalone: true,
  imports: [CardComponent, DecimalPipe, EmptyStateComponent, RouterLink, SkeletonComponent],
  templateUrl: './manufacturing-kpi-dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/** Manufacturing KPI dashboard at `/manufacturing`. */
export class ManufacturingKpiDashboardComponent implements OnInit {
  private readonly workOrderApi = inject(WorkOrderApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  protected readonly skeletonRows = SCORECARD_SKELETON_ROWS;
  protected readonly kpis = signal<ManufacturingKpisDto | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly hasError = signal(false);

  /** One request feeds the product tables and the scrap breakdown; its failure never touches the KPI state. */
  protected readonly scorecardState = toSignal(
    this.workOrderApi.getQualityScorecard().pipe(
      map((scorecard): ScorecardViewState => ({ status: 'loaded', scorecard })),
      catchError(() => of(SCORECARD_ERROR)),
      startWith(SCORECARD_LOADING),
    ),
    { requireSync: true },
  );

  ngOnInit(): void {
    this.loadKpis();
  }

  protected onScrapReasonClick(scrapReasonId: number): void {
    this.router.navigate(['/manufacturing/work-orders'], { queryParams: { scrapReasonId } });
  }

  private loadKpis(): void {
    this.isLoading.set(true);
    this.workOrderApi
      .getKpis()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (kpis) => {
          this.kpis.set(kpis);
          this.isLoading.set(false);
        },
        error: () => {
          this.hasError.set(true);
          this.isLoading.set(false);
        },
      });
  }
}
