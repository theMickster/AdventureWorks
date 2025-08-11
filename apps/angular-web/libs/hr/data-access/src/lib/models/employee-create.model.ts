/** Request body for POST /v1/employees. Creates an employee with contact and address info. */
export interface EmployeeCreate {
  readonly firstName: string;
  readonly lastName: string;
  readonly middleName?: string | null;
  readonly title?: string | null;
  readonly suffix?: string | null;
  readonly jobTitle: string;
  readonly maritalStatus: 'M' | 'S';
  readonly gender: 'M' | 'F';
  readonly salariedFlag: boolean;
  readonly organizationLevel?: number | null;
  readonly nationalIdNumber: string;
  readonly loginId: string;
  readonly birthDate: string;
  readonly phone: {
    readonly phoneNumber: string;
    readonly phoneNumberTypeId: number;
  };
  readonly emailAddress: string;
  readonly address: EmployeeAddressCreate;
  readonly addressTypeId: number;
}

/** Address sub-object for employee creation. */
export interface EmployeeAddressCreate {
  readonly addressLine1: string;
  readonly addressLine2?: string | null;
  readonly city: string;
  readonly stateProvince: {
    readonly id: number;
    readonly name: string;
    readonly code: string;
  };
  readonly postalCode: string;
}
