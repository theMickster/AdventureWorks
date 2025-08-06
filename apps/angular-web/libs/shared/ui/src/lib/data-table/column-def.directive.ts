import { Directive, input, TemplateRef, inject } from '@angular/core';

@Directive({
  selector: '[awColumnDef]',
  standalone: true,
})
/** Structural directive for custom cell templates in DataTableComponent. Usage: `<ng-template awColumnDef="key" let-row>`. */
export class ColumnDefDirective {
  readonly awColumnDef = input.required<string>();
  readonly template = inject(TemplateRef);
}
