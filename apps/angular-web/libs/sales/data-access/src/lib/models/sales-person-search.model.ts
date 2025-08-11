/** Search criteria for POST /v1/salespersons/search. */
export interface SalesPersonSearchBody {
  readonly id?: number | null;
  readonly firstName?: string | null;
  readonly lastName?: string | null;
  readonly salesTerritoryId?: number | null;
  readonly salesTerritoryName?: string | null;
  readonly salesTerritoryGroupName?: string | null;
  readonly emailAddress?: string | null;
}
