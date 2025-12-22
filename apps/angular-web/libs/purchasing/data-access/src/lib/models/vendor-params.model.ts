/**
 * Query params for GET /v1/vendors. Server always sorts by total spend descending — no sort param.
 *
 * Note: field is `pageNumber` (not `page`) to match the API's actual query-string parameter name —
 * `toQueryString` forwards field names verbatim, so a `page` field would silently never reach the server.
 */
export interface VendorListParams {
  readonly pageNumber: number;
  readonly pageSize?: number;
  /** 1=Superior, 2=Excellent, 3=Above Average, 4=Average, 5=Below Average. */
  readonly creditRating?: number;
  readonly preferredVendorStatus?: boolean;
  readonly activeFlag?: boolean;
}
