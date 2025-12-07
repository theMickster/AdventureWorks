// Models
export type {
  Store,
  StoreCreate,
  StoreUpdate,
  StoreAddress,
  StoreContact,
  StoreSalesPerson,
} from './lib/models/store.model';
export type {
  SalesPerson,
  SalesPersonCreate,
  SalesPersonSalesConfigUpdate,
  SalesPersonUpdate,
  SalesPersonAddressCreate,
  SalesPersonPerformance,
  SalesPersonQuotaHistory,
  SalesPersonTerritoryHistory,
} from './lib/models/sales-person.model';
export type { SalesOrder, SalesOrderDetail, SalesOrderAddress, SalesOrderLineItem } from './lib/models/sales-order.model';
export type { SalesOrderAnalytics, SalesOrderMonthlyTrend, SalesOrderAnalyticsFilter } from './lib/models/sales-order-analytics.model';
export { SALES_ORDER_STATUSES } from './lib/models/sales-order.model';
export type { SalesDashboard, DashboardTopPerformer, DashboardTerritory, DashboardMonthlySalesTrend } from './lib/models/sales-dashboard.model';
export type { StoreParams } from './lib/models/store-params.model';
export type { SalesPersonParams } from './lib/models/sales-person-params.model';
export type { SalesOrderParams } from './lib/models/sales-order-params.model';
export type { StoreSearchBody } from './lib/models/store-search.model';
export type { SalesPersonSearchBody } from './lib/models/sales-person-search.model';
export type { CustomerListItem } from './lib/models/customer-list-item.model';
export type { CustomerParams } from './lib/models/customer-params.model';

// Services
export { SalesApiService } from './lib/services/sales-api.service';

// Stores
export { StoreStore } from './lib/stores/store.store';
export { SalesPersonStore } from './lib/stores/sales-person.store';
export { SalesOrderStore } from './lib/stores/sales-order.store';
export { DashboardStore } from './lib/stores/dashboard.store';
export { CustomerStore } from './lib/stores/customer.store';
