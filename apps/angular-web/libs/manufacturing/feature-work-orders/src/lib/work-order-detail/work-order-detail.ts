import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { WorkOrderApiService } from '@adventureworks-web/manufacturing/data-access';
import type { WorkOrderDetail } from '@adventureworks-web/manufacturing/data-access';
import { CardComponent, EmptyStateComponent, SkeletonComponent, StatusBadgeComponent } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';
import { WORK_ORDER_DETAIL_STATUS_BADGE_MAP } from '../work-order-status-badge';

@Component({
  selector: 'aw-work-order-detail',
  standalone: true,
  imports: [RouterLink, DatePipe, SkeletonComponent, EmptyStateComponent, StatusBadgeComponent, CardComponent],
  templateUrl: './work-order-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Full work-order detail view at `/manufacturing/work-orders/:id`. Calls WorkOrderApiService
 * directly; no NgRx store. Mirrors the OrderDetailComponent pattern: isLoading/notFound/hasError
 * signals, error toast via constructor effect().
 */
export class WorkOrderDetailComponent implements OnInit {
  private readonly workOrderApi = inject(WorkOrderApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly workOrder = signal<WorkOrderDetail | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly notFound = signal(false);
  protected readonly hasError = signal(false);

  protected readonly statusBadgeMap = WORK_ORDER_DETAIL_STATUS_BADGE_MAP;

  constructor() {
    effect(() => {
      if (this.hasError()) {
        this.notificationService.error('Failed to load work order. Please try again.');
      }
    });
  }

  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    const id = Math.trunc(Number(rawId));
    if (!id || id <= 0) {
      void this.router.navigate(['/manufacturing/work-orders']);
      return;
    }
    this.isLoading.set(true);
    this.workOrderApi
      .getWorkOrder(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (detail) => {
          this.workOrder.set(detail);
          this.isLoading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          if (err.status === 404) {
            this.notFound.set(true);
          } else {
            this.hasError.set(true);
          }
          this.isLoading.set(false);
        },
      });
  }
}
