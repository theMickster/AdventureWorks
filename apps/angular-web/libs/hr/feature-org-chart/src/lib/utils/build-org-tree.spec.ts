import type { EmployeeOrgTreeItem } from '@adventureworks-web/hr/data-access';
import { buildOrgTree } from './build-org-tree';

const resolveColorClass = (departmentName: string): string => (departmentName === 'Sales' ? 'accent' : 'primary');

function item(overrides: Partial<EmployeeOrgTreeItem> & { employeeId: number }): EmployeeOrgTreeItem {
  return {
    fullName: `Employee ${overrides.employeeId}`,
    jobTitle: 'Some Title',
    departmentName: 'Executive',
    organizationLevel: null,
    parentEmployeeId: null,
    ...overrides,
  };
}

describe('buildOrgTree', () => {
  it('returns null for an empty list', () => {
    expect(buildOrgTree([], resolveColorClass)).toBeNull();
  });

  it('nests children correctly under their parent', () => {
    const items: EmployeeOrgTreeItem[] = [
      item({ employeeId: 1, fullName: 'Ken Sánchez', jobTitle: 'CEO', organizationLevel: null, parentEmployeeId: null }),
      item({
        employeeId: 273,
        fullName: 'Brian Welcker',
        jobTitle: 'VP Sales',
        departmentName: 'Sales',
        organizationLevel: 1,
        parentEmployeeId: 1,
      }),
      item({
        employeeId: 274,
        fullName: 'Stephen Jiang',
        jobTitle: 'NA Sales Manager',
        departmentName: 'Sales',
        organizationLevel: 2,
        parentEmployeeId: 273,
      }),
    ];

    const root = buildOrgTree(items, resolveColorClass);

    expect(root?.employeeId).toBe(1);
    expect(root?.children).toHaveLength(1);
    expect(root?.children[0].employeeId).toBe(273);
    expect(root?.children[0].colorClass).toBe('accent');
    expect(root?.children[0].children).toHaveLength(1);
    expect(root?.children[0].children[0].employeeId).toBe(274);
  });

  it('enforces a single root even when multiple rows have a null parentEmployeeId', () => {
    // Mirrors the real AdventureWorks data quirk: the CEO's OrganizationNode is SQL NULL, so
    // several VPs' hierarchyid ancestor lookup misses and they also carry parentEmployeeId === null.
    const items: EmployeeOrgTreeItem[] = [
      item({ employeeId: 1, fullName: 'Ken Sánchez', organizationLevel: null, parentEmployeeId: null }),
      item({ employeeId: 273, fullName: 'Brian Welcker', organizationLevel: 1, parentEmployeeId: null }),
      item({ employeeId: 2, fullName: 'Terri Duffy', organizationLevel: 1, parentEmployeeId: null }),
    ];

    const root = buildOrgTree(items, resolveColorClass);

    expect(root?.employeeId).toBe(1);
    expect(root?.children.map((c) => c.employeeId).sort()).toEqual([2, 273]);
  });

  it('attaches an employee not yet assigned a manager directly under the root', () => {
    const items: EmployeeOrgTreeItem[] = [
      item({ employeeId: 1, organizationLevel: null, parentEmployeeId: null }),
      item({ employeeId: 22788, fullName: 'New Hire', organizationLevel: null, parentEmployeeId: null }),
    ];

    const root = buildOrgTree(items, resolveColorClass);

    expect(root?.children.map((c) => c.employeeId)).toEqual([22788]);
  });

  it('throws when no row has organizationLevel === null', () => {
    const items: EmployeeOrgTreeItem[] = [item({ employeeId: 273, organizationLevel: 1, parentEmployeeId: null })];

    expect(() => buildOrgTree(items, resolveColorClass)).toThrow(/no root employee/i);
  });
});
