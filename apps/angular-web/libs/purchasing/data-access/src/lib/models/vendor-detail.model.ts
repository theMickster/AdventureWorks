/** Full profile and spend metrics for a single vendor (`GET /v1/vendors/:id`). */
export interface VendorDetail {
  readonly vendorId: number;
  readonly name: string;
  readonly accountNumber: string;
  /** Human-readable credit rating label (e.g. "Superior", "Average") — already server-formatted. */
  readonly creditRatingLabel: string;
  readonly preferredVendorStatus: boolean;
  readonly activeFlag: boolean;
  readonly totalSpend: number;
  readonly poCount: number;
  /** `totalSpend / poCount`, server-computed. Zero — not `NaN` — when the vendor has no purchase orders. */
  readonly avgPoValue: number;
}
