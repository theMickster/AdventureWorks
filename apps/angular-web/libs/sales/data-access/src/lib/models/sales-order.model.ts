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

/** Bill-to or ship-to address on a sales order detail response. */
export interface SalesOrderAddress {
  /** Street address, line 1. */
  readonly addressLine1: string;
  /** Street address, line 2; null when not provided. */
  readonly addressLine2: string | null;
  /** City name. */
  readonly city: string;
  /** State or province name. */
  readonly stateProvince: string;
  /** Postal / ZIP code. */
  readonly postalCode: string;
}

/** A single line item on a sales order detail response. */
export interface SalesOrderLineItem {
  /** Primary key of the Sales.SalesOrderDetail row. */
  readonly salesOrderDetailId: number;
  /** Product name as of the order date. */
  readonly productName: string;
  /** Quantity ordered. */
  readonly orderQty: number;
  /** Unit list price at time of order. */
  readonly unitPrice: number;
  /** Discount fraction applied (0.0–1.0). */
  readonly unitPriceDiscount: number;
  /** Extended line total: unitPrice × (1 − unitPriceDiscount) × orderQty. */
  readonly lineTotal: number;
}

/** Full detail for a single sales order, including addresses and line items. */
export interface SalesOrderDetail {
  /** Primary key. */
  readonly salesOrderId: number;
  /** Human-readable order number (e.g. "SO43659"). */
  readonly salesOrderNumber: string;
  /** ISO 8601 date the order was placed. */
  readonly orderDate: string;
  /** ISO 8601 date the order is due. */
  readonly dueDate: string;
  /** ISO 8601 ship date; null if not yet shipped. */
  readonly shipDate: string | null;
  /** Numeric status code (1–6). */
  readonly status: number;
  /** Human-readable status label matching SALES_ORDER_STATUSES. */
  readonly statusDescription: string;
  /** Pre-tax, pre-freight subtotal. */
  readonly subTotal: number;
  /** Tax amount. */
  readonly taxAmt: number;
  /** Freight charge. */
  readonly freight: number;
  /** subTotal + taxAmt + freight. */
  readonly totalDue: number;
  /** Display name of the customer. */
  readonly customerName: string;
  /** BusinessEntityID of the assigned sales person; null for online orders. */
  readonly salesPersonId: number | null;
  /** Display name of the sales person; null for online orders. */
  readonly salesPersonName: string | null;
  /** Billing address. */
  readonly billToAddress: SalesOrderAddress;
  /** Shipping address. */
  readonly shipToAddress: SalesOrderAddress;
  /** Ordered line items. */
  readonly lineItems: SalesOrderLineItem[];
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
