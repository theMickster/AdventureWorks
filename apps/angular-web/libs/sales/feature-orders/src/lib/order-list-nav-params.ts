import type { Params } from '@angular/router';

/**
 * Extracts the order-list navigation params (orderDateFrom, orderDateTo, status, salesPersonId,
 * territoryId, pageNumber, orderBy, sortOrder) from a route snapshot so OrderDetailComponent can
 * reconstruct the exact order-list URL on back-navigation.
 * Reads from a snapshot (not a live observable) intentionally — the params are captured once at
 * navigation time; making this reactive would cause the back-link to drift as the URL changes.
 */
export function extractOrderListNavParams(queryParams: Params): Record<string, string> {
  const keys = [
    'orderDateFrom',
    'orderDateTo',
    'status',
    'salesPersonId',
    'territoryId',
    'pageNumber',
    'orderBy',
    'sortOrder',
  ];
  const result: Record<string, string> = {};
  for (const key of keys) {
    if (queryParams[key]) {
      result[key] = queryParams[key] as string;
    }
  }
  return result;
}
