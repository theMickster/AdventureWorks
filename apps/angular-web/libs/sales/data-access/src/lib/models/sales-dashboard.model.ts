export interface DashboardTopPerformer {
  readonly salesPersonId: number;
  readonly name: string;
  readonly territory: string;
  readonly revenue: number;
  readonly orderCount: number;
}

export interface DashboardTerritory {
  readonly territoryId: number;
  readonly name: string;
  readonly group: string;
  readonly countryCode: string;
  readonly revenue: number;
  readonly orderCount: number;
}

export interface DashboardMonthlySalesTrend {
  readonly year: number;
  readonly month: number;
  readonly revenue: number;
  readonly orderCount: number;
}

export interface SalesDashboard {
  readonly totalRevenue: number;
  readonly orderCount: number;
  readonly averageOrderValue: number;
  readonly topPerformers: DashboardTopPerformer[];
  readonly territoryBreakdown: DashboardTerritory[];
  readonly monthlySalesTrend: DashboardMonthlySalesTrend[];
}
