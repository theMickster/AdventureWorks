/** Read model for a single row in the Customer LTV list (GET /v1/customers). */
export interface CustomerListItem {
  readonly customerId: number;
  readonly displayName: string;
  readonly customerType: 'Individual' | 'Store';
  readonly storeId?: number | null;
  readonly ltvRank: number;
  readonly totalSpend: number;
  readonly orderCount: number;
  readonly isInactive: boolean;
}
