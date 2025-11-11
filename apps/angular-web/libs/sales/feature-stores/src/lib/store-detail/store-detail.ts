import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import { CardComponent, EmptyStateComponent, SkeletonComponent } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';
import type { Store } from '@adventureworks-web/sales/data-access';
import { extractListNavParams } from '../list-nav-params';

type ActiveTab = 'addresses' | 'contacts';

@Component({
  selector: 'aw-store-detail',
  standalone: true,
  imports: [RouterLink, CardComponent, SkeletonComponent, EmptyStateComponent],
  templateUrl: './store-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StoreDetailComponent implements OnInit {
  // Uses SalesApiService directly (no NgRx store) — one-shot single-entity read with no shared state or pagination
  private readonly salesApi = inject(SalesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly store = signal<Store | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly activeTab = signal<ActiveTab>('addresses');

  protected readonly backQueryParams = computed(() =>
    // snapshot read is intentional: captures list nav state once at navigation time
    extractListNavParams(this.route.snapshot.queryParams),
  );

  protected readonly salesPersonName = computed(() => {
    const sp = this.store()?.salesPerson;
    if (!sp) return '—';
    const parts = [sp.firstName, sp.middleName, sp.lastName].filter(Boolean);
    return parts.join(' ');
  });

  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    const id = Math.trunc(Number(rawId));
    if (!id || id <= 0) {
      void this.router.navigate(['/sales/stores']);
      return;
    }
    this.isLoading.set(true);
    this.salesApi
      .getStore(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (store) => {
          this.store.set(store);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.notificationService.error('Failed to load store. Please try again.');
        },
      });
  }

  protected onTabChange(tab: ActiveTab): void {
    this.activeTab.set(tab);
  }
}
