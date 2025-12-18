import type { EmployeeOrgTreeItem } from '@adventureworks-web/hr/data-access';

export interface OrgChartStats {
  readonly totalEmployees: number;
  readonly managerCount: number;
  readonly averageSpanOfControl: number;
  readonly maxDepth: number;
}

export function computeOrgStats(items: readonly EmployeeOrgTreeItem[]): OrgChartStats {
  if (items.length === 0) {
    return { totalEmployees: 0, managerCount: 0, averageSpanOfControl: 0, maxDepth: 0 };
  }

  // Mirrors build-org-tree.ts's root resolution and orphan-reparenting exactly. Without this, a
  // null parentEmployeeId (VPs hit by the OrganizationNode hierarchyid quirk — see
  // build-org-tree.ts — or an employee not yet assigned a manager) would silently drop out of
  // "has a manager" here while still rendering as a child of the root in the tree, undercounting
  // managerCount by at least 1 (the root itself never shows up as anyone's raw parentEmployeeId).
  const rootId = items.find((item) => item.organizationLevel === null)?.employeeId ?? null;

  const managerIds = new Set<number>();
  let employeesWithManager = 0;
  let maxDepth = 0;

  for (const item of items) {
    if (item.employeeId !== rootId) {
      const effectiveParentId = item.parentEmployeeId ?? rootId;
      if (effectiveParentId !== null) {
        managerIds.add(effectiveParentId);
        employeesWithManager += 1;
      }
    }

    if (item.organizationLevel !== null && item.organizationLevel > maxDepth) {
      maxDepth = item.organizationLevel;
    }
  }

  const managerCount = managerIds.size;
  const averageSpanOfControl = managerCount === 0 ? 0 : employeesWithManager / managerCount;

  return {
    totalEmployees: items.length,
    managerCount,
    averageSpanOfControl,
    maxDepth,
  };
}
