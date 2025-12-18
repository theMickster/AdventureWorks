# AdventureWorks Angular Web — Claude Code Configuration

Angular 21 Nx monorepo for the AdventureWorks capstone. Zoneless, signal-first, standalone components throughout.

## Tech Stack

For the current tech stack, workspace structure, design system, authentication, and commands, read the workspace README — it is the single source of truth:

> Use the `Read` tool on `apps/angular-web/README.md`.

## Rules and Guides

Additional guardrails live in `.claude/rules/` and are auto-loaded by Claude:

| File                        | Covers                                                         |
| --------------------------- | -------------------------------------------------------------- |
| `angular-signals.md`        | Signal APIs, `inject()`, `computed()`, `effect()`, zoneless DI |
| `alpine-circuit-theming.md` | DaisyUI semantic colors, Tailwind `ac-*` utilities, no raw hex |
| `nx-libraries.md`           | Valid tags, import rules, post-generation checklist            |

## Library Tags

Every library must have exactly two tags in `project.json`:

- **type**: `type:feature` | `type:ui` | `type:data-access` | `type:util`
- **scope**: `scope:shared` | `scope:sales` | `scope:hr` | `scope:public`

## NgRx SignalStore Pattern

Domain-data stores live in `libs/{scope}/data-access/`. They use `withRequestStatus()` + `withPagination()` + `rxMethod()` for HTTP bridging. Components inject the store and read signals directly — no subscriptions.

A store may instead live feature-local in `libs/{scope}/feature-*/` when it has exactly one consumer, is not exported from the lib's public barrel, and holds feature-specific view state (e.g. `OrgChartStore`'s `expandedIds`/`highlightedId` and baked-in DaisyUI color classes) rather than reusable domain data.

## Sales Feature Libraries

### List Component Invariants

All list components share these patterns. Individual sections only document deviations.

- **`PAGE_SIZE` constant**: Module-level `const PAGE_SIZE = 25` is the single source of truth across all store calls. Do not inline the literal.
- **`VALID_SORT_COLUMNS` allowlist**: `onSortChange` guards with `(VALID_SORT_COLUMNS as readonly string[]).includes(event.column)`. `DataTableComponent` emits sort events for any column key — `sortable: false` is a UI hint only, not a contract. Any new sortable column must be added to both `columns` config and `VALID_SORT_COLUMNS`.
- **URL-param sync (reactive)**: All list components subscribe to `route.queryParams` with `takeUntilDestroyed(destroyRef)` in `ngOnInit`. Every URL change — including in-place browser back/forward — re-fires the subscription. For server-side components (StoreListComponent), the subscription is the sole driver of all store calls; action methods only write to the URL via `router.navigate([], { queryParamsHandling: 'merge' })`. For client-side components (SalesPersonListComponent), the subscription updates filter/sort signals; data loads once separately. Cleared params are nulled so the merge strips them.
- **Sort state coherence**: `sortColumn` and `sortDirection` signals must be reset to `''` / `'asc'` whenever `onSearch` or `onClearSearch` fires. `onPageChange` must include current sort signals in every store call.
- **`rows` computed**: Maps store entities to flat `Record<string, unknown>[]`. `DataTableComponent` requires this shape — nested navigation properties must be projected to scalar strings.

### `libs/sales/feature-stores`

**StoreListComponent** — server-side paginated list. Follows List Component Invariants exactly.

### `libs/sales/feature-sales-persons`

**SalesPersonListComponent** — client-side search and sort at `/sales/persons`.

Deviations from List Component Invariants:

- **Client-side filter/sort**: All 17 records load once. Do NOT add server-side pagination — the dataset is intentionally small and bounded.
- **`onInputChange` keyboard clear**: `(input)` events route through `onInputChange()`, not directly to `searchTerm.set()`. When cleared by keyboard, `onInputChange('')` delegates to `onClearSearch()` to keep URL state clean.

### SalesPersonDetailComponent

Direct `SalesApiService` call; no SignalStore. `backQueryParams` snapshot read is intentional — do not refactor to reactive. Performance tab lazy-loads on first activation; guard `!performance() && !isLoadingPerformance()` — do not add `|| hasPerformanceError()`. Null `salesQuota` suppresses progress bar — do not add `?? 0`.

- **"View Orders" link**: Inside `@else if (person(); as p)` block — use `p.id` (the template alias), not `person()!.id`. Absent during loading and on error.

### SalesPersonCreateComponent

Direct `SalesApiService` call; no SignalStore.

- **Route ordering**: `persons/new` must appear before `persons/:id` in `sales.routes.ts`. Angular matches routes top-to-bottom; the literal `"new"` would be captured as `:id` otherwise.
- **`stateProvinceId` → `stateProvince` lookup in `onSubmit`**: `SelectFieldComponent` emits strings; `onSubmit` parses to number and resolves the full `StateProvince` object from the loaded signal. Submission is blocked if the lookup misses.
- **Four reference data signals via `forkJoin` in `ngOnInit`**: territories, address types, state/provinces, and phone number types load in a single parallel request. `isLoading` stays true until all four resolve.
- **Tab error badge pattern**: `*HasErrors` computed signals become true only after `submitted() === true` — `_formStatus` is bridged via `toSignal()` so they re-evaluate on every validation state change.

### SalesPersonEditComponent

Sales config edit form at `/sales/persons/:id/edit`. Direct `SalesApiService` PATCH call; no SignalStore. `isLoading`/`isSaving` are separate signals — do not merge. `backQueryParams` snapshot read is intentional. Route `persons/:id/edit` must follow `persons/:id` in `sales.routes.ts`.

### `libs/sales/feature-orders`

**OrderListComponent** — server-side paginated, filterable list at `/sales/orders`.

- **Server-side pagination**: Dataset is large and unbounded. Do NOT switch to client-side.
- **Default sort `orderDate` desc**: `onResetFilters` clears all filters and sort and returns to exactly this baseline.
- **`statusKey` vs `status` binding gotcha**: The `rows` computed projects both `status` (cased `statusDescription`, for display) and `statusKey` (lowercased). `STATUS_BADGE_MAP` keys off `statusKey` — bind the badge variant to `statusKey`, NOT `status`. Binding `status` silently falls through to `badge-outline`.
- **Status dropdown is a hardcoded const**: Driven by `SALES_ORDER_STATUSES` in `sales-order.model.ts` — no lookup endpoint. Labels mirror the server's `StatusDescription` strings (note "In process", lowercase 'p').
- **Dropdown data via `forkJoin`, independent of the grid**: A slow or failed lookup must not block or break the grid — shows a "filtering may be unavailable" toast only.
- **Clickable rows route to detail**: `onRowClick` navigates to `/sales/orders/:id`.

#### Filter-hash pattern

`_lastFilterHash` serialises the current filter values as a JSON string. In `loadFromUrl`, the new hash is compared to the stored hash before deciding which store method to call:

- **Filters changed** → `store.applyFilters(params)` dispatches both `loadPage` and `loadAnalytics`.
- **Page/sort only** → `store.loadPage(params)` skips the analytics re-fetch.

`_lastFilterHash` must be updated only in the `filtersChanged` branch, and the hash must include all five filter fields (`orderDateFrom`, `orderDateTo`, `status`, `salesPersonId`, `territoryId`). Adding a new filter param requires updating `parseFilterParams` so the hash detects the change.

### OrderDetailComponent

Read-only at `/sales/orders/:id`. Calls `SalesApiService.getSalesOrder()` directly; no NgRx store. Mirrors `StoreDetailComponent` pattern.

- `backQueryParams` calls `extractOrderListNavParams` from `'../order-list-nav-params'` (feature-orders-local; handles the 8 order-list filter params). Snapshot read is intentional — do not refactor to `toSignal(route.queryParams)`.
- `STATUS_BADGE_MAP` lives in `order-status-badge.ts` (shared with `OrderListComponent`) — do not duplicate
- 404 response sets `notFound` signal; non-404 errors set `hasError` signal
- `BillToAddress` / `ShipToAddress` are always present — NOT NULL in schema

### OrderAnalyticsPanelComponent

Lives at `libs/sales/feature-orders/src/lib/order-analytics-panel/`. Exported from `index.ts`.

Inputs: `analytics: SalesOrderAnalytics | null`, `status: 'idle' | 'loading' | 'loaded' | 'error'`.

Four template states: loading/idle (skeleton), error, empty (zero orders or non-finite revenue), data (KPI tiles + trend chart).

**`OrderAnalyticsTrendChartComponent`** — internal Chart.js line chart. Not exported from `index.ts`.

- **Reactive**: an `effect()` watching `monthlyTrend()` destroys and recreates the Chart.js instance on every input change. `afterNextRender` sets `isRendered = true` to guard against the effect firing before the canvas is in the DOM.
- **`isPartialMonth`**: points render at reduced opacity (`rgba(8,145,178,0.4)`) and append "(Partial month)" to the tooltip label.
- **`ngOnDestroy`**: always destroys the Chart.js instance to release the canvas context.

### `libs/sales/feature-dashboard`

**DashboardComponent** — three KPI cards plus `TrendChartComponent`, `TopPerformersComponent`, and `TerritoryBreakdownComponent` wired via `DashboardStore` signals.

- **No URL-param sync** — single-page KPI display; no list state to preserve.
- **`DashboardStore`** is root-scoped (`providedIn: 'root'`, `withState`, not `withEntities`) — loaded once in `ngOnInit`; no manual refresh trigger.
- **Error toast via constructor `effect()`** — placement is load-bearing: Angular evaluates the effect after `ngOnInit`'s synchronous `setLoading()` clears any stale error state, preventing a spurious toast on revisit. Do not move the effect to `ngOnInit`.
- **`switchMap` not `exhaustMap`** in `DashboardStore.load` — cancels a stale in-flight request if `load()` is re-triggered before the response arrives.
- **`TrendChartComponent`**: Chart.js instance created in `afterNextRender` (zoneless-safe) and destroyed in `ngOnDestroy`. One-shot render — data input is not reactive after mount.
- **`TrendChartComponent.dataPointClick`**: `output<{ year: number; month: number }>()` emitted from the Chart.js `onClick` handler. Bound as `(dataPointClick)="onTrendChartClick($event)"`.
- **`DashboardComponent.onTrendChartClick`**: Navigates to `/sales/orders?orderDateFrom=YYYY-MM-01&orderDateTo=YYYY-MM-DD`. Uses `new Date(year, month, 0).getDate()` to find the last day — the "day 0 of next month" trick handles variable month lengths and leap years.
- **`TopPerformersComponent`**: `maxRevenue` is `performers()[0]?.revenue` — API guarantees descending sort.
- **`TerritoryBreakdownComponent`**: `grouped` computed builds a Map keyed by `group`, sorts group names alphabetically, preserves API's revenue-descending order within each group. `onTerritoryClick` navigates to `/sales/orders?territoryId=N`.
- **Zero-order territory guard**: `@let drillable = t.orderCount > 0` gates all interactive attributes on a single `<tr>`. Uses `NgClass` (not `[class.xxx]`) to avoid Tailwind scanner issues with `hover:bg-base-200`.
- **`TrendChartComponent` `onHover` cursor**: Chart.js `onHover` sets `canvas.style.cursor` to `pointer` on a data point, `default` otherwise.
- **All three widgets are internal** — not exported from `index.ts`.

### StoreDetailComponent

Tabbed detail at `/sales/stores/:id`. Calls `SalesApiService` directly; no NgRx SignalStore. The `backQueryParams` computed signal reads `snapshot.queryParams` — this is intentional; it captures list-view state at navigation time and must not be refactored into a reactive signal read.

### StoreCreateComponent and StoreEditComponent

**StoreCreateComponent** — form at `stores/new`. **StoreEditComponent** — form at `stores/:id/edit`. Both inject `SalesApiService` directly.

- **Route ordering**: `stores/new` must appear before `stores/:id` in `sales.routes.ts`. The literal `"new"` would be captured as `:id` otherwise.
- **`isSaving` separate from `isLoading`**: `StoreEditComponent` has two distinct signals — `isLoading` (true while the initial `forkJoin` loads store + sales persons) and `isSaving` (true while the save call is in flight). Do not merge them.
- **Navigate on `next`**: After successful create or update, navigates to `/sales/stores/:id`. `isSaving`/`isLoading` is reset to false in the `error` handler so the user can retry.
- **`extractListNavParams` utility**: Both `StoreDetailComponent` and `StoreEditComponent` use `extractListNavParams(this.route.snapshot.queryParams)` inside a `computed()`. The function is exported from `@adventureworks-web/shared/util` (not a local file). The snapshot read is intentional — do not refactor to `toSignal(route.queryParams)`.

## HR Feature Libraries

### `libs/hr/feature-departments`

**DepartmentListComponent**, **DepartmentDetailComponent**, **DepartmentCreateComponent**, **DepartmentEditComponent** — Department CRUD at `/hr/departments`. All four call `HrApiService` directly; no NgRx SignalStore (department count is small and bounded, mirroring `SalesPersonListComponent`'s client-side justification).

- **Route ordering**: `departments/new` must appear before `departments/:id` in `hr.routes.ts`. Angular matches routes top-to-bottom; the literal `"new"` would otherwise be captured as `:id`.
- **Duplicate-name handling branches on `errorCode`, not `propertyName`**: the API rejects a duplicate name with a 400 FluentValidation error. `DepartmentCreateComponent` matches `errorCode: 'Rule-05'`; `DepartmentEditComponent` matches `errorCode: 'Rule-06'`. The update-path rule is whole-model, not per-field, so its `propertyName` is empty — a lookup keyed on `propertyName` would never match it. Both components set the server error on the `name` control regardless.
- **`isSaving`/`isLoading` separation in `DepartmentEditComponent`**: `isLoading` covers the initial `forkJoin` (department + department list, for group-name options); `isSaving` covers the update call. Do not merge them.
- **Group-name dropdown is runtime-derived, not hardcoded**: all three form/list components (`DepartmentListComponent`'s filter, `DepartmentCreateComponent`, `DepartmentEditComponent`) source their group-name options from the shared `extractGroupNames()` helper in `extract-group-names.ts` — distinct `groupName` values from the loaded department list, deduped and sorted alphabetically. Do not re-implement the dedupe/sort logic locally; import the helper instead.
- **`renderDepartmentComponent` testing helper** (`testing/render-department-component.ts`): a lib-local duplicate of `shared/util`'s `renderComponent()`. Intentional — `shared/util`'s helper is test-only and excluded from its public barrel, so importing it here would violate `@nx/enforce-module-boundaries`. Only wires `provideRouter([])` since no department component injects `AuthService`, `LoadingService`, or `AppInsightsService`.

### `libs/hr/feature-employees`

**EmployeeCreateComponent** — guided, sequential 3-step wizard (Personal Info → Contact → Address) plus a Review step at `/hr/employees/new`. Direct `HrApiService.createEmployee()` call; no NgRx SignalStore, consistent with `DepartmentCreateComponent`/`SalesPersonCreateComponent`.

- **Route ordering**: `employees/new` must appear before `employees/:id` in `hr.routes.ts`. Angular matches routes top-to-bottom; the literal `"new"` would otherwise be captured as `:id`.
- **Guided wizard, not free-clickable tabs**: unlike `SalesPersonCreateComponent`'s always-clickable tab bar, `currentStep` (1-4) only advances via `onNext()`, which calls `markAllAsTouched()` on the current step's `FormGroup` and blocks advancing while it is invalid. `onBack()` and the review step's "Edit" links are never gated — the whole wizard is one `FormGroup` tree, so moving between steps never loses entered data. Step 4 (Review) owns no `FormGroup` of its own; it reads the other three groups' `.value` directly.
- **Three non-null guards in `onSubmit`**: `EmployeeCreate.phone.phoneNumberTypeId` and `EmployeeCreate.addressTypeId` are non-nullable `number` (unlike `SalesPersonCreate`, where the equivalents are `number | null`) — `onSubmit` resolves the selected `phoneNumberTypeId` and `addressTypeId` against their loaded lookup signals before building the payload, in addition to the existing `stateProvinceId` → `stateProvince` object resolution. All three block submission with a distinct error toast when the selected id has no match in the loaded lookup list.
- **No `hireDate` field**: `EmployeeCreateModel` has no `HireDate` property — the server hardcodes it on creation. Do not add a hire date field to this wizard even though older Postman requests for this endpoint include one (silently ignored server-side). `salariedFlag` is hardcoded `false` and `organizationLevel` to `null` — neither is part of this story's AC.
- **`ApiValidationError` step-jump behavior**: `API_PROPERTY_TO_FORM_PATH` maps each server `propertyName` to both a nested form control path and the wizard step that owns it. On a validation error, every mapped property gets `setErrors({ server })` + `markAsTouched()`, and `currentStep` jumps to the step of the _first_ mapped property — necessary because, unlike tabs, wizard steps aren't all visible at once.
- **Reference data via `forkJoin` in `ngOnInit`**: address types, state/provinces, and phone number types (three lookups, not `SalesPersonCreateComponent`'s four — no territory concept for employees). `isLoading` stays true until all three resolve.
- **`_touchTick` signal forces error re-evaluation on bare `markAsTouched()` calls**: `markAllAsTouched()`/`markAsTouched()` mutate `AbstractControl.touched` without emitting through `valueChanges`, and a plain `ctrl.touched` property read inside a `computed()` does not register as a tracked dependency — so the `*Errors` computed signals would otherwise keep returning their stale (pre-touch) cached value after `onNext()` blocks or an `ApiValidationError` sets a server error. `_touchTick` is bumped at every touch-marking call site and read inside `getErrors()` to force recomputation. `onSubmit()`'s form-wide `markAllAsTouched()` doesn't need this — it's already covered because `submitted.set(true)`, a real signal write, runs first.

**EmployeeDetailComponent** — read/edit detail at `/hr/employees/:id`. Direct `HrApiService` calls; no NgRx SignalStore.

- **In-place edit toggle (US-757), not a routed `/edit` page**: unlike every other entity's edit flow (`DepartmentEditComponent`, `SalesPersonEditComponent`, `StoreEditComponent`), the Personal Info card itself flips between read-only and editable via an `isEditing` signal and `@if`/`@else` in the template — a deliberate deviation, first of its kind in this workspace.
- **Editable fields are narrow**: First/Middle/Last Name, Job Title, Marital Status, and Gender only. Birth Date and Hire Date are immutable and always render as plain read-only text, even inside the edit form.
- **Cancel has no explicit revert logic**: the read-only view always renders from the `employee()` signal, which is never mutated while editing. `onCancelClick()` just flips `isEditing` back to `false`; re-entering edit mode via `onEditClick()` rebuilds `personalInfoForm` fresh from `employee()` every time, so there is nothing to roll back.
- **`isSaving`/`isLoading` are separate signals**, matching the `StoreEditComponent`/`DepartmentEditComponent` convention — do not merge them.
- **Save updates `employee()` directly from the PUT response** — no follow-up GET.
- **`SERVER_ERROR_FIELD_MAP` keys are PascalCase**, matching `EmployeeCreateComponent`'s `API_PROPERTY_TO_FORM_PATH`: `ExceptionHandlerMiddleware`'s `CamelCaseOptions` only renames the JSON key (`PropertyName` -> `propertyName`) — the string value inside it is FluentValidation's raw C# property name (e.g. `"JobTitle"`) and stays PascalCase. A camelCase lookup key here silently never matches.
- **Lifecycle wizards (US-758)**: Hire/Terminate/Rehire are modal wizards — `EmployeeHireModalComponent`, `EmployeeTerminateModalComponent`, `EmployeeRehireModalComponent`, colocated under `employee-detail/` and NOT exported from `feature-employees`'s `index.ts` (internal, single-consumer components, same convention as the Sales Dashboard's internal widgets). They are the first feature-level consumers of the shared `aw-modal` (`ModalComponent`).
  - **`EmployeeStore` injection is a narrow exception**: unlike the rest of `EmployeeDetailComponent` and its lifecycle modals (which otherwise follow the "no NgRx store" HR convention), the three modals each inject `EmployeeStore` directly and call its `hireEmployee`/`terminateEmployee`/`rehireEmployee` rxMethods so the resulting entity flows through the store's cache. `EmployeeDetailComponent` itself is untouched by this — it still reads via `HrApiService` and saves Personal Info via `HrApiService.updateEmployee`.
  - **Completion detection via shared store signals, not a return value**: `hireEmployee`/`terminateEmployee`/`rehireEmployee` are fire-and-forget `rxMethod`s (see `employee.store.ts`) — each modal's constructor `effect()` watches the store's shared `isLoading()`/`hasError()` signals to detect success/failure. This is only safe because each modal is the sole active consumer of those signals at a time (a user can only have one lifecycle wizard open at once). If a second concurrent consumer of `EmployeeStore`'s request-status signals is ever introduced, this must move to a per-request result instead.
  - **Hire button only renders when `lifecycle().employmentStatus === 'OnLeave'`** — not for `Terminated` (that's Rehire's job) or `Active`.
  - **Hardcoded lookup constants**: `SHIFT_OPTIONS`, `PAY_FREQUENCY_OPTIONS`, `TERMINATION_TYPE_OPTIONS` live in `hr/data-access`'s `employee-lifecycle-options.model.ts` (exported from its barrel) because no backend lookup endpoint exists for these — they mirror `HumanResourcesConstants.cs` byte/const values on the API.
  - **`maxFutureDateValidator(days)`/`minDateValidator(minDate)`** live alongside `minAgeValidator` in `shared/util`'s `validators/` folder, following the same UTC-calendar-date comparison pattern.

### `libs/hr/feature-org-chart`

**OrgChartComponent** — interactive org chart at `/hr/org-chart`: CEO-down tree with expand/collapse, department color-coding, search/click-through, and summary stats. First recursive-component and first `libs/shared/ui` scroll-indicator pattern in this workspace.

- **`OrgChartStore` is `withState`, not `withEntities`**: one-shot ~290-row payload with no per-entity CRUD, mirroring `DashboardStore` rather than `EmployeeStore`. `load()` runs a `forkJoin` of `getOrgTree()` + `getDepartments()` so the department→color map is available before the tree is built.
- **Department color source is `GroupName` (6 values), not `DepartmentName` (17 values)**: `department-group-color-map.ts` maps the 6 `HumanResources.Department.GroupName` values to DaisyUI semantic classes (`primary`/`accent`/`info`/`warning`/`secondary`/`success`), with a `neutral` fallback for an unmapped or blank department name. The store resolves each employee's `departmentName` to a color once at load time and bakes it into the tree node (`colorClass`) — `OrgNodeComponent` never does a lookup itself.
- **`build-org-tree.ts` reparents orphans under the true root — a real AdventureWorks data quirk, not a hypothetical**: the CEO's `HumanResources.Employee.OrganizationNode` is SQL `NULL` (not the hierarchyid root value), so the backend's `OrganizationNode.GetAncestor(1)` self-join misses for every level-1 VP — they come back from `/employees/org-tree` with `parentEmployeeId === null` even though they aren't the root. The true root is identified by `organizationLevel === null` (unique to the CEO); every other row with a null `parentEmployeeId` — VPs hit by this quirk, plus any employee created through the app that hasn't been assigned a manager yet — is attached directly under that root instead of becoming a second tree or being dropped. `OrgChartStore.searchAndExpand`'s ancestor walk defaults a null `parentEmployeeId` link to the same resolved root for exactly this reason, so the expansion path matches what's actually rendered. **`compute-org-stats.ts` mirrors this same root-resolution and reparenting** — without it, `managerCount`/`averageSpanOfControl` silently disagree with the tree: the root itself never appears as anyone's raw `parentEmployeeId` (verified against the live DB — zero rows), so a naive "distinct non-null `parentEmployeeId`" count undercounts managers by at least 1 and excludes every orphan's contribution to span of control.
- **`expandedIds: Set<number>` lives on the store, not per-node local signals**: this is what lets `searchAndExpand` auto-expand an arbitrary ancestor chain (e.g. searching "Stephen Jiang" expands `{273, 1}`) without `OrgNodeComponent` knowing anything about search. `toggleExpanded`/`searchAndExpand` always write a _new_ `Set` reference — never mutate in place — so signal equality checks pick up the change.
- **`OrgNodeComponent` is dumb and store-free**: `input.required<OrgChartTreeNode>()`, `input.required<ReadonlySet<number>>()` for `expandedIds`, `input<number | null>()` for `highlightedId`, and two `output<number>()`s. It self-imports its own class (`imports: [OrgNodeComponent]` inside its own `@Component` decorator) — the standard Angular pattern for a standalone recursive component, first use of it in this workspace.
- **Output is named `toggleExpand`, not `toggle`**: `@angular-eslint/no-output-native` forbids `toggle` because `<details>` fires a real native `toggle` event.
- **Expand/collapse is CSS-only**: a `grid-template-rows: 0fr` → `1fr` transition (~200ms) on the children wrapper, driven by a class binding on the expanded state. Children stay mounted in the DOM even when collapsed — no `@if` around the collapsed content, only around whether the node has any children at all.
- **Tree is built once, not per render**: `OrgChartStore.tree` and `.stats` are `withComputed` signals derived from the flat `items` state — they only recompute when `items()`/`departmentColorClasses()` actually change (i.e. once per `load()`), not on every template re-render.
- **`ScrollIndicatorComponent`** (`libs/shared/ui`) — new reusable gradient-fade edge affordance for horizontally-scrollable containers, used to wrap the tree. Not org-chart-specific; any future wide/scrollable content can reuse it.

## SignalR

`SignalrService` lives in `libs/shared/util/src/lib/signalr/`.

- **Hub URL** is configured via `environment.signalr.hubUrl`. A `__`-prefixed value means the deployment pipeline has not yet substituted the real URL — the service throws immediately to prevent tokens from being sent to a wrong endpoint.
- **`connect()` never throws** — errors are tracked to App Insights and the caller observes state via the `connectionStatus` signal.
- **SignalR disconnect on logout is NOT triggered by `AuthService`** — the `initializeSignalrLifecycle` effect in `app.config.ts` owns the connection lifetime and calls `disconnect()` when `isAuthenticated` becomes false.
- **`connectionStatus` signal** type: `'connecting'` | `'connected'` | `'reconnecting'` | `'disconnected'`. Automatic retry backoff: `[0, 2s, 5s, 10s, 30s]`.
- **`manualReconnect()`** — call when `connectionStatus === 'disconnected'` (retries exhausted) to start a fresh connection.
- **`on<T>()`** — returns a `() => void` teardown. Pass `destroyRef` to auto-wire teardown to component destruction; call the returned function manually otherwise. The connection is captured at registration time, so the teardown stays safe after `disconnect()` nulls the connection reference.
- **Do NOT invoke `SubscribeToDashboard` from the client** — `DashboardHub.OnConnectedAsync` handles group subscription server-side on every new `ConnectionId`.

## CLI

Use `--configuration` flags explicitly; do not rely on gitignored `environment.development.ts` in CI commands.

## Testing

- Test framework: **Vitest** (not Jest)
- Mock only at library boundaries (store, service) — do not mock Angular internals

### Testing Utilities

`libs/shared/util/src/lib/testing/` holds shared spec helpers: `provideMockEnvironment`, `provideMockAuthService`, `provideMockNotificationService`, `provideMockLoadingService`, `provideMockAppInsightsService`, `renderComponent()`, `buildActivatedRoute()`, and `mockSearchResult()`.

- **Not part of the public `@adventureworks-web/shared/util` barrel** — deliberately excluded from `index.ts` since they're test-only. Only reachable today via relative import from within `libs/shared/util` itself.
- A future secondary Nx entry point may expose these to other libraries' specs if/when needed — not built yet.

### E2E (Playwright) — `apps/adventureworks-web-e2e`

See `apps/adventureworks-web-e2e/README.md` for env vars and run commands.

- **Page Objects, not raw selectors in specs**: `src/page-objects/*.page.ts` wrap existing app markup IDs (`#aw-shell`, `#aw-public-login-btn`, `#aw-login-failed`, etc.) — specs never hardcode a selector that has a Page Object. Page Objects are read-only wrappers; they never add IDs to app markup.
- **Selector convention they wrap**: every interactive/structural element the E2E suite touches already has a stable `#aw-*` id in the component template (`app-layout.html`, `public-nav.html`, `login-failed.ts`) — this convention predates the E2E suite and is not something E2E changes.
- **Auth fixture (`src/support/global-setup.ts`)**: a dependency-based Playwright `setup` project (not the config-level `globalSetup` hook) drives a real Entra ID login and persists `storageState` to `playwright/.auth/user.json` (gitignored). Using a setup _project_ — rather than a global hook — means `--project=chromium-unauthenticated` never triggers it, so `auth-boundary.spec.ts` runs without `E2E_TEST_USERNAME`/`E2E_TEST_PASSWORD`.
- **`storage-state-path.ts` anchors on `workspaceRoot`, not `__dirname`**: the Nx Playwright plugin statically evaluates `playwright.config.ts` and its imports in a context where `__dirname` is undefined — using it crashes `nx show project`/`nx lint` with an opaque `Cannot read properties of undefined (reading 'join')`.
- **`E2E_TEST_USERNAME`/`E2E_TEST_PASSWORD` are not provisioned** — a real Entra test account is a known, accepted gap (Epic 565, #671). The setup project fails fast with a clear thrown error rather than silently skipping.

## Security

- MSAL guard (`canActivate: [MsalGuard]`) is inherited by all lazy child routes under the authenticated root — verify the route tree when adding new lazy routes
- Never use `innerHTML` or `bypassSecurityTrust*` — all store data must render through Angular interpolation or property bindings
- `pageNumber` from URL query params must be clamped with `Math.max(1, Math.trunc(Number(param)) || 1)` before forwarding to the API
- Error toasts must show hardcoded user-facing messages — never forward raw `err.message` to the UI

## AI Done Gate

Before finishing a UI task, verify:

- New or changed muted/secondary text uses `text-secondary` (or another solid DaisyUI semantic
  color) — never a Tailwind opacity modifier (`text-*/NN`) on any text-color utility. Enforced by
  `@nx/workspace-no-text-color-opacity` (`nx lint`); see `.claude/rules/alpine-circuit-theming.md`.
- New authenticated routes are covered by, or don't regress, the `accessibility.spec.ts`
  axe-core WCAG AA checks.
