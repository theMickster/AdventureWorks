/** Sales.SalesOrderHeader list-row projection returned by GET /v1/sales-orders. */
export interface SalesOrder {
  readonly salesOrderId: number;
  readonly salesOrderNumber: string;
  readonly orderDate: string;
  readonly status: number;
  readonly statusDescription: string;
  readonly totalDue: number;
  readonly customerName: string;
  readonly salesPersonName: string | null;
}

/**
 * Sales order status codes and their human-readable labels, used to drive the status filter dropdown.
 * Labels match the server's authoritative StatusDescription strings from
 * SalesOrderResolverHelpers.GetStatusDescription (note "In process" — lowercase 'p').
 */
export const SALES_ORDER_STATUSES = [
  { value: 1, label: 'In process' },
  { value: 2, label: 'Approved' },
  { value: 3, label: 'Backordered' },
  { value: 4, label: 'Rejected' },
  { value: 5, label: 'Shipped' },
  { value: 6, label: 'Cancelled' },
] as const;
