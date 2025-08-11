import type { PaginationParams } from '@adventureworks-web/shared/data-access';

/** Query parameters for GET /v1/salespersons. Extends shared pagination with sales-person-specific sort options. */
export interface SalesPersonParams extends PaginationParams {
  readonly orderBy?: 'id' | 'firstName' | 'lastName';
}
