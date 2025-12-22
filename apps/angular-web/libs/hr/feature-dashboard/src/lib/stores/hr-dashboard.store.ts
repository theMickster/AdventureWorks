import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { catchError, EMPTY, pipe, switchMap, tap } from 'rxjs';
import { setError, setLoaded, setLoading, withRequestStatus } from '@adventureworks-web/shared/data-access';
import { HrApiService } from '@adventureworks-web/hr/data-access';
import type { EmployeeAggregates } from '@adventureworks-web/hr/data-access';

interface HrDashboardState {
  readonly aggregates: EmployeeAggregates | null;
  readonly lastUpdated: Date | null;
}

/**
 * One-shot HR dashboard aggregates payload — `withState`, not `withEntities`, mirroring
 * `OrgChartStore`/Sales `DashboardStore`.
 *
 * Feature-local (single consumer: `HrDashboardComponent`) rather than in `hr/data-access`,
 * matching the `OrgChartStore` precedent for feature-specific view state.
 *
 * Deviates from Sales `DashboardStore`: US-767 requires a manual refresh, so `load()` can be
 * called repeatedly (not just once from `ngOnInit`) and stamps `lastUpdated` on every successful
 * response.
 */
export const HrDashboardStore = signalStore(
  { providedIn: 'root' },
  withDevtools('hr-dashboard'),
  withState<HrDashboardState>({ aggregates: null, lastUpdated: null }),
  withRequestStatus(),
  withMethods((store, hrApi = inject(HrApiService)) => ({
    /** Loads (or reloads) the dashboard aggregates. Safe to call repeatedly — a fresh call cancels any stale in-flight request. */
    load: rxMethod<void>(
      pipe(
        tap(() => patchState(store, setLoading())),
        // switchMap (not exhaustMap) cancels a stale in-flight request if load()/refresh() is re-triggered before the response arrives.
        switchMap(() =>
          hrApi.getAggregates().pipe(
            tap((aggregates) => patchState(store, { aggregates, lastUpdated: new Date() }, setLoaded())),
            catchError(() => {
              patchState(store, setError('Failed to load HR dashboard data'));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),
  })),
);
