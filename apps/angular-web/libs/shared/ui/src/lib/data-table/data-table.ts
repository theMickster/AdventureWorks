import {
  ChangeDetectionStrategy,
  Component,
  computed,
  contentChildren,
  input,
  output,
  booleanAttribute,
} from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonComponent } from '../button/button';
import { EmptyStateComponent } from '../empty-state/empty-state';
import { ColumnDefDirective } from './column-def.directive';
import { ColumnConfig } from './column-config.model';

@Component({
  selector: 'aw-data-table',
  standalone: true,
  imports: [NgTemplateOutlet, TranslatePipe, ButtonComponent, EmptyStateComponent],
  templateUrl: './data-table.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/**
 * Presentational data table with sortable columns, server-side pagination, and directive-based cell templates.
 * Emits sortChange/pageChange — the consuming component or store handles data fetching.
 */
export class DataTableComponent {
  readonly fieldId = input('aw-data-table');
  readonly columns = input.required<ColumnConfig[]>();
  readonly data = input<Record<string, unknown>[]>([]);
  readonly loading = input(false, { transform: booleanAttribute });
  readonly emptyIcon = input('fa-solid fa-inbox');
  readonly emptyTitle = input('No data');
  readonly emptyDescription = input('');

  // Pagination inputs (match withPagination store feature)
  readonly pageNumber = input(1);
  readonly pageSize = input(10);
  readonly totalRecords = input(0);
  readonly totalPages = input(0);

  // Sort state
  readonly sortColumn = input('');
  readonly sortDirection = input<'asc' | 'desc'>('asc');

  // Outputs
  readonly sortChange = output<{ column: string; direction: 'asc' | 'desc' }>();
  readonly pageChange = output<number>();

  // Collect cell templates from content children
  private readonly columnDefs = contentChildren(ColumnDefDirective);

  protected readonly templateMap = computed(() => {
    const map = new Map<string, ColumnDefDirective>();
    for (const def of this.columnDefs()) {
      map.set(def.awColumnDef(), def);
    }
    return map;
  });

  protected onSort(columnKey: string): void {
    const currentColumn = this.sortColumn();
    const currentDirection = this.sortDirection();
    const direction = columnKey === currentColumn && currentDirection === 'asc' ? 'desc' : 'asc';
    this.sortChange.emit({ column: columnKey, direction });
  }

  protected getCellValue(row: Record<string, unknown>, key: string): unknown {
    return row[key];
  }
}
