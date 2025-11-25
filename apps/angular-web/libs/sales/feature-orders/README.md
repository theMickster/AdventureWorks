# feature-orders

Sales order list and detail views for the AdventureWorks Angular app.

## Components

- **OrderListComponent** (`/sales/orders`) — server-side paginated, filterable order list with five filter fields (date range, status, sales person, territory), sortable columns, and full URL-param sync. URL state uses a reactive `route.queryParams` subscription (US-738) so browser back/forward navigation re-fires the load while the component stays mounted. All action methods only write to the URL; the subscription drives data loading.
- **OrderDetailComponent** (`/sales/orders/:id`) — read-only order detail. Direct `SalesApiService` call; no NgRx store.

## Running unit tests

Run `nx test sales-feature-orders` to execute the unit tests.
