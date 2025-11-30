/** Revenue total for a single calendar month within an analytics trend series. */
export interface SalesOrderMonthlyTrend {
  readonly year: number;
  readonly month: number;
  readonly revenue: number;
}

/** Aggregated analytics for a filtered slice of sales orders. */
export interface SalesOrderAnalytics {
  readonly totalRevenue: number;
  readonly orderCount: number;
  /** Filtered revenue as a percentage (0–100) of the all-time unfiltered revenue. */
  readonly percentageOfTotal: number;
  readonly monthlyTrend: SalesOrderMonthlyTrend[];
}

/**
 * Filter-only subset of SalesOrderParams — no pagination fields.
 * `accountNumber` is intentionally absent; the analytics panel does not filter by account.
 */
export interface SalesOrderAnalyticsFilter {
  readonly orderDateFrom?: string;
  readonly orderDateTo?: string;
  readonly status?: number;
  readonly salesPersonId?: number;
  readonly territoryId?: number;
}
