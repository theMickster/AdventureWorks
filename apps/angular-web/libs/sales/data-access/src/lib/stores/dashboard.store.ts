import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { catchError, EMPTY, pipe, switchMap, tap } from 'rxjs';
import { setError, setLoaded, setLoading, withRequestStatus } from '@adventureworks-web/shared/data-access';
import type { SalesDashboard } from '../models/sales-dashboard.model';
import { SalesApiService } from '../services/sales-api.service';

export const DashboardStore = signalStore(
  { providedIn: 'root' },
  withDevtools('dashboard'),
  withState<{ dashboard: SalesDashboard | null }>({ dashboard: null }),
  withRequestStatus(),
  withMethods((store, salesApiService = inject(SalesApiService)) => ({
    load: rxMethod<void>(
      pipe(
        tap(() => patchState(store, setLoading())),
        // switchMap (not exhaustMap) cancels a stale in-flight load if load() is re-triggered before the response arrives.
        switchMap(() =>
          salesApiService.getSalesDashboard().pipe(
            tap((data) => patchState(store, { dashboard: data }, setLoaded())),
            catchError(() => {
              patchState(store, setError('Failed to load dashboard KPIs'));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),
  })),
);
