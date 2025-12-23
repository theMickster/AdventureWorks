import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import { toQueryString } from '@adventureworks-web/shared/data-access';
import type { VendorListItem } from '../models/vendor-list-item.model';
import type { VendorListParams } from '../models/vendor-params.model';
import type { VendorDetail } from '../models/vendor-detail.model';
import type { PurchaseOrderSummary, VendorPurchaseOrderParams } from '../models/purchase-order-summary.model';

/** HTTP client for Purchasing domain endpoints (Vendors). */
@Injectable({ providedIn: 'root' })
export class PurchasingApiService {
  private readonly apiService = inject(ApiService);

  /** Fetches a paginated, risk-ranked page of vendors from GET /v1/vendors. */
  getVendors(params?: VendorListParams): Observable<SearchResult<VendorListItem>> {
    const query = params ? toQueryString(params) : '';
    return this.apiService.get<SearchResult<VendorListItem>>(`/v1/vendors${query}`);
  }

  /** Fetches a single vendor's profile and spend metrics from GET /v1/vendors/:id. */
  getVendorDetail(vendorId: number): Observable<VendorDetail> {
    return this.apiService.get<VendorDetail>(`/v1/vendors/${vendorId}`);
  }

  /** Fetches a paginated, filterable page of a vendor's purchase order history from GET /v1/vendors/:id/purchase-orders. */
  getVendorPurchaseOrders(
    vendorId: number,
    params?: VendorPurchaseOrderParams,
  ): Observable<SearchResult<PurchaseOrderSummary>> {
    const query = params ? toQueryString(params) : '';
    return this.apiService.get<SearchResult<PurchaseOrderSummary>>(`/v1/vendors/${vendorId}/purchase-orders${query}`);
  }
}
