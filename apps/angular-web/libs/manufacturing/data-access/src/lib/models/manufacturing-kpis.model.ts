/** Aggregate production KPI values returned by GET /v1/manufacturing/kpis. */
export interface ManufacturingKpisDto {
  readonly totalWorkOrders: number;
  readonly totalOrdered: number;
  readonly totalStocked: number;
  readonly totalScrapped: number;
  readonly overallYieldPct: number;
  readonly overallScrapPct: number;
}
