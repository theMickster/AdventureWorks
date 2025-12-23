import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

@Component({
  selector: 'aw-purchase-order-detail-placeholder',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div id="aw-purchase-order-detail-placeholder-page">
      <a id="aw-purchase-order-detail-placeholder-back" [routerLink]="['/purchasing/vendors']" class="btn btn-ghost btn-sm mb-6">
        <i class="fa-solid fa-arrow-left" aria-hidden="true"></i>
        Vendors
      </a>
      <div class="card bg-base-100 shadow">
        <div class="card-body">
          <h1 id="aw-purchase-order-detail-placeholder-title" class="card-title text-2xl font-bold text-base-content">
            Purchase Order PO{{ purchaseOrderId() }}
          </h1>
          <p class="text-secondary">A full purchase order detail view has not been built yet.</p>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Bare-bones placeholder at `/purchasing/purchase-orders/:id`. The vendor purchase order history
 * table links each row to this route (an explicit US-985 AC scenario), but no full PO detail
 * feature was in scope for #978 — this stub exists solely so that link has a real destination
 * instead of falling through to the app-wide `NotFoundComponent`. A future story can replace this
 * with a real detail view.
 */
export class PurchaseOrderDetailPlaceholderComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly purchaseOrderId = signal(0);

  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    const id = Math.trunc(Number(rawId));
    if (!id || id <= 0) {
      void this.router.navigate(['/purchasing/vendors']);
      return;
    }

    this.purchaseOrderId.set(id);
  }
}
