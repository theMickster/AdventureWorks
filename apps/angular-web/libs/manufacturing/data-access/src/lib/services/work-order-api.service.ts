import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import { toQueryString } from '@adventureworks-web/shared/data-access';
import type { WorkOrder } from '../models/work-order.model';
import type { WorkOrderDetail } from '../models/work-order-detail.model';
import type { WorkOrderParams } from '../models/work-order-params.model';

/** HTTP client for Manufacturing domain endpoints (Work Orders). */
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
}
