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
import type { SalesPersonParams } from '../models/sales-person-params.model';
import type { SalesPersonSearchBody } from '../models/sales-person-search.model';
import type { SalesPerson, SalesPersonCreate, SalesPersonUpdate } from '../models/sales-person.model';
import { SalesApiService } from '../services/sales-api.service';

/** Entity store for Sales.SalesPerson with paginated list, search, and CRUD operations. */
export const SalesPersonStore = signalStore(
  { providedIn: 'root' },
  withDevtools('salesPersons'),
  withEntities<SalesPerson>(),
  withRequestStatus(),
  withPagination(),
  withMethods((store, salesApi = inject(SalesApiService)) => ({
    loadPage: rxMethod<SalesPersonParams>(
      pipe(
        tap(() => patchState(store, setLoading())),
        switchMap((params) =>
          salesApi.getSalesPersons(params).pipe(
            tap((result) =>
              patchState(store, setAllEntities(result.results ?? []), setPaginationFromResult(result), setLoaded()),
            ),
            catchError((err: unknown) => {
              if (err instanceof ApiEmptyResultError) {
                patchState(store, setAllEntities([] as SalesPerson[]), setLoaded());
              } else {
                const message = err instanceof Error ? err.message : 'Failed to load sales persons';
                patchState(store, setError(message));
              }
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    search: rxMethod<{ params: SalesPersonParams; body: SalesPersonSearchBody }>(
      pipe(
        tap(() => patchState(store, setLoading())),
        switchMap(({ params, body }) =>
          salesApi.searchSalesPersons(params, body).pipe(
            tap((result) =>
              patchState(store, setAllEntities(result.results ?? []), setPaginationFromResult(result), setLoaded()),
            ),
            catchError((err: unknown) => {
              if (err instanceof ApiEmptyResultError) {
                patchState(store, setAllEntities([] as SalesPerson[]), setLoaded());
              } else {
                const message = err instanceof Error ? err.message : 'Failed to search sales persons';
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
          salesApi.getSalesPerson(id).pipe(
            tap((entity) => patchState(store, setAllEntities([entity]), setLoaded())),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to load sales person';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    create: rxMethod<SalesPersonCreate>(
      pipe(
        tap(() => patchState(store, setLoading())),
        exhaustMap((model) =>
          salesApi.createSalesPerson(model).pipe(
            tap((entity) => patchState(store, addEntity(entity), setLoaded())),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to create sales person';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    update: rxMethod<{ id: number; model: SalesPersonUpdate }>(
      pipe(
        tap(() => patchState(store, setLoading())),
        exhaustMap(({ id, model }) =>
          salesApi.updateSalesPerson(id, model).pipe(
            tap((entity) => patchState(store, updateEntity({ id, changes: entity }), setLoaded())),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to update sales person';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),
  })),
);
