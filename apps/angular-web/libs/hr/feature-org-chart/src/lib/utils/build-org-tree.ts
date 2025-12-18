import type { EmployeeOrgTreeItem } from '@adventureworks-web/hr/data-access';
import type { OrgChartTreeNode } from '../models/org-chart-tree-node.model';

/**
 * Builds the nested org tree from the flat `/employees/org-tree` payload in a single O(n) pass.
 *
 * The true root is the one row with `organizationLevel === null` (the CEO). In the live
 * AdventureWorks data, several *other* rows also carry a null `parentEmployeeId`: VPs whose
 * hierarchyid ancestor lookup misses because the CEO's `OrganizationNode` is SQL `NULL` rather
 * than the hierarchyid root value, plus any employee created through the app that hasn't been
 * assigned a manager yet. All of these orphans are attached directly under the true root so the
 * result is always a single connected tree instead of a forest or dropped nodes.
 */
export function buildOrgTree(
  items: readonly EmployeeOrgTreeItem[],
  resolveColorClass: (departmentName: string) => string,
): OrgChartTreeNode | null {
  if (items.length === 0) {
    return null;
  }

  const rootItem = items.find((item) => item.organizationLevel === null);
  if (!rootItem) {
    throw new Error('Org tree has no root employee (no row with organizationLevel === null).');
  }

  const rootId = rootItem.employeeId;

  const nodesById = new Map<number, OrgChartTreeNode>();
  for (const item of items) {
    nodesById.set(item.employeeId, {
      employeeId: item.employeeId,
      fullName: item.fullName,
      jobTitle: item.jobTitle,
      departmentName: item.departmentName,
      organizationLevel: item.organizationLevel,
      colorClass: resolveColorClass(item.departmentName),
      children: [],
    });
  }

  for (const item of items) {
    if (item.employeeId === rootId) {
      continue;
    }

    const parentId = item.parentEmployeeId ?? rootId;
    const parentNode = nodesById.get(parentId) ?? nodesById.get(rootId);
    parentNode?.children.push(nodesById.get(item.employeeId) as OrgChartTreeNode);
  }

  return nodesById.get(rootId) ?? null;
}
