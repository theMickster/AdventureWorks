/** A single pay rate change record in an employee's history. `rateChangeDate` arrives as an ISO string over the wire. */
export interface EmployeePayHistory {
  readonly rateChangeDate: string;
  readonly rate: number;
  readonly payFrequency: 1 | 2;
  readonly payFrequencyLabel: string;
}
