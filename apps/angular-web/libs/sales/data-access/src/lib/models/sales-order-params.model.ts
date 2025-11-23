import type { PaginationParams } from '@adventureworks-web/shared/data-access';

/** Query parameters for GET /v1/sales-orders. Extends shared pagination with sort and filter options. */
export interface SalesOrderParams extends PaginationParams {
  readonly orderBy?: 'salesOrderId' | 'orderDate' | 'totalDue' | 'salesOrderNumber';
  readonly orderDateFrom?: string;
  readonly orderDateTo?: string;
  readonly status?: number;
  readonly salesPersonId?: number;
  readonly territoryId?: number;
}
