import { Signal, inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { Observable, catchError, EMPTY, exhaustMap, pipe, tap } from 'rxjs';
import { withRequestStatus, setLoading, setLoaded, setError } from '../store-features/with-request-status';
import { LookupApiService } from '../services/lookup-api.service';
import { AddressType } from '../models/lookup/address-type.model';
import { CountryRegion } from '../models/lookup/country-region.model';
import { Department } from '../models/lookup/department.model';
import { SalesTerritory } from '../models/lookup/sales-territory.model';
import { Shift } from '../models/lookup/shift.model';
import { StateProvince } from '../models/lookup/state-province.model';

interface LookupState {
  departments: Department[];
  shifts: Shift[];
  territories: SalesTerritory[];
  addressTypes: AddressType[];
  countryRegions: CountryRegion[];
  stateProvinces: StateProvince[];
}

const initialState: LookupState = {
  departments: [],
  shifts: [],
  territories: [],
  addressTypes: [],
  countryRegions: [],
  stateProvinces: [],
};

/**
 * Shared store for lookup/reference data used across domains.
 * Each load method fetches once and caches; subsequent calls are no-ops.
 *
 * NOTE: Single requestStatus tracks the most recent operation (last-write-wins).
 * If multiple load methods are called concurrently, status reflects whichever
 * completes last. Consumers loading multiple collections should not rely on
 * the shared isLoading/isLoaded/hasError signals for per-collection state.
 */
export const LookupStore = signalStore(
  { providedIn: 'root' },
  withDevtools('lookups'),
  withState(initialState),
  withRequestStatus(),
  withMethods((store, lookupApi = inject(LookupApiService)) => {
    function loadCollection<K extends keyof LookupState>(
      cached: Signal<LookupState[K]>,
      apiFn: () => Observable<LookupState[K]>,
      key: K,
    ) {
      return rxMethod<void>(
        pipe(
          exhaustMap(() => {
            if ((cached() as unknown[]).length > 0) return EMPTY;
            patchState(store, setLoading());
            return apiFn().pipe(
              tap((data) => patchState(store, { [key]: data } as Partial<LookupState>, setLoaded())),
              catchError((err: unknown) => {
                const message = err instanceof Error ? err.message : 'An unexpected error occurred';
                patchState(store, setError(message));
                return EMPTY;
              }),
            );
          }),
        ),
      );
    }

    return {
      loadDepartments: loadCollection(store.departments, () => lookupApi.getDepartments(), 'departments'),
      loadShifts: loadCollection(store.shifts, () => lookupApi.getShifts(), 'shifts'),
      loadTerritories: loadCollection(store.territories, () => lookupApi.getTerritories(), 'territories'),
      loadAddressTypes: loadCollection(store.addressTypes, () => lookupApi.getAddressTypes(), 'addressTypes'),
      loadCountryRegions: loadCollection(store.countryRegions, () => lookupApi.getCountryRegions(), 'countryRegions'),
      loadStateProvinces: loadCollection(store.stateProvinces, () => lookupApi.getStateProvinces(), 'stateProvinces'),
    };
  }),
);
