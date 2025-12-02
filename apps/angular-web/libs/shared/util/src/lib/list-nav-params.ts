import type { Params } from '@angular/router';

/**
 * Extracts the list navigation params (search, pageNumber, orderBy, sortOrder) from a
 * route snapshot so a detail or edit page can reconstruct the exact list URL on back-navigation.
 * Reads from a snapshot (not a live observable) intentionally — the params are captured once at
 * navigation time; making this reactive would cause the back-link to drift as the URL changes.
 */
export function extractListNavParams(queryParams: Params): Record<string, string> {
  const result: Record<string, string> = {};
  if (queryParams['search']) {
    result['search'] = queryParams['search'] as string;
  }
  if (queryParams['pageNumber']) {
    result['pageNumber'] = queryParams['pageNumber'] as string;
  }
  if (queryParams['orderBy']) {
    result['orderBy'] = queryParams['orderBy'] as string;
  }
  if (queryParams['sortOrder']) {
    result['sortOrder'] = queryParams['sortOrder'] as string;
  }
  return result;
}
