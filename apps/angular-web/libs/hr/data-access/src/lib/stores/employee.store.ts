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
import type { Employee } from '../models/employee.model';
import type { EmployeeCreate } from '../models/employee-create.model';
import type { EmployeeUpdate } from '../models/employee-update.model';
import type { EmployeeParams } from '../models/employee-params.model';
import type { EmployeeSearchBody } from '../models/employee-search.model';
import type { EmployeeHire, EmployeeTerminate, EmployeeRehire } from '../models/employee-lifecycle.model';
import type { JsonPatchOperation } from '../models/json-patch.model';
import { HrApiService } from '../services/hr-api.service';

/** Entity store for HumanResources.Employee with CRUD, patch, and lifecycle operations. */
export const EmployeeStore = signalStore(
  { providedIn: 'root' },
  withDevtools('employees'),
  withEntities<Employee>(),
  withRequestStatus(),
  withPagination(),
  withMethods((store, hrApi = inject(HrApiService)) => ({
    loadPage: rxMethod<EmployeeParams>(
      pipe(
        tap(() => patchState(store, setLoading())),
        switchMap((params) =>
          hrApi.getEmployees(params).pipe(
            tap((result) =>
              patchState(store, setAllEntities(result.results ?? []), setPaginationFromResult(result), setLoaded()),
            ),
            catchError((err: unknown) => {
              if (err instanceof ApiEmptyResultError) {
                patchState(store, setAllEntities([] as Employee[]), setLoaded());
              } else {
                const message = err instanceof Error ? err.message : 'Failed to load employees';
                patchState(store, setError(message));
              }
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    search: rxMethod<{ params: EmployeeParams; body: EmployeeSearchBody }>(
      pipe(
        tap(() => patchState(store, setLoading())),
        switchMap(({ params, body }) =>
          hrApi.searchEmployees(params, body).pipe(
            tap((result) =>
              patchState(store, setAllEntities(result.results ?? []), setPaginationFromResult(result), setLoaded()),
            ),
            catchError((err: unknown) => {
              if (err instanceof ApiEmptyResultError) {
                patchState(store, setAllEntities([] as Employee[]), setLoaded());
              } else {
                const message = err instanceof Error ? err.message : 'Failed to search employees';
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
          hrApi.getEmployee(id).pipe(
            tap((entity) => patchState(store, setAllEntities([entity]), setLoaded())),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to load employee';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    create: rxMethod<EmployeeCreate>(
      pipe(
        tap(() => patchState(store, setLoading())),
        exhaustMap((model) =>
          hrApi.createEmployee(model).pipe(
            tap((entity) => patchState(store, addEntity(entity), setLoaded())),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to create employee';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    update: rxMethod<{ id: number; model: EmployeeUpdate }>(
      pipe(
        tap(() => patchState(store, setLoading())),
        exhaustMap(({ id, model }) =>
          hrApi.updateEmployee(id, model).pipe(
            tap((entity) => patchState(store, updateEntity({ id, changes: entity }), setLoaded())),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to update employee';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    patch: rxMethod<{ id: number; operations: JsonPatchOperation[] }>(
      pipe(
        tap(() => patchState(store, setLoading())),
        exhaustMap(({ id, operations }) =>
          hrApi.patchEmployee(id, operations).pipe(
            tap((entity) => patchState(store, updateEntity({ id, changes: entity }), setLoaded())),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to patch employee';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    hireEmployee: rxMethod<{ id: number; model: EmployeeHire }>(
      pipe(
        tap(() => patchState(store, setLoading())),
        exhaustMap(({ id, model }) =>
          hrApi.hireEmployee(id, model).pipe(
            switchMap(() =>
              hrApi.getEmployee(id).pipe(
                tap((employee) => patchState(store, updateEntity({ id, changes: employee }), setLoaded())),
                catchError(() => {
                  patchState(store, setLoaded());
                  return EMPTY;
                }),
              ),
            ),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to hire employee';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    terminateEmployee: rxMethod<{ id: number; model: EmployeeTerminate }>(
      pipe(
        tap(() => patchState(store, setLoading())),
        exhaustMap(({ id, model }) =>
          hrApi.terminateEmployee(id, model).pipe(
            switchMap(() =>
              hrApi.getEmployee(id).pipe(
                tap((employee) => patchState(store, updateEntity({ id, changes: employee }), setLoaded())),
                catchError(() => {
                  patchState(store, setLoaded());
                  return EMPTY;
                }),
              ),
            ),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to terminate employee';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    rehireEmployee: rxMethod<{ id: number; model: EmployeeRehire }>(
      pipe(
        tap(() => patchState(store, setLoading())),
        exhaustMap(({ id, model }) =>
          hrApi.rehireEmployee(id, model).pipe(
            switchMap(() =>
              hrApi.getEmployee(id).pipe(
                tap((employee) => patchState(store, updateEntity({ id, changes: employee }), setLoaded())),
                catchError(() => {
                  patchState(store, setLoaded());
                  return EMPTY;
                }),
              ),
            ),
            catchError((err: unknown) => {
              const message = err instanceof Error ? err.message : 'Failed to rehire employee';
              patchState(store, setError(message));
              return EMPTY;
            }),
          ),
        ),
      ),
    ),
  })),
);
