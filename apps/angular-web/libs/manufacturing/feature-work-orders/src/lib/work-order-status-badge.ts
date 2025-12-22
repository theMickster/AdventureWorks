/**
 * DaisyUI badge variant per work-order `statusKey`. Only the "completed late" state gets a badge —
 * an on-time work order renders a blank Status cell (see `WorkOrderListComponent.rows`).
 */
export const WORK_ORDER_STATUS_BADGE_MAP: Record<string, string> = {
  'completed late': 'badge-warning',
};

/**
 * DaisyUI badge variant for the work-order detail view's completed-late indicator. A separate,
 * higher-severity (danger) variant from the list's `WORK_ORDER_STATUS_BADGE_MAP` — the detail page
 * renders this badge alongside a sibling days-late text span, not the day count interpolated into
 * the badge itself (the badge status string is piped through `|translate`).
 */
export const WORK_ORDER_DETAIL_STATUS_BADGE_MAP: Record<string, string> = {
  'completed-late': 'badge-error',
};
