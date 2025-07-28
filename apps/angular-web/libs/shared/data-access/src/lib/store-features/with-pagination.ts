import { computed } from '@angular/core';
import { signalStoreFeature, withComputed, withState } from '@ngrx/signals';
import { SearchResult } from '../models/search-result.model';

export function setPaginationFromResult(result: SearchResult<unknown>): {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
} {
  return {
    pageNumber: result.pageNumber,
    pageSize: result.pageSize,
    totalPages: result.totalPages,
    totalRecords: result.totalRecords,
  };
}

export function withPagination(defaultPageSize = 10) {
  return signalStoreFeature(
    withState({
      pageNumber: 1,
      pageSize: defaultPageSize,
      totalPages: 0,
      totalRecords: 0,
    }),
    withComputed((store) => ({
      hasPreviousPage: computed(() => store.pageNumber() > 1),
      hasNextPage: computed(() => store.pageNumber() < store.totalPages()),
    })),
  );
}
