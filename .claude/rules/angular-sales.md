---
paths:
  - "apps/angular-web/libs/sales/**"
---

# Sales Library Rules

Rules the specs and code comments in `libs/sales/**` do not already enforce. Cross-domain list, store, and testing rules are in `apps/angular-web/.claude/CLAUDE.md`.

- `SalesPersonListComponent` loads one page once in `ngOnInit` and filters client-side; its `route.queryParams` subscription only updates filter and sort signals. Keep its `PAGE_SIZE` above the sales-person count, or rows past the first page disappear silently.
- Keep `OrderListComponent` server-paginated through `SalesOrderStore`. The dataset is unbounded.
- Adding an order-list filter touches seven places together: `SalesOrderFilters`, `filterForm`, `restoreFiltersFromUrl`, `parseFilterParams`, `filterUrlParams`, `onResetFilters`, and `extractOrderListNavParams`. The filter hash is `JSON.stringify(parseFilterParams())`, so a param missing from the parser never triggers an analytics refresh.
- In `OrderDetailComponent`, map a 404 or a null body to `notFound` and every other error to `hasError`.
- In `territory-breakdown.html`, keep the conditional hover and cursor classes bound through `[ngClass]`, not `[class.x]`.
