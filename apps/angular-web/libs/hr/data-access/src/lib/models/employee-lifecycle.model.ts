/** Request body for POST /v1/employees/:id/lifecycle/hire. */
export interface EmployeeHire {
  readonly employeeId: number;
  readonly hireDate: string;
  readonly departmentId: number;
  readonly shiftId: number;
  readonly initialPayRate: number;
  readonly payFrequency: number;
  readonly managerId?: number | null;
  readonly initialVacationHours?: number;
  readonly initialSickLeaveHours?: number;
  readonly notes?: string | null;
}

/** Request body for POST /v1/employees/:id/lifecycle/terminate. */
export interface EmployeeTerminate {
  readonly employeeId: number;
  readonly terminationDate: string;
  readonly reason: string;
  readonly terminationType: 'Voluntary' | 'Involuntary' | 'Retirement' | 'Layoff';
  readonly eligibleForRehire: boolean;
  readonly payoutPto?: boolean;
  readonly notes?: string | null;
}

/** Request body for POST /v1/employees/:id/lifecycle/rehire. */
export interface EmployeeRehire {
  readonly employeeId: number;
  readonly rehireDate: string;
  readonly departmentId: number;
  readonly shiftId: number;
  readonly payRate: number;
  readonly payFrequency: number;
  readonly managerId?: number | null;
  readonly restoreSeniority: boolean;
  readonly notes?: string | null;
}

/** Response from GET /v1/employees/:id/lifecycle/status. */
export interface EmployeeLifecycleStatus {
  readonly employeeId: number;
  readonly fullName: string;
  readonly employmentStatus: 'Active' | 'Terminated' | 'OnLeave';
  readonly hireDate: string;
  readonly terminationDate: string | null;
  readonly daysEmployed: number | null;
  readonly currentDepartment: string | null;
  readonly currentShift: string | null;
  readonly departmentStartDate: string | null;
  readonly currentPayRate: number | null;
  readonly payRateEffectiveDate: string | null;
  readonly vacationHoursBalance: number;
  readonly sickLeaveHoursBalance: number;
  readonly eligibleForRehire: boolean;
  readonly rehireCount: number | null;
}
