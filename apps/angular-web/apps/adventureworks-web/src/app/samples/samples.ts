import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { LookupStore } from '@adventureworks-web/shared/data-access';
import { ConfirmService } from '@adventureworks-web/shared/util';
import {
  ButtonComponent,
  InputFieldComponent,
  SelectFieldComponent,
  TextareaFieldComponent,
  ToggleFieldComponent,
  CardComponent,
  DataTableComponent,
  ColumnDefDirective,
  EmptyStateComponent,
  StatusBadgeComponent,
  ModalComponent,
} from '@adventureworks-web/shared/ui';
import type { ColumnConfig } from '@adventureworks-web/shared/ui';

@Component({
  selector: 'aw-samples',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ButtonComponent,
    InputFieldComponent,
    SelectFieldComponent,
    TextareaFieldComponent,
    ToggleFieldComponent,
    CardComponent,
    DataTableComponent,
    ColumnDefDirective,
    EmptyStateComponent,
    StatusBadgeComponent,
    ModalComponent,
  ],
  templateUrl: './samples.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SamplesComponent {
  protected readonly lookupStore = inject(LookupStore);
  private readonly confirmService = inject(ConfirmService);

  // Form demo
  protected readonly demoForm = new FormGroup({
    storeName: new FormControl('', [Validators.required, Validators.minLength(3)]),
    territory: new FormControl(''),
    description: new FormControl(''),
    active: new FormControl(true),
  });
  protected readonly storeNameErrors = signal<Record<string, string> | null>(null);
  protected readonly territoryOptions = [
    { value: 'nw', label: 'Northwest' },
    { value: 'sw', label: 'Southwest' },
    { value: 'ce', label: 'Central' },
    { value: 'ne', label: 'Northeast' },
  ];

  // Data table demo
  protected readonly tableColumns: ColumnConfig[] = [
    { key: 'id', label: 'ID' },
    { key: 'name', label: 'Store Name', sortable: true },
    { key: 'territory', label: 'Territory', sortable: true },
    { key: 'status', label: 'Status' },
  ];
  protected readonly tableData = signal<Record<string, unknown>[]>([
    { id: 1, name: 'Northwest Bikes', territory: 'Northwest', status: 'active' },
    { id: 2, name: 'Pacific Gear Co.', territory: 'Southwest', status: 'pending' },
    { id: 3, name: 'Mountain Supply', territory: 'Central', status: 'active' },
    { id: 4, name: 'Coastal Cycling', territory: 'Northeast', status: 'inactive' },
    { id: 5, name: 'Summit Sports', territory: 'Northwest', status: 'active' },
  ]);
  protected readonly sortColumn = signal('');
  protected readonly sortDirection = signal<'asc' | 'desc'>('asc');

  // Modal demo
  protected readonly modalOpen = signal(false);

  // Confirm dialog result
  protected readonly lastConfirmResult = signal<string>('');

  constructor() {
    this.lookupStore.loadDepartments();
  }

  protected validateStoreName(): void {
    const control = this.demoForm.get('storeName');
    if (control?.invalid && control?.touched) {
      const errors: Record<string, string> = {};
      if (control.errors?.['required']) errors['required'] = 'Store name is required';
      if (control.errors?.['minlength']) errors['minlength'] = 'Minimum 3 characters';
      this.storeNameErrors.set(errors);
    } else {
      this.storeNameErrors.set(null);
    }
  }

  protected onSortChange(event: { column: string; direction: 'asc' | 'desc' }): void {
    this.sortColumn.set(event.column);
    this.sortDirection.set(event.direction);
  }

  protected async onConfirmDemo(): Promise<void> {
    const result = await this.confirmService.confirm({
      title: 'Delete Store',
      message: 'Are you sure you want to delete "Northwest Bikes"? This action cannot be undone.',
      confirmLabel: 'Delete',
      cancelLabel: 'Keep',
      variant: 'error',
    });
    this.lastConfirmResult.set(result ? 'Confirmed (deleted)' : 'Cancelled (kept)');
  }
}
