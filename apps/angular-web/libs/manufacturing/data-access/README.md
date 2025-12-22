# manufacturing-data-access

`WorkOrderApiService` and `WorkOrderStore` for the Production.WorkOrder domain. `WorkOrderStore` is an NgRx SignalStore (`withEntities` + `withRequestStatus` + `withPagination`) whose `loadPage` rxMethod bridges to `GET /v1/work-orders`; `applyFilters` is a thin wrapper that resets to page 1 and delegates to `loadPage`.

## Running unit tests

Run `nx test manufacturing-data-access` to execute the unit tests.
