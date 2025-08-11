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
export type { StoreParams } from './lib/models/store-params.model';
export type { SalesPersonParams } from './lib/models/sales-person-params.model';
export type { StoreSearchBody } from './lib/models/store-search.model';
export type { SalesPersonSearchBody } from './lib/models/sales-person-search.model';

// Services
export { SalesApiService } from './lib/services/sales-api.service';

// Stores
export { StoreStore } from './lib/stores/store.store';
export { SalesPersonStore } from './lib/stores/sales-person.store';
