# AdventureWorks Angular Web — Claude Code Configuration

Angular 21 Nx monorepo for the AdventureWorks capstone. Zoneless, signal-first, standalone components throughout.

For the tech stack, workspace structure, design system, authentication, and commands (including the library generator), read `apps/angular-web/README.md`. It is the single source of truth.

## Rules

Path-scoped rules live at the repo root in `.claude/rules/` and load automatically when a file matching their `paths:` globs is read or edited. This file holds only the cross-domain rules; anything specific to one domain's libraries is in that domain's rule file.

| File                        | Loads for                                | Covers                                                         |
| --------------------------- | ---------------------------------------- | -------------------------------------------------------------- |
| `angular-signals.md`        | `apps/angular-web/**/*.ts`               | Signal APIs, `inject()`, `computed()`, `effect()`, zoneless DI |
| `alpine-circuit-theming.md` | `apps/angular-web/**` css, html, ts      | DaisyUI semantic colors, Tailwind `ac-*` utilities, no raw hex |
| `nx-libraries.md`           | `apps/angular-web/**/*`                  | Valid tags, import rules, post-generation checklist            |
| `angular-sales.md`          | `apps/angular-web/libs/sales/**`         | Sales list, order filter, and detail invariants                |
| `angular-purchasing.md`     | `apps/angular-web/libs/purchasing/**`    | Vendor store scope, badge maps                                 |
| `angular-hr.md`             | `apps/angular-web/libs/hr/**`            | Department helpers, employee wizard and detail, org chart      |
| `angular-manufacturing.md`  | `apps/angular-web/libs/manufacturing/**` | Scorecard percentages, card hover                              |

## NgRx SignalStore Pattern

- List stores in `libs/{scope}/data-access/` use `withEntities` + `withRequestStatus` + `withPagination` + `rxMethod` for HTTP bridging. One-shot aggregates (`DashboardStore`, `HrDashboardStore`, `LookupStore`) use `withState` + `withRequestStatus`.
- Components read store signals directly. Never subscribe to store state.
- Put a store in `libs/{scope}/feature-*/` only when it has exactly one consumer, is not exported from the lib's barrel, and holds feature view state (`OrgChartStore`'s `expandedIds`, for example). Reusable domain data goes in `data-access`.
- Name every `*Params` field exactly as the target endpoint's query-string parameter, because `toQueryString` forwards keys verbatim. `GET /v1/vendors` binds `pageNumber`; `GET /v1/departments/:id/employees` binds `page`. Check the API's parameter model; there is no workspace-wide convention.

## List Component Invariants

- Declare a module-level `PAGE_SIZE` per list component and pass it to every store call. Never inline the literal.
- When making a column sortable, add it to both the `columns` config and `VALID_SORT_COLUMNS`. `DataTableComponent` emits sort events for any column key; `sortable: false` is a UI hint, not a contract.
- For server-paginated lists, drive every load from a `route.queryParams` subscription with `takeUntilDestroyed(destroyRef)`. Action methods write to the URL through `router.navigate([], { queryParamsHandling: 'merge' })` and null out cleared params so the merge strips them. Client-side lists load once and use the subscription only to update filter and sort signals.
- Never add a row-click output to the shared `DataTableComponent`; it only emits `sortChange` and `pageChange`, and every list consumes it. Use a per-row View button in an `awColumnDef` cell or a `routerLink` cell.
- Build metric-tile grids with `aw-card` from `shared/ui` (see `vendor-detail.html` and `sales-person-detail.html`), and wrap horizontally scrollable content in `aw-scroll-indicator`. Never hand-roll `card bg-base-100` tile markup or edge-fade gradients.
- Date validators live in `libs/shared/util/src/lib/validators/`, compare UTC calendar dates, and are exported from the `shared/util` barrel.

## SignalR

`SignalrService` lives in `libs/shared/util/src/lib/signalr/`. Both `AuthService.logout()` and the `initializeSignalrLifecycle` effect in `app.config.ts` disconnect SignalR. Keep both: logout must finish disconnecting before the MSAL redirect, and the lifecycle effect covers every other auth-state change. `connect()` never throws; callers read the `connectionStatus` signal.

## Testing

- Test framework is **Vitest**, not Jest.
- `npx nx test adventureworks-web` runs ONLY the app project; every lib under `libs/` has its own test target. Use `npx nx run-many -t test` (or `nx affected -t test`) to cover them, and check the test count, not just pass/fail.
- Mock stores, services, and third-party SDKs. Stub `ActivatedRoute` with `buildActivatedRoute()` and spy on `Router.navigate`; do not mock other Angular internals.
- **Never assert on the Chart.js constructor or `destroy()` in specs.** Chart components build their instance in `afterNextRender`, and Angular routes exceptions thrown there to the plain `ErrorHandler`, so a failing hook becomes a silent `vi.waitFor` timeout. Keep the `vi.mock('chart.js', …)` factory (module-scope `Chart.register(...)` needs it), but assert only on the canvas, its `role`/`aria-label`, the DOM legend, and component `computed()` state. Precedent: `6cf98cb`, `88ea17b`.
- Every Chart.js component destroys its chart in `ngOnDestroy` to release the canvas context. The rule above means this is not spec-tested; keep the teardown anyway.

Shared spec helpers (`provideMockEnvironment`, `provideMockAuthService`, `provideMockNotificationService`, `provideMockLoadingService`, `provideMockAppInsightsService`, `renderComponent()`, `buildActivatedRoute()`, `mockSearchResult()`) live in `libs/shared/util/src/lib/testing/`. Never export them from `shared/util`'s `index.ts`; test-only code would ship in the production barrel, so other libs keep local `render-*-component.ts` copies instead.

## E2E (Playwright) — `apps/adventureworks-web-e2e`

See `apps/adventureworks-web-e2e/README.md` for env vars and run commands.

- Use the Page Object selector in `src/page-objects/*.page.ts` wherever one exists. Page Objects are read-only wrappers over the stable `#aw-*` ids already in the templates; they never add ids to app markup.
- Auth is a dependency-based Playwright `setup` project (`src/support/global-setup.ts`), not the config-level `globalSetup` hook. It acquires a token through ROPC and injects the MSAL cache, then persists `storageState`. Because it is a project, `--project=chromium-unauthenticated` never triggers it, so `auth-boundary.spec.ts` runs without credentials. Missing env vars fail fast with a thrown error.
- `storage-state-path.ts` anchors on `workspaceRoot`, not `__dirname`. The Nx Playwright plugin evaluates `playwright.config.ts` where `__dirname` is undefined.

## Security

- Add every authenticated route as a child of the `MsalGuard` layout route in `app.routes.ts`, never as a top-level sibling. The guard is inherited, not repeated.
- Never use `innerHTML` or `bypassSecurityTrust*`. All store data renders through interpolation or property bindings.
- Clamp `pageNumber` from URL query params with `Math.max(1, Math.trunc(Number(param)) || 1)` before forwarding it to the API.
- Error toasts show hardcoded user-facing messages. Never forward raw `err.message` to the UI.

## AI Done Gate

Before finishing a UI task, verify:

- New or changed muted text uses `text-secondary` or another solid DaisyUI semantic color, never a Tailwind opacity modifier (`text-*/NN`). The `@nx/workspace-no-text-color-opacity` lint rule enforces this for static `class` attributes only; `[ngClass]` and `[class.x]` bindings bypass it, so check those by hand.
- Each new authenticated route is added to `apps/adventureworks-web-e2e/src/accessibility.spec.ts` so the axe-core WCAG AA check covers it.
