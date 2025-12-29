/**
 * DaisyUI badge variant per lowercased purchase order `statusLabel` — 1=Pending, 2=Approved,
 * 3=Rejected, 4=Complete. Shared by `VendorDetailComponent`'s PO-history table and
 * `PurchaseOrderDetailComponent`'s header badge; do not duplicate inline in either.
 */
export const PURCHASE_ORDER_STATUS_BADGE_MAP: Record<string, string> = {
  pending: 'badge-warning',
  approved: 'badge-info',
  rejected: 'badge-error',
  complete: 'badge-success',
};
