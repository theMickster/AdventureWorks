import { inject } from '@angular/core';
import { patchState, signalStore, withMethods } from '@ngrx/signals';
import { setAllEntities, withEntities } from '@ngrx/signals/entities';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { catchError, EMPTY, pipe, switchMap, tap } from 'rxjs';
import {
  ApiEmptyResultError,
  setError,
  setLoaded,
  setLoading,
  setPaginationFromResult,
  withPagination,
  withRequestStatus,
} from '@adventureworks-web/shared/data-access';
import type { VendorListItem } from '../models/vendor-list-item.model';
import type { VendorListParams } from '../models/vendor-params.model';
import { PurchasingApiService } from '../services/purchasing-api.service';

/** Entity store for the risk-ranked, paginated vendor list. */
export const VendorStore = signalStore(
  { providedIn: 'root' },
  withDevtools('vendors'),
  withEntities<VendorListItem>(),
  withRequestStatus(),
  withPagination(),
  withMethods((store, purchasingApi = inject(PurchasingApiService)) => {
    /** Loads a page of vendors, applying the optional credit-rating/preferred/active filters. */
    const loadPage = rxMethod<VendorListParams>(
      pipe(
        tap(() => patchState(store, setLoading())),
        switchMap((params) =>
          purchasingApi.getVendors(params).pipe(
            tap((result) =>
              patchState(
                store,
                setAllEntities(result.results ?? [], { selectId: (vendor) => vendor.vendorId }),
                setPaginationFromResult(result),
                setLoaded(),
              ),
            ),
            catchError((err: unknown) => {
              if (err instanceof ApiEmptyResultError) {
                patchState(
                  store,
                  setAllEntities([] as VendorListItem[], { selectId: (vendor) => vendor.vendorId }),
                  setLoaded(),
                );
              } else {
                patchState(store, setError('Failed to load vendors'));
              }
              return EMPTY;
            }),
          ),
        ),
      ),
    );

    return {
      loadPage,
    };
  }),
);
