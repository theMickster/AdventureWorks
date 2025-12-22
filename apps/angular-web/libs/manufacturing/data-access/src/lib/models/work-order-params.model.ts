import type { PaginationParams } from '@adventureworks-web/shared/data-access';

/**
 * Query parameters for GET /v1/work-orders. Extends shared pagination with sort and filter options.
 *
 * `startDate`/`endDate` both filter against `WorkOrder.StartDate` (range, inclusive) — `endDate`
 * does NOT filter `WorkOrder.EndDate`. This matches the API's `WorkOrderParameter` contract exactly.
 */
export interface WorkOrderParams extends PaginationParams {
  readonly orderBy?: 'workOrderId' | 'startDate' | 'dueDate';
  readonly productId?: number;
  readonly startDate?: string;
  readonly endDate?: string;
  readonly hasScrapped?: boolean;
  readonly scrapReasonId?: number;
}
