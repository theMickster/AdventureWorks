import type { PaginationParams } from '@adventureworks-web/shared/data-access';

/** Query parameters for GET /v1/stores. Extends shared pagination with store-specific sort options. */
export interface StoreParams extends PaginationParams {
  readonly orderBy?: 'id' | 'name';
}
