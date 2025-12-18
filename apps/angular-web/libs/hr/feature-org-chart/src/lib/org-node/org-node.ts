import { ChangeDetectionStrategy, Component, ElementRef, effect, forwardRef, inject, input, output } from '@angular/core';
import type { OrgChartTreeNode } from '../models/org-chart-tree-node.model';

// CSS grid-row/width expand transition (org-node.css) takes 200ms; scrolling before it settles
// would measure the pre-expansion (collapsed) box.
const EXPAND_TRANSITION_MS = 200;

/**
 * Dumb, store-free, purely presentational recursive renderer for one org tree node and its
 * descendants. Self-imports its own component class via `forwardRef` — direct self-reference
 * (`imports: [OrgNodeComponent]`) compiles fine under this lib's own isolated `ng-packagr-lite`
 * build/test but fails with TS2449 ("Class used before its declaration") once TypeScript
 * type-checks the file as part of a full app-level build (e.g. `adventureworks-web`). `forwardRef`
 * defers the reference to inside a closure, which both TypeScript and Angular's runtime resolve
 * safely after the class is fully declared — the standard fix for a standalone component
 * importing itself. First use of a self-referencing recursive standalone component in this
 * workspace.
 */
@Component({
  selector: 'aw-org-node',
  imports: [forwardRef(() => OrgNodeComponent)],
  templateUrl: './org-node.html',
  styleUrl: './org-node.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrgNodeComponent {
  private readonly elementRef = inject(ElementRef<HTMLElement>);

  readonly node = input.required<OrgChartTreeNode>();
  readonly expandedIds = input.required<ReadonlySet<number>>();
  readonly highlightedId = input<number | null>(null);

  // Named toggleExpand, not toggle — @angular-eslint/no-output-native forbids reusing a native
  // DOM event name (<details> fires a real "toggle" event).
  readonly toggleExpand = output<number>();
  readonly navigate = output<number>();

  // Expanding a node re-centers its (now wider) children row under this node's own fixed
  // position, which can spill new content to the left of the current scroll position with
  // nothing bringing it back into view. null (not false) on the first run distinguishes "just
  // mounted already-expanded" (root, VPs at load) from a real collapsed-to-expanded transition —
  // only the latter should scroll.
  private previousExpanded: boolean | null = null;

  constructor() {
    effect(() => {
      const expanded = this.isExpanded();
      if (this.previousExpanded !== null && expanded && !this.previousExpanded) {
        setTimeout(() => {
          this.elementRef.nativeElement.scrollIntoView({ behavior: 'smooth', inline: 'nearest', block: 'nearest' });
        }, EXPAND_TRANSITION_MS);
      }

      this.previousExpanded = expanded;
    });
  }

  protected isExpanded(): boolean {
    return this.expandedIds().has(this.node().employeeId);
  }

  protected isHighlighted(): boolean {
    return this.highlightedId() === this.node().employeeId;
  }

  protected onToggleClick(event: Event): void {
    event.stopPropagation();
    this.toggleExpand.emit(this.node().employeeId);
  }

  protected onCardClick(): void {
    this.navigate.emit(this.node().employeeId);
  }

  protected onChildToggle(employeeId: number): void {
    this.toggleExpand.emit(employeeId);
  }

  protected onChildNavigate(employeeId: number): void {
    this.navigate.emit(employeeId);
  }
}
