/** Full detail for a single customer, including LTV rank and lifetime-spend metrics (`GET /v1/customers/:id`). */
export interface CustomerDetail {
  readonly customerId: number;
  readonly displayName: string;
  readonly ltvRank: number;
  /** Total number of customers ranked — the denominator for `ltvRank`. */
  readonly totalCustomerCount: number;
  readonly totalSpend: number;
  readonly orderCount: number;
  /** `totalSpend / orderCount`, server-computed. Zero — not `NaN` — when the customer has no orders. */
  readonly avgOrderValue: number;
  readonly lastOrderDate: string | null;
  readonly isInactive: boolean;
  readonly customerType: 'Individual' | 'Store';
  readonly storeId?: number | null;
  readonly storeName?: string | null;
  readonly firstName?: string | null;
  readonly lastName?: string | null;
}
