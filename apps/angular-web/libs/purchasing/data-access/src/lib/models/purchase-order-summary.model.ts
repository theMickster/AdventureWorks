/** A single row in a vendor's purchase order history (`GET /v1/vendors/:id/purchase-orders`). */
export interface PurchaseOrderSummary {
  readonly purchaseOrderId: number;
  readonly orderDate: string;
  /**
   * Earliest due date across the PO's line items, server-computed as `MIN(DueDate)` — the API's
   * `PurchaseOrderHeader` has no due-date column of its own. Null only when the PO has no line
   * items (a data-integrity edge case, not expected in practice).
   */
  readonly dueDate: string | null;
  /** Raw status code: 1=Pending, 2=Approved, 3=Rejected, 4=Complete. */
  readonly status: number;
  /** Human-readable label for `status` — already server-formatted. */
  readonly statusLabel: string;
  readonly totalDue: number;
}

/**
 * Query params for `GET /v1/vendors/:id/purchase-orders`. Server always sorts by order date
 * descending — no sort param, matching `VendorListParams`'s fixed-order precedent.
 */
export interface VendorPurchaseOrderParams {
  readonly pageNumber: number;
  readonly pageSize?: number;
  /** 1=Pending, 2=Approved, 3=Rejected, 4=Complete. */
  readonly status?: number;
  readonly startDate?: string;
  readonly endDate?: string;
}
