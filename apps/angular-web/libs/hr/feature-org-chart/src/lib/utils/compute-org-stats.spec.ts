import type { EmployeeOrgTreeItem } from '@adventureworks-web/hr/data-access';
import { computeOrgStats } from './compute-org-stats';

function item(overrides: Partial<EmployeeOrgTreeItem> & { employeeId: number }): EmployeeOrgTreeItem {
  return {
    fullName: `Employee ${overrides.employeeId}`,
    jobTitle: 'Some Title',
    departmentName: 'Sales',
    organizationLevel: null,
    parentEmployeeId: null,
    ...overrides,
  };
}

describe('computeOrgStats', () => {
  it('returns zeroed stats for an empty list', () => {
    expect(computeOrgStats([])).toEqual({
      totalEmployees: 0,
      managerCount: 0,
      averageSpanOfControl: 0,
      maxDepth: 0,
    });
  });

  it('computes manager count and span of control for the Brian Welcker case (3 direct reports)', () => {
    const items: EmployeeOrgTreeItem[] = [
      item({ employeeId: 1, organizationLevel: null, parentEmployeeId: null }),
      item({ employeeId: 273, organizationLevel: 1, parentEmployeeId: 1 }),
      item({ employeeId: 274, organizationLevel: 2, parentEmployeeId: 273 }),
      item({ employeeId: 285, organizationLevel: 2, parentEmployeeId: 273 }),
      item({ employeeId: 287, organizationLevel: 2, parentEmployeeId: 273 }),
    ];

    const stats = computeOrgStats(items);

    expect(stats.totalEmployees).toBe(5);
    expect(stats.managerCount).toBe(2); // employee 1 and employee 273 each have at least one direct report
    expect(stats.averageSpanOfControl).toBe(4 / 2); // 4 employees have a manager, across 2 distinct managers
    expect(stats.maxDepth).toBe(2);
  });

  it('guards against divide-by-zero when no employee has a manager', () => {
    const items: EmployeeOrgTreeItem[] = [item({ employeeId: 1, organizationLevel: null, parentEmployeeId: null })];

    const stats = computeOrgStats(items);

    expect(stats.managerCount).toBe(0);
    expect(stats.averageSpanOfControl).toBe(0);
  });

  it('counts the root as a manager of a VP whose parentEmployeeId is null (real data quirk)', () => {
    // Mirrors the real AdventureWorks data: the CEO's OrganizationNode is SQL NULL, so no row's
    // raw parentEmployeeId is ever the CEO's id — Brian Welcker (a VP, organizationLevel 1) comes
    // back with parentEmployeeId === null even though buildOrgTree renders him as the CEO's
    // child. Stats must agree with that rendered tree, not the raw (broken) link.
    const items: EmployeeOrgTreeItem[] = [
      item({ employeeId: 1, organizationLevel: null, parentEmployeeId: null }),
      item({ employeeId: 273, organizationLevel: 1, parentEmployeeId: null }),
      item({ employeeId: 274, organizationLevel: 2, parentEmployeeId: 273 }),
    ];

    const stats = computeOrgStats(items);

    expect(stats.managerCount).toBe(2); // employee 1 (via the reparented VP) and employee 273
    expect(stats.averageSpanOfControl).toBe(1); // 2 employees have a manager, across 2 distinct managers
  });
});
