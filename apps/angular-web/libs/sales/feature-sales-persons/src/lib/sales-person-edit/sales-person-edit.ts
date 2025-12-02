import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { SalesPerson, SalesPersonSalesConfigUpdate } from '@adventureworks-web/sales/data-access';
import { LookupApiService } from '@adventureworks-web/shared/data-access';
import type { SalesTerritory } from '@adventureworks-web/shared/data-access';
import { InputFieldComponent, SelectFieldComponent, SkeletonComponent } from '@adventureworks-web/shared/ui';
import { extractListNavParams, NotificationService } from '@adventureworks-web/shared/util';

@Component({
  selector: 'aw-sales-person-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputFieldComponent, SelectFieldComponent, SkeletonComponent],
  templateUrl: './sales-person-edit.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalesPersonEditComponent implements OnInit {
  private readonly salesApi = inject(SalesApiService);
  private readonly lookupApi = inject(LookupApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly salesPerson = signal<SalesPerson | null>(null);
  protected readonly territories = signal<SalesTerritory[]>([]);

  protected personId = 0;

  protected readonly form = this.fb.group({
    territoryId: [null as string | null],
    salesQuota: [null as string | null],
    bonus: ['0'],
    commissionPct: ['0'],
  });

  protected readonly territoryOptions = computed(() =>
    this.territories().map((t) => ({
      value: t.id,
      label: t.name,
    })),
  );

  protected readonly fullName = computed(() => {
    const p = this.salesPerson();
    if (!p) { return ''; }
    return [p.firstName, p.middleName, p.lastName].filter(Boolean).join(' ');
  });

  protected readonly backQueryParams = computed(() =>
    extractListNavParams(this.route.snapshot.queryParams),
  );

  /** Reads the sales person ID from the route, then loads the sales person and territory list in parallel via `forkJoin` and patches the form. Navigates to the detail view on load failure. */
  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    this.personId = Math.trunc(Number(rawId));

    if (!this.personId || this.personId <= 0) {
      void this.router.navigate(['/sales/persons']);
      return;
    }

    forkJoin({
      salesPerson: this.salesApi.getSalesPerson(this.personId),
      territories: this.lookupApi.getTerritories(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ salesPerson, territories }) => {
          this.salesPerson.set(salesPerson);
          this.territories.set(territories);
          this.form.patchValue({
            territoryId: salesPerson.territoryId != null ? String(salesPerson.territoryId) : null,
            salesQuota: salesPerson.salesQuota != null ? String(salesPerson.salesQuota) : null,
            bonus: String(salesPerson.bonus),
            commissionPct: String(Math.round(salesPerson.commissionPct * 10000) / 100),
          });
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.notificationService.error('Failed to load sales person. Please try again.');
          void this.router.navigate(['/sales/persons', this.personId]);
        },
      });
  }

  /** Builds the sales config patch payload from the form and calls the PATCH endpoint; the `isSaving` guard prevents duplicate submissions while the request is in flight. */
  protected onSubmit(): void {
    if (this.isSaving()) {
      return;
    }

    const rawTerritoryId = this.form.value.territoryId;
    const parsedTerritoryId = rawTerritoryId ? Number(rawTerritoryId) : null;

    const rawSalesQuota = this.form.value.salesQuota;
    const parsedSalesQuota = rawSalesQuota != null && rawSalesQuota !== '' ? Number(rawSalesQuota) : null;

    const model: SalesPersonSalesConfigUpdate = {
      id: this.personId,
      territoryId: parsedTerritoryId,
      salesQuota: parsedSalesQuota,
      bonus: Number(this.form.value.bonus ?? '0'),
      commissionPct: Math.round(Number(this.form.value.commissionPct ?? '0') * 100) / 10000,
    };

    this.isSaving.set(true);
    this.salesApi
      .updateSalesPersonSalesConfig(this.personId, model)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.notificationService.success('Sales configuration updated successfully.');
          void this.router.navigate(['/sales/persons', this.personId]);
        },
        error: () => {
          this.isSaving.set(false);
          this.notificationService.error('Failed to update sales configuration. Please try again.');
        },
      });
  }

  /** Navigates back to the sales person detail view, restoring the list query params captured at load time. */
  protected onCancel(): void {
    void this.router.navigate(['/sales/persons', this.personId], { queryParams: this.backQueryParams() });
  }
}
