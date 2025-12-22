# manufacturing-feature-work-orders

Production work-order list view for the AdventureWorks Angular app.

## Components

- **WorkOrderListComponent** (`/manufacturing/work-orders`) — server-side paginated, filterable work-order list with plain numeric filter inputs (`productId`, `scrapReasonId` — no lookup endpoint exists for either), a default `startDate` desc sort matching the API, and full URL-param sync via a reactive `route.queryParams` subscription. A per-row "View" button navigates to `WorkOrderDetailComponent` at `/manufacturing/work-orders/:id`. The Status column renders a "completed late" badge only when the work order finished after its due date, otherwise blank.
- **WorkOrderDetailComponent** (`/manufacturing/work-orders/:id`) — read-only detail view. Calls `WorkOrderApiService.getWorkOrder(id)` directly; no NgRx store, mirroring `OrderDetailComponent`'s direct-service pattern. Completed-late is rendered as a fixed `status="completed-late"` badge (danger variant) plus a separate sibling `<span>` for the day count — the day count is never interpolated into the badge's status string since that value is piped through `|translate`. The product name links to `/products/:productId`, a route that does not exist yet in this app — an accepted, tracked gap (see `apps/angular-web/.claude/CLAUDE.md`).

## Running unit tests

Run `nx test manufacturing-feature-work-orders` to execute the unit tests.
