import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'aw-order-detail',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './order-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Placeholder sales-order detail view at `/sales/orders/:id`. The full detail view is delivered in US-737;
 * this stub reads the route `:id` from the snapshot and renders a back link to the list.
 */
export class OrderDetailComponent {
  private readonly route = inject(ActivatedRoute);

  protected readonly salesOrderId = signal(Number(this.route.snapshot.paramMap.get('id')));
}
