/** One department's active headcount, from `EmployeeAggregates.departmentHeadcounts`. Zero-headcount departments are included. */
export interface DepartmentHeadcountSummary {
  readonly departmentId: number;
  readonly departmentName: string;
  readonly groupName: string;
  readonly activeEmployeeCount: number;
}

/** Active-employee tenure bucket counts. Always sums to `EmployeeAggregates.activeEmployeeCount`. */
export interface TenureDistribution {
  readonly underOneYear: number;
  readonly oneToThreeYears: number;
  readonly threeToFiveYears: number;
  readonly fiveToTenYears: number;
  readonly tenPlusYears: number;
}

/** Pay rate summary for one department group (e.g. "Research and Development"). */
export interface PayBandSummary {
  readonly departmentGroup: string;
  readonly averageRate: number;
  readonly minRate: number;
  readonly maxRate: number;
}

/** Response body for GET /v1/employees/aggregates — HR dashboard stat cards and chart data. */
export interface EmployeeAggregates {
  readonly totalEmployeeCount: number;
  readonly activeEmployeeCount: number;
  readonly terminatedEmployeeCount: number;
  readonly departmentCount: number;
  readonly departmentHeadcounts: DepartmentHeadcountSummary[];
  readonly tenureDistribution: TenureDistribution;
  readonly payBandSummary: PayBandSummary[];
}
