# feature-vendors

Vendor list view for the AdventureWorks Angular app.

## Components

- **VendorListComponent** (`/purchasing/vendors`) — server-side paginated, filterable vendor list with three filter fields (credit rating, preferred-vendor toggle, active-flag toggle). The server always sorts by total spend descending — there is no client-facing sort column. URL state uses a reactive `route.queryParams` subscription (mirroring `OrderListComponent`) so browser back/forward navigation re-fires the load while the component stays mounted. There is no row-click navigation — `purchasing.routes.ts` has no `:id` route yet.

## Running unit tests

Run `nx test purchasing-feature-vendors` to execute the unit tests.
