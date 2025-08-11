import { inject } from '@angular/core';
import { patchState, signalStore, withMethods } from '@ngrx/signals';
import { addEntity, setAllEntities, updateEntity, withEntities } from '@ngrx/signals/entities';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { catchError, EMPTY, exhaustMap, pipe, switchMap, tap } from 'rxjs';
import {
  ApiEmptyResultError,
  setError,
  setLoaded,
  setLoading,
  setPaginationFromResult,
  withPagination,
  withRequestStatus,
} from '@adventureworks-web/shared/data-access';
import type { StoreParams } from '../models/store-params.model';
import type { StoreSearchBody } from '../models/store-search.model';
import type { Store, StoreCreate, StoreUpdate } from '../models/store.model';
import { SalesApiService } from '../services/sales-api.service';

/** Entity store for Sales.Store with paginated list, search, and CRUD operations. */
export const StoreStore = signalStore(
  { providedIn: 'root' },
  withDevtools('stores'),
  withEntities<Store>(),
  withRequestStatus(),
  withPagination(),
  withMethods((store, salesApi = inject(SalesApiService)) => ({
    loadPage: rxMethod<StoreParams>(
      pipe(
        tap(() => patchState(store, setLoading())),
        switchMap((params) =>
          salesApi.getStores(params).pipe(
            tap((result) =>
              patchState(store, setAllEntities(result.results ?? []), setPaginationFromResult(result), setLoaded()),
            ),
            catchError((err: unknown) => {
              if (err instanceof ApiEmptyResultError) {
                patchState(store, setAllEntities([] as Store[]), setLoaded());
              } else {
                const message = err instanceof Error ? err.message : 'Failed to load stores';
                patchState(store, setError(message));
              }
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    search: rxMethod<{ params: StoreParams; body: StoreSearchBody }>(
      pipe(
        tap(() => patchState(store, setLoading())),
        switchMap(({ params, body }) =>
          salesApi.searchStores(params, body).pipe(
            tap((result) =>
              patchState(store, setAllEntities(result.results ?? []), setPaginationFromResult(result), setLoaded()),
            ),
            catchError((err: unknown) => {
              if (err instanceof ApiEmptyResultError) {
                patchState(store, setAllEntities([] as Store[]), setLoaded());
              } else {
                const message = err instanceof Error ? err.message : 'Failed to search stores';
                patchState(store, setError(message));
              }
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    loadById: rxMethod<number>(
      pipe(
        tap(() => patchState(store, setLoading())),
        exhaustMap((id) =>
          salesApi.getStore(id).pipe(
            tap((entity) => patchState(store, setAllEntities([entity]), setLoaded())),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to load store';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    create: rxMethod<StoreCreate>(
      pipe(
        tap(() => patchState(store, setLoading())),
        exhaustMap((model) =>
          salesApi.createStore(model).pipe(
            tap((entity) => patchState(store, addEntity(entity), setLoaded())),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to create store';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    update: rxMethod<{ id: number; model: StoreUpdate }>(
      pipe(
        tap(() => patchState(store, setLoading())),
        exhaustMap(({ id, model }) =>
          salesApi.updateStore(id, model).pipe(
            tap((entity) => patchState(store, updateEntity({ id, changes: entity }), setLoaded())),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to update store';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),
  })),
);
