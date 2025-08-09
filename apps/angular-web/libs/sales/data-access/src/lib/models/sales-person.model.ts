/** Sales.SalesPerson entity with employee details and commission info. */
export interface SalesPerson {
  readonly id: number;
  readonly title: string | null;
  readonly firstName: string;
  readonly middleName: string | null;
  readonly lastName: string;
  readonly suffix: string | null;
  readonly jobTitle: string;
  readonly emailAddress: string;
  readonly territoryId: number | null;
  readonly salesQuota: number | null;
  readonly bonus: number;
  readonly commissionPct: number;
  readonly modifiedDate: string;
}

/** Request body for POST /v1/salespersons. Creates employee + sales person in one call. */
export interface SalesPersonCreate {
  readonly firstName: string;
  readonly lastName: string;
  readonly middleName?: string | null;
  readonly title?: string | null;
  readonly suffix?: string | null;
  readonly nationalIdNumber: string;
  readonly loginId: string;
  readonly jobTitle: string;
  readonly birthDate: string;
  readonly hireDate: string;
  readonly maritalStatus: 'M' | 'S';
  readonly gender: 'M' | 'F';
  readonly salariedFlag: boolean;
  readonly organizationLevel?: number | null;
  readonly territoryId?: number | null;
  readonly salesQuota?: number | null;
  readonly bonus: number;
  readonly commissionPct: number;
  readonly phone: {
    readonly phoneNumber: string;
    readonly phoneNumberTypeId: number;
  };
  readonly emailAddress: string;
  readonly address: SalesPersonAddressCreate;
  readonly addressTypeId: number;
}

export interface SalesPersonAddressCreate {
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

/** Request body for PUT /v1/salespersons/:id. */
export interface SalesPersonUpdate {
  readonly id: number;
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
  readonly territoryId?: number | null;
  readonly salesQuota?: number | null;
  readonly bonus: number;
  readonly commissionPct: number;
}
