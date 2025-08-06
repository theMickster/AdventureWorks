import { describe, it, expect, beforeEach } from 'vitest';
import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { DataTableComponent } from './data-table';
import { ColumnDefDirective } from './column-def.directive';
import { ColumnConfig } from './column-config.model';

@Component({
  standalone: true,
  imports: [DataTableComponent, ColumnDefDirective],
  template: `
    <aw-data-table
      fieldId="test-table"
      [columns]="columns"
      [data]="data()"
      [loading]="loading()"
      [pageNumber]="pageNumber()"
      [totalPages]="totalPages()"
      [totalRecords]="totalRecords()"
      [sortColumn]="sortColumn()"
      [sortDirection]="sortDirection()"
    >
      <ng-template awColumnDef="name" let-row>
        <strong class="custom-name">{{ row['name'] }}</strong>
      </ng-template>
    </aw-data-table>
  `,
})
class TestHostComponent {
  columns: ColumnConfig[] = [
    { key: 'id', label: 'ID' },
    { key: 'name', label: 'Name', sortable: true },
  ];
  data = signal<Record<string, unknown>[]>([
    { id: 1, name: 'Store A' },
    { id: 2, name: 'Store B' },
  ]);
  loading = signal(false);
  pageNumber = signal(1);
  totalPages = signal(1);
  totalRecords = signal(2);
  sortColumn = signal('');
  sortDirection = signal<'asc' | 'desc'>('asc');
}

describe('DataTableComponent', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;
  let el: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
      providers: [provideTranslateService()],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
    el = fixture.nativeElement as HTMLElement;
  });

  it('should render correct number of columns', () => {
    const headers = el.querySelectorAll('th');
    expect(headers.length).toBe(2);
  });

  it('should render correct number of rows', () => {
    const rows = el.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
  });

  it('should render a button in sortable column header', () => {
    const nameHeader = el.querySelector('#test-table-th-name') as HTMLElement;
    const button = nameHeader.querySelector('button');
    expect(button).toBeTruthy();
  });

  it('should not render a button in non-sortable column header', () => {
    const idHeader = el.querySelector('#test-table-th-id') as HTMLElement;
    const button = idHeader.querySelector('button');
    expect(button).toBeNull();
  });

  it('should use custom template for column with awColumnDef', () => {
    const customCell = el.querySelector('.custom-name') as HTMLElement;
    expect(customCell).toBeTruthy();
    expect(customCell.textContent).toContain('Store A');
  });

  it('should fall back to text content for columns without custom template', () => {
    const firstRow = el.querySelector('tbody tr') as HTMLElement;
    const firstCell = firstRow.querySelector('td') as HTMLElement;
    expect(firstCell.textContent).toContain('1');
  });

  it('should show empty state when data is empty', () => {
    host.data.set([]);
    fixture.detectChanges();
    const emptyState = el.querySelector('#test-table-empty');
    expect(emptyState).toBeTruthy();
  });

  it('should show skeleton when loading', () => {
    host.loading.set(true);
    fixture.detectChanges();
    const loadingEl = el.querySelector('#test-table-loading') as HTMLElement;
    expect(loadingEl).toBeTruthy();
    const skeletons = loadingEl.querySelectorAll('.skeleton');
    expect(skeletons.length).toBe(5);
  });

  it('should show pagination controls when totalPages > 1', () => {
    host.totalPages.set(3);
    host.totalRecords.set(25);
    fixture.detectChanges();
    const pagination = el.querySelector('#test-table-pagination') as HTMLElement;
    expect(pagination).toBeTruthy();
    expect(pagination.textContent).toContain('25 records');
  });

  it('should disable previous button on page 1', () => {
    host.totalPages.set(3);
    host.pageNumber.set(1);
    fixture.detectChanges();
    const prevButton = el.querySelector('#test-table-prev button') as HTMLButtonElement;
    expect(prevButton.disabled).toBe(true);
  });

  it('should disable next button on last page', () => {
    host.totalPages.set(3);
    host.pageNumber.set(3);
    fixture.detectChanges();
    const nextButton = el.querySelector('#test-table-next button') as HTMLButtonElement;
    expect(nextButton.disabled).toBe(true);
  });

  it('should show sort icon for active sort column', () => {
    host.sortColumn.set('name');
    host.sortDirection.set('asc');
    fixture.detectChanges();
    const nameHeader = el.querySelector('#test-table-th-name') as HTMLElement;
    const sortIcon = nameHeader.querySelector('.fa-sort-up');
    expect(sortIcon).toBeTruthy();
  });

  it('should show desc sort icon when direction is desc', () => {
    host.sortColumn.set('name');
    host.sortDirection.set('desc');
    fixture.detectChanges();
    const nameHeader = el.querySelector('#test-table-th-name') as HTMLElement;
    const sortIcon = nameHeader.querySelector('.fa-sort-down');
    expect(sortIcon).toBeTruthy();
  });

  it('should not show pagination when totalPages is 1', () => {
    host.totalPages.set(1);
    fixture.detectChanges();
    const pagination = el.querySelector('#test-table-pagination');
    expect(pagination).toBeNull();
  });
});
