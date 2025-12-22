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
import type { WorkOrder } from '../models/work-order.model';
import type { WorkOrderParams } from '../models/work-order-params.model';
import { WorkOrderApiService } from '../services/work-order-api.service';

/**
 * Entity store for the Production.WorkOrder paginated, filterable list.
 *
 * - `loadPage` — loads one page of work orders; owns all HTTP-bridging logic (loading/loaded/error
 *   state transitions).
 * - `applyFilters` — thin wrapper over `loadPage`, kept as a distinct, explicit entry point for
 *   filter-bar callers — mirrors `SalesOrderStore.applyFilters`'s role as the filter-changed entry
 *   point, minus the sales-domain's second analytics side-effect (work orders has no analytics
 *   feature). It does NOT override `pageNumber` — callers (e.g. `WorkOrderListComponent`) are
 *   responsible for passing the correct page, matching `SalesOrderStore.applyFilters`.
 */
export const WorkOrderStore = signalStore(
  { providedIn: 'root' },
  withDevtools('workOrders'),
  withEntities<WorkOrder>(),
  withRequestStatus(),
  withPagination(25),
  withMethods((store, workOrderApi = inject(WorkOrderApiService)) => {
    /**
     * Loads one page of work orders into the entity collection, replacing the prior page.
     * An empty result (ApiEmptyResultError) is treated as a successful empty page, not an error,
     * so a filter that matches nothing clears the grid rather than showing the error toast.
     */
    const loadPage = rxMethod<WorkOrderParams>(
      pipe(
        tap(() => patchState(store, setLoading())),
        switchMap((params) =>
          workOrderApi.getWorkOrders(params).pipe(
            tap((result) =>
              patchState(store, setAllEntities(result.results ?? [], { selectId: (order) => order.workOrderId }), setPaginationFromResult(result), setLoaded()),
            ),
            catchError((err: unknown) => {
              if (err instanceof ApiEmptyResultError) {
                patchState(store, setAllEntities([] as WorkOrder[], { selectId: (order) => order.workOrderId }), setLoaded());
              } else {
                patchState(store, setError('Failed to load work orders'));
              }
              return EMPTY;
            }),
          ),
        ),
      ),
    );

    /** Delegates to `loadPage`, matching `SalesOrderStore.applyFilters`'s pass-through of `pageNumber`. */
    function applyFilters(params: WorkOrderParams): void {
      loadPage(params);
    }

    return {
      loadPage,
      applyFilters,
    };
  }),
);
