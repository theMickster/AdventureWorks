/** Query parameters for GET /v1/customers. */
export interface CustomerParams {
  readonly pageNumber: number;
  readonly pageSize?: number;
  readonly search?: string;
}
