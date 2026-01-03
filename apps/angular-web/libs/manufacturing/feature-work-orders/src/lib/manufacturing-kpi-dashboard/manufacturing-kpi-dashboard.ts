import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { WorkOrderApiService } from '@adventureworks-web/manufacturing/data-access';
import type { ManufacturingKpisDto } from '@adventureworks-web/manufacturing/data-access';
import { CardComponent, EmptyStateComponent, SkeletonComponent } from '@adventureworks-web/shared/ui';

@Component({
  selector: 'aw-manufacturing-kpi-dashboard',
  standalone: true,
  imports: [CardComponent, EmptyStateComponent, SkeletonComponent],
  templateUrl: './manufacturing-kpi-dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/** Manufacturing KPI dashboard at `/manufacturing`. */
export class ManufacturingKpiDashboardComponent implements OnInit {
  private readonly workOrderApi = inject(WorkOrderApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly kpis = signal<ManufacturingKpisDto | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly hasError = signal(false);

  ngOnInit(): void {
    this.loadKpis();
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
