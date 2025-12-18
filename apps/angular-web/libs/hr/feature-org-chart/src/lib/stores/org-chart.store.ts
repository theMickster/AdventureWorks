import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { catchError, EMPTY, forkJoin, pipe, switchMap, tap } from 'rxjs';
import { setError, setLoaded, setLoading, withRequestStatus } from '@adventureworks-web/shared/data-access';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { EmployeeOrgTreeItem } from '@adventureworks-web/hr/data-access';
import type { OrgChartTreeNode } from '../models/org-chart-tree-node.model';
import { buildOrgTree } from '../utils/build-org-tree';
import { computeOrgStats } from '../utils/compute-org-stats';
import type { OrgChartStats } from '../utils/compute-org-stats';
import { resolveGroupColorClass } from '../utils/department-group-color-map';

interface OrgChartState {
  readonly items: EmployeeOrgTreeItem[];
  readonly departmentColorClasses: Readonly<Record<string, string>>;
  readonly expandedIds: ReadonlySet<number>;
  readonly highlightedId: number | null;
}

/** One-shot ~290-row org chart payload — withState, not withEntities (mirrors DashboardStore, not EmployeeStore). */
export const OrgChartStore = signalStore(
  { providedIn: 'root' },
  withDevtools('org-chart'),
  withState<OrgChartState>({
    items: [],
    departmentColorClasses: {},
    expandedIds: new Set<number>(),
    highlightedId: null,
  }),
  withRequestStatus(),
  withComputed((store) => ({
    // Recomputes only when items()/departmentColorClasses() change (on load), never per render.
    tree: computed<OrgChartTreeNode | null>(() => {
      const items = store.items();
      if (items.length === 0) {
        return null;
      }

      const colorClasses = store.departmentColorClasses();
      return buildOrgTree(items, (departmentName) => colorClasses[departmentName] ?? resolveGroupColorClass(undefined));
    }),
    stats: computed<OrgChartStats>(() => computeOrgStats(store.items())),
  })),
  withMethods((store, hrApi = inject(HrApiService)) => ({
    load: rxMethod<void>(
      pipe(
        tap(() => patchState(store, setLoading())),
        switchMap(() =>
          forkJoin({ items: hrApi.getOrgTree(), departments: hrApi.getDepartments() }).pipe(
            tap(({ items, departments }) => {
              const departmentColorClasses: Record<string, string> = {};
              for (const department of departments) {
                departmentColorClasses[department.name] = resolveGroupColorClass(department.groupName);
              }

              const root = buildOrgTree(items, (departmentName) => departmentColorClasses[departmentName] ?? resolveGroupColorClass(undefined));

              const expandedIds = new Set<number>();
              if (root) {
                expandedIds.add(root.employeeId);
              }
              for (const item of items) {
                if (item.organizationLevel === 1) {
                  expandedIds.add(item.employeeId);
                }
              }

              patchState(store, { items, departmentColorClasses, expandedIds, highlightedId: null }, setLoaded());
            }),
            catchError(() => {
              patchState(store, setError('Failed to load organization chart'));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),
    toggleExpanded(employeeId: number): void {
      const next = new Set(store.expandedIds());
      if (next.has(employeeId)) {
        next.delete(employeeId);
      } else {
        next.add(employeeId);
      }

      patchState(store, { expandedIds: next });
    },
    searchAndExpand(term: string): void {
      const trimmed = term.trim().toLowerCase();
      if (trimmed.length === 0) {
        patchState(store, { highlightedId: null });
        return;
      }

      const items = store.items();
      const match = items.find((item) => item.fullName.toLowerCase().includes(trimmed));
      if (!match) {
        return;
      }

      const rootId = store.tree()?.employeeId ?? null;
      const byId = new Map(items.map((item) => [item.employeeId, item] as const));
      const next = new Set(store.expandedIds());

      // Walk the ancestor chain via parentEmployeeId, defaulting a null link to the tree's resolved
      // root — the same fallback buildOrgTree uses to reparent orphans (see build-org-tree.ts) — so
      // the expansion path matches what actually rendered, not the raw (occasionally broken) link.
      let parentId: number | null = match.parentEmployeeId ?? rootId;
      while (parentId !== null) {
        next.add(parentId);
        if (parentId === rootId) {
          break;
        }

        const parentItem = byId.get(parentId);
        parentId = parentItem ? parentItem.parentEmployeeId ?? rootId : null;
      }

      patchState(store, { expandedIds: next, highlightedId: match.employeeId });
    },
  })),
);
