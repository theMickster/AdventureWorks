import type { Params } from '@angular/router';

/**
 * Extracts the order-list navigation params from a route snapshot so the detail page can
 * reconstruct the exact list URL on back-navigation. Reads from a snapshot intentionally —
 * params are captured once at navigation time; making this reactive would cause the back-link
 * to drift as the URL changes.
 */
export function extractListNavParams(queryParams: Params): Record<string, string | number | null> {
  const keys = ['orderDateFrom', 'orderDateTo', 'status', 'salesPersonId', 'territoryId', 'pageNumber', 'orderBy', 'sortOrder'];
  const result: Record<string, string | number | null> = {};
  for (const key of keys) {
    if (queryParams[key]) {
      result[key] = queryParams[key] as string;
    }
  }
  return result;
}
