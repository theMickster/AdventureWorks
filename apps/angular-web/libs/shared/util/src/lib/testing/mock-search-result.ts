/**
 * Structurally identical to `SearchResult<T>` in `libs/shared/data-access`. Duplicated locally —
 * not imported — because `shared-util` is `type:util` and the ESLint `depConstraints` only allow
 * `type:util` to depend on `type:util`; `shared/data-access` is `type:data-access`.
 */
export interface MockSearchResult<T> {
  readonly pageNumber: number;
  readonly pageSize: number;
  readonly totalPages: number;
  readonly hasPreviousPage: boolean;
  readonly hasNextPage: boolean;
  readonly totalRecords: number;
  readonly results: T[] | null;
}

/**
 * Builds a typed paginated search result with correct pagination math from a minimal set of overrides.
 * An explicit `results: null` override is preserved as-is (not coerced to `[]`) to support empty/error-state tests.
 */
export function mockSearchResult<T>(overrides: Partial<MockSearchResult<T>> = {}): MockSearchResult<T> {
  const pageNumber = overrides.pageNumber ?? 1;
  const pageSize = overrides.pageSize ?? 10;
  // 'results' in overrides (not ??) preserves an explicit `results: null` instead of coercing it to [].
  const results = 'results' in overrides ? (overrides.results as T[] | null) : [];
  const totalRecords = overrides.totalRecords ?? results?.length ?? 0;
  const totalPages = pageSize > 0 ? Math.ceil(totalRecords / pageSize) : 0;

  return {
    pageNumber,
    pageSize,
    totalPages,
    hasPreviousPage: pageNumber > 1,
    hasNextPage: pageNumber < totalPages,
    totalRecords,
    results,
  };
}
