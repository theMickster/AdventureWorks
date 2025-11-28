/** Reserved for US-740 (charts) — not yet consumed; kept here to avoid a breaking index.ts change when Feature 713 ships. */
export interface DashboardTopPerformer {
  readonly salesPersonId: number;
  readonly name: string;
  readonly totalSales: number;
}

/** Reserved for US-742 (territory breakdown) — not yet consumed; see DashboardTopPerformer. */
export interface DashboardTerritory {
  readonly territoryId: number;
  readonly name: string;
  readonly totalSales: number;
}

/** Reserved for US-741 (leaderboard/trend chart) — not yet consumed; see DashboardTopPerformer. */
export interface DashboardMonthlySalesTrend {
  readonly year: number;
  readonly month: number;
  readonly totalSales: number;
}

export interface SalesDashboard {
  readonly totalRevenue: number;
  readonly orderCount: number;
  readonly averageOrderValue: number;
  readonly topPerformers: DashboardTopPerformer[];
  readonly territoryBreakdown: DashboardTerritory[];
  readonly monthlySalesTrend: DashboardMonthlySalesTrend[];
}
