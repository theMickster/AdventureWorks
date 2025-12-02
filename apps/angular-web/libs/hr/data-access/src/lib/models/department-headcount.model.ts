/** Response body for GET /v1/departments/:id/headcount. */
export interface DepartmentHeadcount {
  readonly departmentId: number;
  readonly departmentName: string;
  readonly activeEmployeeCount: number;
}
