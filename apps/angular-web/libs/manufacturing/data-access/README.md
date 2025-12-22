# manufacturing-data-access

`WorkOrderApiService` and `WorkOrderStore` for the Production.WorkOrder domain. `WorkOrderStore` is an NgRx SignalStore (`withEntities` + `withRequestStatus` + `withPagination`) whose `loadPage` rxMethod bridges to `GET /v1/work-orders`; `applyFilters` is a thin wrapper that resets to page 1 and delegates to `loadPage`.

`WorkOrderApiService.getWorkOrder(id)` fetches the full detail for a single work order from `GET /v1/work-orders/:id`, returning a `WorkOrderDetail` (adds `daysLate`, `scrapReasonId`, `scrapReasonName` on top of the list row's `WorkOrder` fields). Called directly by `WorkOrderDetailComponent` — not routed through the store, since the detail view has no entity-cache use case.

## Running unit tests

Run `nx test manufacturing-data-access` to execute the unit tests.
