/** Request body for PUT /v1/employees/:id. Updates basic employee info only. */
export interface EmployeeUpdate {
  readonly id: number;
  readonly firstName: string;
  readonly lastName: string;
  readonly middleName?: string | null;
  readonly title?: string | null;
  readonly suffix?: string | null;
  readonly maritalStatus: 'M' | 'S';
  readonly gender: 'M' | 'F';
}
