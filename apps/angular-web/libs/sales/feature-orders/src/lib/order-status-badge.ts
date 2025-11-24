/**
 * DaisyUI badge variant per lowercased server statusDescription for sales orders.
 * Keys must match the lowercased values of SalesOrderResolverHelpers.GetStatusDescription
 * (note "in process" — lowercase 'p').
 */
export const STATUS_BADGE_MAP: Record<string, string> = {
  'in process': 'badge-info',
  approved: 'badge-success',
  backordered: 'badge-warning',
  rejected: 'badge-error',
  shipped: 'badge-success',
  cancelled: 'badge-error',
};
