export interface SearchResult<T> {
  readonly pageNumber: number;
  readonly pageSize: number;
  readonly totalPages: number;
  readonly hasPreviousPage: boolean;
  readonly hasNextPage: boolean;
  readonly totalRecords: number;
  readonly results: T[] | null;
}
