// Models
export type {
  Store,
  StoreCreate,
  StoreUpdate,
  StoreAddress,
  StoreAddressDetail,
  StoreContact,
  StoreSalesPerson,
} from './lib/models/store.model';
export type {
  SalesPerson,
  SalesPersonCreate,
  SalesPersonUpdate,
  SalesPersonAddressCreate,
} from './lib/models/sales-person.model';
export type { StoreParams, SalesPersonParams } from './lib/models/store-params.model';

// Services
export { SalesApiService } from './lib/services/sales-api.service';
