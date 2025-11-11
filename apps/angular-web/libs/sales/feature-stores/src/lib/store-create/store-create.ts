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
import { Router, RouterLink } from '@angular/router';
import { SalesApiService } from '@adventureworks-web/sales/data-access';
import type { SalesPerson } from '@adventureworks-web/sales/data-access';
import { InputFieldComponent, SelectFieldComponent } from '@adventureworks-web/shared/ui';
import { NotificationService } from '@adventureworks-web/shared/util';

const SALES_PERSON_PAGE_SIZE = 25;

@Component({
  selector: 'aw-store-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputFieldComponent, SelectFieldComponent],
  templateUrl: './store-create.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StoreCreateComponent implements OnInit {
  private readonly salesApi = inject(SalesApiService);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  protected readonly isLoading = signal(false);
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

  ngOnInit(): void {
    this.salesApi
      .getSalesPersons({ pageNumber: 1, pageSize: SALES_PERSON_PAGE_SIZE })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.salesPersons.set(result.results ?? []);
        },
        error: () => {
          this.notificationService.error('Failed to load sales persons.');
        },
      });
  }

  protected onSubmit(): void {
    if (this.isLoading()) {
      return;
    }
    this.form.controls.name.setValue((this.form.value.name ?? '').trim());
    this.submitted.set(true);
    if (this.form.invalid) {
      return;
    }
    const rawSalesPersonId = this.form.value.salesPersonId;
    const model = {
      name: this.form.value.name ?? '',
      salesPersonId: rawSalesPersonId ? Number(rawSalesPersonId) : null,
    };
    this.isLoading.set(true);
    this.salesApi
      .createStore(model)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (store) => {
          this.notificationService.success('Store created successfully.');
          void this.router.navigate(['/sales/stores', store.id]);
        },
        error: () => {
          this.isLoading.set(false);
          this.notificationService.error('Failed to create store. Please try again.');
        },
      });
  }
}
