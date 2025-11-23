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
import type { SalesOrderParams } from '../models/sales-order-params.model';
import type { SalesOrder } from '../models/sales-order.model';
import { SalesApiService } from '../services/sales-api.service';

/** Entity store for the Sales.SalesOrder paginated, filterable list. Read-only: loadPage only. */
export const SalesOrderStore = signalStore(
  { providedIn: 'root' },
  withDevtools('salesOrders'),
  withEntities<SalesOrder>(),
  withRequestStatus(),
  withPagination(),
  withMethods((store, salesApi = inject(SalesApiService)) => {
    /**
     * Loads one page of sales orders into the entity collection, replacing the prior page.
     * An empty result (ApiEmptyResultError) is treated as a successful empty page, not an error,
     * so a filter that matches nothing clears the grid rather than showing the error toast.
     */
    const loadPage = rxMethod<SalesOrderParams>(
      pipe(
        tap(() => patchState(store, setLoading())),
        switchMap((params) =>
          salesApi.getSalesOrders(params).pipe(
            tap((result) =>
              patchState(store, setAllEntities(result.results ?? [], { selectId: (order) => order.salesOrderId }), setPaginationFromResult(result), setLoaded()),
            ),
            catchError((err: unknown) => {
              if (err instanceof ApiEmptyResultError) {
                patchState(store, setAllEntities([] as SalesOrder[], { selectId: (order) => order.salesOrderId }), setLoaded());
              } else {
                const message = err instanceof Error ? err.message : 'Failed to load sales orders';
                patchState(store, setError(message));
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
