# manufacturing-feature-work-orders

Production work-order list view for the AdventureWorks Angular app.

## Components

- **WorkOrderListComponent** (`/manufacturing/work-orders`) — server-side paginated, filterable work-order list with plain numeric filter inputs (`productId`, `scrapReasonId` — no lookup endpoint exists for either), a default `startDate` desc sort matching the API, and full URL-param sync via a reactive `route.queryParams` subscription. A per-row "View" button is present but has no detail route/component to navigate to yet — clicking it currently falls through to the app-wide `NotFoundComponent`. This is an accepted, tracked gap pending a future story (see `apps/angular-web/.claude/CLAUDE.md`'s Manufacturing Feature Libraries section). The Status column renders a "completed late" badge only when the work order finished after its due date, otherwise blank.

## Running unit tests

Run `nx test manufacturing-feature-work-orders` to execute the unit tests.
