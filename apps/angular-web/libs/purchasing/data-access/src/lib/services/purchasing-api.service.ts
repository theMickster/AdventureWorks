import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import { toQueryString } from '@adventureworks-web/shared/data-access';
import type { VendorListItem } from '../models/vendor-list-item.model';
import type { VendorListParams } from '../models/vendor-params.model';

/** HTTP client for Purchasing domain endpoints (Vendors). */
@Injectable({ providedIn: 'root' })
export class PurchasingApiService {
  private readonly apiService = inject(ApiService);

  /** Fetches a paginated, risk-ranked page of vendors from GET /v1/vendors. */
  getVendors(params?: VendorListParams): Observable<SearchResult<VendorListItem>> {
    const query = params ? toQueryString(params) : '';
    return this.apiService.get<SearchResult<VendorListItem>>(`/v1/vendors${query}`);
  }
}
