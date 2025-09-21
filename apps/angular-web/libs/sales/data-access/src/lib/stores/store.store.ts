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

type StoreListRequest =
  | { kind: 'loadPage'; params: StoreParams }
  | { kind: 'search'; params: StoreParams; body: StoreSearchBody };

/** Entity store for Sales.Store with paginated list, search, and CRUD operations. */
export const StoreStore = signalStore(
  { providedIn: 'root' },
  withDevtools('stores'),
  withEntities<Store>(),
  withRequestStatus(),
  withPagination(),
  withMethods((store, salesApi = inject(SalesApiService)) => {
    let lastListRequest: StoreListRequest | null = null;

    const loadPage = rxMethod<StoreParams>(
      pipe(
        tap((params) => {
          lastListRequest = { kind: 'loadPage', params: { ...params } };
          patchState(store, setLoading());
        }),
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
    );

    const search = rxMethod<{ params: StoreParams; body: StoreSearchBody }>(
      pipe(
        tap(({ params, body }) => {
          lastListRequest = {
            kind: 'search',
            params: { ...params },
            body: { ...body },
          };
          patchState(store, setLoading());
        }),
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
    );

    const loadById = rxMethod<number>(
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
    );

    const create = rxMethod<StoreCreate>(
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
    );

    const update = rxMethod<{ id: number; model: StoreUpdate }>(
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
    );

    const refresh = (): void => {
      if (!lastListRequest) {
        return;
      }

      if (lastListRequest.kind === 'loadPage') {
        loadPage({ ...lastListRequest.params });
        return;
      }

      search({
        params: { ...lastListRequest.params },
        body: { ...lastListRequest.body },
      });
    };

    /** Applies a SignalR store-updated payload when it contains a valid store id. */
    const applySignalrStoreUpdated = (payload: unknown): void => {
      if (!payload || typeof payload !== 'object') {
        return;
      }

      const candidate = payload as Partial<Store> & { id?: unknown };
      if (typeof candidate.id !== 'number') {
        return;
      }

      patchState(store, updateEntity({ id: candidate.id, changes: candidate }));
    };

    return {
      loadPage,
      search,
      loadById,
      create,
      update,
      refresh,
      applySignalrStoreUpdated,
    };
  }),
);
