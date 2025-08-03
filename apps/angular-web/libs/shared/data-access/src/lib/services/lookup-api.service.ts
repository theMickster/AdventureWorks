import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@adventureworks-web/shared/util';
import { AddressType } from '../models/lookup/address-type.model';
import { CountryRegion } from '../models/lookup/country-region.model';
import { Department } from '../models/lookup/department.model';
import { SalesTerritory } from '../models/lookup/sales-territory.model';
import { Shift } from '../models/lookup/shift.model';
import { StateProvince } from '../models/lookup/state-province.model';

/** HTTP client for shared lookup/reference data endpoints. */
@Injectable({ providedIn: 'root' })
export class LookupApiService {
  private readonly apiService = inject(ApiService);

  getDepartments(): Observable<Department[]> {
    return this.apiService.get<Department[]>('/v1/departments');
  }

  getShifts(): Observable<Shift[]> {
    return this.apiService.get<Shift[]>('/v1/shifts');
  }

  getTerritories(): Observable<SalesTerritory[]> {
    return this.apiService.get<SalesTerritory[]>('/v1/territories');
  }

  getAddressTypes(): Observable<AddressType[]> {
    return this.apiService.get<AddressType[]>('/v1/addressTypes');
  }

  getCountryRegions(): Observable<CountryRegion[]> {
    return this.apiService.get<CountryRegion[]>('/v1/countries');
  }

  getStateProvinces(): Observable<StateProvince[]> {
    return this.apiService.get<StateProvince[]>('/v1/states');
  }
}
