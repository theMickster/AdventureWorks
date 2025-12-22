/** A single vendor row in the risk-ranked vendor list (`GET /v1/vendors`). */
export interface VendorListItem {
  readonly vendorId: number;
  readonly name: string;
  readonly accountNumber: string;
  /** Human-readable credit rating label (e.g. "Superior", "Average") — already server-formatted. */
  readonly creditRatingLabel: string;
  readonly preferredVendorStatus: boolean;
  readonly activeFlag: boolean;
  readonly totalSpend: number;
  readonly poCount: number;
  readonly isHighRisk: boolean;
}
