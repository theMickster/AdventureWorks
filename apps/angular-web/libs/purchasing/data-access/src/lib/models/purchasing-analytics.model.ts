/** A single vendor's total spend and its running cumulative share of total spend, for the Pareto view. */
export interface VendorSpendDto {
  readonly vendorId: number;
  readonly vendorName: string;
  readonly totalSpend: number;
  /** Running share of total spend as a percentage; monotonically increasing, the last entry is 100. */
  readonly cumulativePercent: number;
}

/** Purchase order count and value rolled up by status, for the pipeline summary. */
export interface PipelineSummaryItemDto {
  /** Human-readable status: Pending, Approved, Rejected, or Complete. */
  readonly statusLabel: string;
  readonly poCount: number;
  readonly totalValue: number;
}

/** Aggregate purchasing analytics (`GET /v1/purchasing/analytics`) — Pareto vendor spend plus the PO pipeline summary. */
export interface PurchasingAnalyticsDto {
  /** Every vendor, ordered by total spend descending. */
  readonly paretoData: VendorSpendDto[];
  /** Always exactly four entries — Pending, Approved, Rejected, Complete — in that order. */
  readonly pipelineSummary: PipelineSummaryItemDto[];
}
