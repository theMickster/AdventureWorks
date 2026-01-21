import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import { toQueryString } from '@adventureworks-web/shared/data-access';
import type { WorkOrder } from '../models/work-order.model';
import type { WorkOrderDetail } from '../models/work-order-detail.model';
import type { WorkOrderParams } from '../models/work-order-params.model';
import type { ManufacturingKpisDto } from '../models/manufacturing-kpis.model';
import type { ManufacturingQualityScorecard } from '../models/manufacturing-quality-scorecard.model';
import type { ProductLookup } from '../models/product-lookup.model';
import type { ScrapReason } from '../models/scrap-reason.model';

/** Rows requested per typeahead search; the dropdown only needs a short list of candidates. */
const PRODUCT_SEARCH_PAGE_SIZE = 10;

/** HTTP client for Manufacturing domain endpoints (Work Orders and KPIs) and the lookups its filters need. */
@Injectable({ providedIn: 'root' })
export class WorkOrderApiService {
  private readonly apiService = inject(ApiService);

  /** Fetches a paginated, filterable page of work-order list rows from GET /v1/work-orders. */
  getWorkOrders(params?: WorkOrderParams): Observable<SearchResult<WorkOrder>> {
    const query = params ? toQueryString(params) : '';
    return this.apiService.get<SearchResult<WorkOrder>>(`/v1/work-orders${query}`);
  }

  /** Fetches the full detail for a single work order by id from GET /v1/work-orders/:id. */
  getWorkOrder(id: number): Observable<WorkOrderDetail> {
    return this.apiService.get<WorkOrderDetail>(`/v1/work-orders/${id}`);
  }

  /** Fetches aggregate manufacturing KPIs from GET /v1/manufacturing/kpis. */
  getKpis(): Observable<ManufacturingKpisDto> {
    return this.apiService.get<ManufacturingKpisDto>('/v1/manufacturing/kpis');
  }

  /** Fetches the manufacturing quality scorecard from GET /v1/manufacturing/quality-scorecard. */
  getQualityScorecard(): Observable<ManufacturingQualityScorecard> {
    return this.apiService.get<ManufacturingQualityScorecard>('/v1/manufacturing/quality-scorecard');
  }

  /** Fetches every scrap reason from GET /v1/scrap-reasons. */
  getScrapReasons(): Observable<ScrapReason[]> {
    return this.apiService.get<ScrapReason[]>('/v1/scrap-reasons');
  }

  /**
   * Searches products by name via POST /v1/products/search (a name LIKE match). The API exposes no
   * `GET /v1/products?search=`; `name` is capped at 50 characters server-side.
   */
  searchProducts(name: string): Observable<SearchResult<ProductLookup>> {
    return this.apiService.post<SearchResult<ProductLookup>>(
      `/v1/products/search?pageNumber=1&pageSize=${PRODUCT_SEARCH_PAGE_SIZE}`,
      { name },
    );
  }

  /** Fetches a single product's id and name from GET /v1/products/:id, to label a `productId` restored from the URL. */
  getProduct(id: number): Observable<ProductLookup> {
    return this.apiService.get<ProductLookup>(`/v1/products/${id}`);
  }
}
