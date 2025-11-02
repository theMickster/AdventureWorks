export interface StoreAddress {
  readonly id: number;
  readonly storeId: number;
  readonly addressTypeId: number;
  readonly addressTypeName: string;
  readonly addressLine1: string;
  readonly addressLine2: string | null;
  readonly city: string;
  readonly stateProvinceCode: string;
  readonly stateProvinceName: string;
  readonly countryRegionCode: string;
  readonly countryRegionName: string;
  readonly postalCode: string;
  readonly modifiedDate: string;
}

export interface StoreContact {
  readonly id: number;
  readonly title: string | null;
  readonly firstName: string;
  readonly middleName: string | null;
  readonly lastName: string;
  readonly suffix: string | null;
  readonly contactTypeId: number;
  readonly contactTypeName: string;
  readonly storeId: number;
}

export interface StoreSalesPerson {
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

/** Sales.Store entity with addresses, contacts, and assigned sales person. */
export interface Store {
  readonly id: number;
  readonly name: string;
  readonly modifiedDate: string;
  readonly storeAddresses: StoreAddress[];
  readonly storeContacts: StoreContact[];
  readonly salesPerson: StoreSalesPerson | null;
}

/** Request body for POST /v1/stores. */
export interface StoreCreate {
  readonly name: string;
  readonly salesPersonId?: number | null;
}

/** Request body for PUT /v1/stores/:id. */
export interface StoreUpdate {
  readonly id: number;
  readonly name: string;
  readonly salesPersonId?: number | null;
}
