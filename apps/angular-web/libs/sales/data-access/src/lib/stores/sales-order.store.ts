import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
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
import type { SalesOrderAnalytics, SalesOrderAnalyticsFilter } from '../models/sales-order-analytics.model';
import type { SalesOrderParams } from '../models/sales-order-params.model';
import type { SalesOrder } from '../models/sales-order.model';
import { SalesApiService } from '../services/sales-api.service';

/**
 * Entity store for the Sales.SalesOrder paginated, filterable list.
 *
 * - `loadPage` — loads one page of orders (list only, no analytics side-effect).
 * - `loadAnalytics` — fetches filter-scoped aggregates; uses switchMap so rapid calls cancel in-flight requests.
 * - `applyFilters` — use for filter changes; dispatches both `loadPage` and `loadAnalytics`.
 *   For pagination/sort changes (page number, sort column), call `loadPage` directly to avoid an unnecessary analytics re-fetch.
 * - Analytics state (`analyticsStatus`, `analytics`, `analyticsIsLoading`, `analyticsHasError`) is independent
 *   of `requestStatus` from `withRequestStatus()`.
 */
export const SalesOrderStore = signalStore(
  { providedIn: 'root' },
  withDevtools('salesOrders'),
  withEntities<SalesOrder>(),
  withRequestStatus(),
  withPagination(),
  withState<{
    analytics: SalesOrderAnalytics | null;
    analyticsStatus: 'idle' | 'loading' | 'loaded' | 'error';
    analyticsError: string | null;
  }>({
    analytics: null,
    analyticsStatus: 'idle',
    analyticsError: null,
  }),
  withComputed((store) => ({
    analyticsIsLoading: computed(() => store.analyticsStatus() === 'loading'),
    analyticsHasError: computed(() => store.analyticsStatus() === 'error'),
  })),
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
                patchState(store, setError('Failed to load sales orders'));
              }
              return EMPTY;
            }),
          ),
        ),
      ),
    );

    const loadAnalytics = rxMethod<SalesOrderAnalyticsFilter>(
      pipe(
        tap(() => patchState(store, { analyticsStatus: 'loading', analyticsError: null })),
        switchMap((filter) =>
          salesApi.getOrderAnalytics(filter).pipe(
            tap((data) => patchState(store, { analytics: data, analyticsStatus: 'loaded' })),
            catchError(() => {
              patchState(store, { analyticsStatus: 'error', analyticsError: 'Failed to load order analytics' });
              return EMPTY;
            }),
          ),
        ),
      ),
    );

    function applyFilters(params: SalesOrderParams): void {
      loadPage(params);
      loadAnalytics({
        orderDateFrom: params.orderDateFrom,
        orderDateTo: params.orderDateTo,
        status: params.status,
        salesPersonId: params.salesPersonId,
        territoryId: params.territoryId,
      });
    }

    return {
      loadPage,
      loadAnalytics,
      applyFilters,
    };
  }),
);
