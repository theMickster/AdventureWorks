/** A single line item on a purchase order (part of `GET /v1/purchase-orders/:id`). */
export interface PurchaseOrderLineItem {
  readonly purchaseOrderDetailId: number;
  readonly productId: number;
  readonly productName: string;
  readonly dueDate: string;
  readonly orderQty: number;
  readonly unitPrice: number;
  readonly lineTotal: number;
  readonly receivedQty: number;
  readonly rejectedQty: number;
  readonly stockedQty: number;
}

/** Full detail for a single purchase order (`GET /v1/purchase-orders/:id`). */
export interface PurchaseOrderDetail {
  readonly purchaseOrderId: number;
  /** Raw status code: 1=Pending, 2=Approved, 3=Rejected, 4=Complete. */
  readonly status: number;
  /** Human-readable label for `status` — already server-formatted. */
  readonly statusLabel: string;
  readonly orderDate: string;
  /**
   * Earliest due date across the PO's line items, server-computed as `MIN(DueDate)` — falls back
   * to `orderDate` when the purchase order has zero line items.
   */
  readonly dueDate: string;
  readonly shipDate: string | null;
  readonly vendorId: number;
  readonly vendorName: string;
  readonly employeeId: number;
  readonly approvingEmployeeFullName: string;
  readonly shipMethodId: number;
  readonly shipMethodName: string;
  readonly subTotal: number;
  readonly taxAmt: number;
  readonly freight: number;
  readonly totalDue: number;
  readonly lineItems: readonly PurchaseOrderLineItem[];
}
