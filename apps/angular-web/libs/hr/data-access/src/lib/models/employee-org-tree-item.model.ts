/** Flat org-hierarchy row from GET /v1/employees/org-tree. The root (CEO) has organizationLevel and parentEmployeeId both null. */
export interface EmployeeOrgTreeItem {
  readonly employeeId: number;
  readonly fullName: string;
  readonly jobTitle: string;
  readonly departmentName: string;
  readonly organizationLevel: number | null;
  readonly parentEmployeeId: number | null;
}
