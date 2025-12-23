/** A single department assignment record in an employee's history. Dates arrive as ISO strings over the wire. */
export interface EmployeeDepartmentHistory {
  readonly departmentId: number;
  readonly departmentName: string;
  readonly shiftId: number;
  readonly shiftName: string;
  readonly startDate: string;
  readonly endDate: string | null;
}
