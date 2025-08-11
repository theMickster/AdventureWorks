import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import { toQueryString } from '@adventureworks-web/shared/data-access';
import type { Store, StoreCreate, StoreUpdate } from '../models/store.model';
import type { SalesPerson, SalesPersonCreate, SalesPersonUpdate } from '../models/sales-person.model';
import type { StoreParams } from '../models/store-params.model';
import type { SalesPersonParams } from '../models/sales-person-params.model';
import type { StoreSearchBody } from '../models/store-search.model';
import type { SalesPersonSearchBody } from '../models/sales-person-search.model';

/** HTTP client for Sales domain endpoints (Stores and Sales Persons). */
@Injectable({ providedIn: 'root' })
export class SalesApiService {
  private readonly apiService = inject(ApiService);

  getStores(params?: StoreParams): Observable<SearchResult<Store>> {
    const query = params ? toQueryString(params) : '';
    return this.apiService.get<SearchResult<Store>>(`/v1/stores${query}`);
  }

  getStore(id: number): Observable<Store> {
    return this.apiService.get<Store>(`/v1/stores/${id}`);
  }

  createStore(model: StoreCreate): Observable<Store> {
    return this.apiService.post<Store>('/v1/stores', model);
  }

  updateStore(id: number, model: StoreUpdate): Observable<Store> {
    return this.apiService.put<Store>(`/v1/stores/${id}`, model);
  }

  searchStores(params: StoreParams, body: StoreSearchBody): Observable<SearchResult<Store>> {
    const query = params ? toQueryString(params) : '';
    return this.apiService.post<SearchResult<Store>>(`/v1/stores/search${query}`, body);
  }

  getSalesPersons(params?: SalesPersonParams): Observable<SearchResult<SalesPerson>> {
    const query = params ? toQueryString(params) : '';
    return this.apiService.get<SearchResult<SalesPerson>>(`/v1/salespersons${query}`);
  }

  getSalesPerson(id: number): Observable<SalesPerson> {
    return this.apiService.get<SalesPerson>(`/v1/salespersons/${id}`);
  }

  createSalesPerson(model: SalesPersonCreate): Observable<SalesPerson> {
    return this.apiService.post<SalesPerson>('/v1/salespersons', model);
  }

  updateSalesPerson(id: number, model: SalesPersonUpdate): Observable<SalesPerson> {
    return this.apiService.put<SalesPerson>(`/v1/salespersons/${id}`, model);
  }

  searchSalesPersons(params: SalesPersonParams, body: SalesPersonSearchBody): Observable<SearchResult<SalesPerson>> {
    const query = params ? toQueryString(params) : '';
    return this.apiService.post<SearchResult<SalesPerson>>(`/v1/salespersons/search${query}`, body);
  }
}
