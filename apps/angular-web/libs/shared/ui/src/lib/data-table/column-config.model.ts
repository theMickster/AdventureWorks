/** Column definition for DataTableComponent. Controls header label, sort behavior, and CSS classes. */
export interface ColumnConfig {
  readonly key: string;
  readonly label: string;
  readonly sortable?: boolean;
  readonly headerClass?: string;
  readonly cellClass?: string;
}
