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
import type { CustomerParams } from '../models/customer-params.model';
import type { CustomerListItem } from '../models/customer-list-item.model';
import { SalesApiService } from '../services/sales-api.service';

/** Entity store for the Customer LTV list — server-side paginated (large, unbounded dataset). */
export const CustomerStore = signalStore(
  { providedIn: 'root' },
  withDevtools('customers'),
  withEntities<CustomerListItem>(),
  withRequestStatus(),
  withPagination(),
  withMethods((store, salesApi = inject(SalesApiService)) => {
    const loadPage = rxMethod<CustomerParams>(
      pipe(
        tap(() => patchState(store, setLoading())),
        switchMap((params) =>
          salesApi.getCustomers(params).pipe(
            tap((result) =>
              patchState(
                store,
                setAllEntities(result.results ?? [], { selectId: (customer) => customer.customerId }),
                setPaginationFromResult(result),
                setLoaded(),
              ),
            ),
            catchError((err: unknown) => {
              if (err instanceof ApiEmptyResultError) {
                patchState(store, setAllEntities([] as CustomerListItem[], { selectId: (customer) => customer.customerId }), setLoaded());
              } else {
                patchState(store, setError('Failed to load customers'));
              }
              return EMPTY;
            }),
          ),
        ),
      ),
    );

    return { loadPage };
  }),
);
