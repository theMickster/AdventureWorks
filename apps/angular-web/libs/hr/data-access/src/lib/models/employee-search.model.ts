/** Search criteria for POST /v1/employees/search. */
export interface EmployeeSearchBody {
  readonly id?: number | null;
  readonly firstName?: string | null;
  readonly lastName?: string | null;
  readonly jobTitle?: string | null;
  readonly emailAddress?: string | null;
  readonly nationalIdNumber?: string | null;
  readonly loginId?: string | null;
}
