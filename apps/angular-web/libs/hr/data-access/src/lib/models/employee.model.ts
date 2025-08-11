/** HumanResources.Employee entity with personal and employment details. */
export interface Employee {
  readonly id: number;
  readonly firstName: string;
  readonly lastName: string;
  readonly middleName: string | null;
  readonly title: string | null;
  readonly suffix: string | null;
  readonly jobTitle: string;
  readonly maritalStatus: 'M' | 'S';
  readonly gender: 'M' | 'F';
  readonly salariedFlag: boolean;
  readonly organizationLevel: number | null;
  readonly nationalIdNumber: string;
  readonly loginId: string;
  readonly birthDate: string;
  readonly hireDate: string;
  readonly currentFlag: boolean;
  readonly vacationHours: number;
  readonly sickLeaveHours: number;
  readonly emailAddress: string | null;
  readonly modifiedDate: string;
}
