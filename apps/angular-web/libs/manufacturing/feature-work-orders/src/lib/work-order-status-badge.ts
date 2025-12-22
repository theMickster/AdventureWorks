/**
 * DaisyUI badge variant per work-order `statusKey`. Only the "completed late" state gets a badge —
 * an on-time work order renders a blank Status cell (see `WorkOrderListComponent.rows`).
 */
export const WORK_ORDER_STATUS_BADGE_MAP: Record<string, string> = {
  'completed late': 'badge-warning',
};
