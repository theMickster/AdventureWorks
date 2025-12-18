/** A single node in the nested org hierarchy, built once from the flat `EmployeeOrgTreeItem[]` payload. */
export interface OrgChartTreeNode {
  readonly employeeId: number;
  readonly fullName: string;
  readonly jobTitle: string;
  readonly departmentName: string;
  readonly organizationLevel: number | null;
  readonly colorClass: string;
  readonly children: OrgChartTreeNode[];
}
