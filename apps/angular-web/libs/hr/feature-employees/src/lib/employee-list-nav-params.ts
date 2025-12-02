import type { Params } from '@angular/router';

/**
 * Extracts the employee-list navigation params (name, status, pageNumber) from a route snapshot
 * so EmployeeDetailComponent can reconstruct the exact employee-list URL on back-navigation.
 * Mirrors `extractOrderListNavParams` — the shared `extractListNavParams` helper only recognizes
 * `search`/`orderBy`/`sortOrder`, none of which `EmployeeListComponent` writes to the URL.
 */
export function extractEmployeeListNavParams(queryParams: Params): Record<string, string> {
  const keys = ['name', 'status', 'pageNumber'];
  const result: Record<string, string> = {};
  for (const key of keys) {
    if (queryParams[key]) {
      result[key] = queryParams[key] as string;
    }
  }
  return result;
}
