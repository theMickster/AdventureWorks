import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { SalesPerson } from '@adventureworks-web/sales/data-access';
import { InputFieldComponent, SelectFieldComponent, SkeletonComponent } from '@adventureworks-web/shared/ui';
import { extractListNavParams, NotificationService } from '@adventureworks-web/shared/util';
import { SALES_PERSON_PAGE_SIZE } from '../feature-stores.constants';

@Component({
  selector: 'aw-store-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputFieldComponent, SelectFieldComponent, SkeletonComponent],
  templateUrl: './store-edit.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StoreEditComponent implements OnInit {
  private readonly salesApi = inject(SalesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly storeId: number = (() => {
    const rawId = this.route.snapshot.paramMap.get('id');
    return Math.trunc(Number(rawId));
  })();
  protected readonly salesPersons = signal<SalesPerson[]>([]);
  // Flipped to true on first submit attempt; drives nameErrors to show even before typing
  protected readonly submitted = signal(false);

  protected readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    salesPersonId: [null as string | null],
  });

  // Bridge form status changes into the signal graph so computed() re-evaluates on validation state changes
  private readonly _formStatus = toSignal(this.form.statusChanges, { initialValue: this.form.status });

  protected readonly salesPersonOptions = computed(() =>
    this.salesPersons().map((sp) => ({
      value: sp.id,
      label: `${sp.firstName} ${sp.lastName}`,
    })),
  );

  protected readonly nameErrors = computed((): Record<string, string> | null => {
    this._formStatus(); // read to re-evaluate whenever form status changes (value, validator runs)
    const ctrl = this.form.controls.name;
    if (!ctrl.errors) {
      return null;
    }
    // Show errors once the user has attempted to submit or the field has been blurred
    if (!this.submitted() && !ctrl.touched) {
      return null;
    }
    const errors: Record<string, string> = {};
    if (ctrl.errors['required']) {
      errors['required'] = 'Name is required.';
    }
    if (ctrl.errors['maxlength']) {
      errors['maxlength'] = 'Name cannot exceed 50 characters.';
    }
    return Object.keys(errors).length ? errors : null;
  });

  protected readonly backQueryParams = computed(() =>
    // snapshot read is intentional: captures list nav state once at navigation time
    extractListNavParams(this.route.snapshot.queryParams),
  );

  ngOnInit(): void {
    if (!this.storeId || this.storeId <= 0) {
      void this.router.navigate(['/sales/stores']);
      return;
    }

    forkJoin({
      store: this.salesApi.getStore(this.storeId),
      salesPersons: this.salesApi.getSalesPersons({ pageNumber: 1, pageSize: SALES_PERSON_PAGE_SIZE }),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ store, salesPersons }) => {
          this.salesPersons.set(salesPersons.results ?? []);
          this.form.patchValue({
            name: store.name,
            salesPersonId: store.salesPerson ? String(store.salesPerson.id) : null,
          });
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.notificationService.error('Failed to load store. Please try again.');
        },
      });
  }

  protected onSubmit(): void {
    if (this.isSaving()) {
      return;
    }
    this.form.controls.name.setValue((this.form.value.name ?? '').trim());
    this.submitted.set(true);
    if (this.form.invalid) {
      return;
    }
    const rawSalesPersonId = this.form.value.salesPersonId;
    const model = {
      id: this.storeId,
      name: this.form.value.name ?? '',
      salesPersonId: rawSalesPersonId ? Number(rawSalesPersonId) : null,
    };
    this.isSaving.set(true);
    this.salesApi
      .updateStore(this.storeId, model)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (store) => {
          this.notificationService.success('Store updated successfully.');
          void this.router.navigate(['/sales/stores', store.id]);
        },
        error: () => {
          this.isSaving.set(false);
          this.notificationService.error('Failed to update store. Please try again.');
        },
      });
  }
}
