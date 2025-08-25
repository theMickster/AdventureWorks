# AdventureWorks Enterprise Application - Implementation Plan

**Project Vision**: Build a modern, production-ready AdventureWorks showcase demonstrating Angular v21, .NET 10.0 API with Clean Architecture, Azure Functions microservices, and cloud-native practices - achievable by solo developer + Claude Code.

**Scope Philosophy**: Focus on **one domain end-to-end** (Sales) with impressive Azure Functions scenarios, rather than broad but shallow coverage. Quality over quantity. Showcase best-in-class architecture that can be extended later.

**Current Status Summary** (as of 2026-04-27):

- ✅ Backend API Round 1: Epic #552 closed (Done) — Store Manager, Products Manager, ProductReview, CQRS foundation
- 🔄 Backend API Round 2: Epic #873 (API Completion) created — 13 net-new Features + 3 reparented enabler Features (#715, #716, #722); 36 net-new Stories + 10 reparented Stories under Initiative #559
- ✅ Frontend Foundation: Angular Foundation (Epic #560) complete — all 9 features done
- ✅ Frontend Data Layer: Data Interaction Layer (Epic #577) 83% complete — Feature #620 done, #621 (SignalR) remaining
- ❌ Azure Functions: Planned as Epic #569 (Polyglot Azure Functions Architecture) - not yet started
- ✅ Infrastructure: IaC + CI/CD (Epic #570) complete — all 5 features done

**Success Metrics**:

- ✅ Working Angular app with authentication + Sales management UI
- ✅ 2-3 "wow factor" Azure Functions showcasing event-driven architecture
- ✅ Containerized with docker-compose for local dev
- ✅ Deployed to Azure with basic IaC (Bicep)
- ✅ Production-ready security (Entra ID, Key Vault, HTTPS)
- ✅ Observable with Application Insights

---

## Initiatives Overview (Streamlined for 4-Month Delivery)

**2 focused initiatives** that deliver maximum impact with minimal scope creep:

### Initiative Priority & Phasing Strategy (Revised 2026-03-15)

**Phase 1 - Angular Foundation** (Epic #560) -- ✅ COMPLETE (2026-03-17):

- ✅ Nx workspace, Tailwind/DaisyUI, Shared Component Library, Entra Auth, HTTP interceptors, Environment config, Error handling, App Shell, Routing with Guards

**Phase 2 - Data Interaction Layer** (Epic #577) -- 83% COMPLETE (5/6 features done):

- ✅ Shared data models, API contract corrections, enhanced HTTP pipeline, NgRx SignalStore foundation, domain data access libraries (Sales, HR)
- Remaining: SignalR real-time communication (#621)

**Phase 2.5 - API Completion** (Epic #873 — successor to closed #552) -- NEW (2026-04-27):

- 📋 **Wave 1**: Store Manager Completion — Features #874 (Contact Mgmt), #878 (Address Mgmt), #882 (Analytics), #886 (Sales Person Assignment Tracking) — 12 stories (#875-#877, #879-#881, #883-#885, #887-#889)
- 📋 **Wave 2**: HR Process Completion — Features #890 (Department Transfer), #892 (Pay Management), #894 (Department Reporting) — 5 stories (#891, #893, #895-#897). Stories 2.2, 2.4 covered by existing #751, #750.
- 📋 **Wave 3**: Person Foundation — Features #898 (Email Mgmt), #903 (Phone Mgmt), #908 (Directory & Search), #911 (PersonCreditCard fix) — 11 stories (#899-#902, #904-#907, #909-#910, #912)
- 📋 **Wave 4**: Lookup Endpoint Blitz — Features #913 (Production Lookups: 4 stories), #918 (Sales Lookups: 4 stories) — Stories 4.1, 4.2 covered by Done #699
- 📋 **Reparented Enablers** (now under #873): #715 (Sales DB Views, 3 stories), #716 (HR API Endpoints, 5 stories), #722 (HR DB Views, 2 stories)
- Full details: `docs/theBigPlan/api-completion-features.md`

**Phase 3 - Simple IaC + CI/CD** (Epic #570) -- ✅ COMPLETE (all 5 features done)

**Phase 4 - Testing Foundation** (Epic #565) -- NOT STARTED:

- .NET integration tests, k6 load testing, Playwright E2E smoke tests, Angular testing utilities

**Phase 5 - Sales & HR CRUD UI** (Epics #561 + #562) -- NOT STARTED:

- Sales CRUD UI: Stores, SalesPersons, Customers (requires Phase 2.5 Wave 1 API endpoints)
- HR CRUD UI: Employees, Departments (requires Phase 2.5 Wave 2 API endpoints)
- Real-Time Dashboard (Epic #563): SignalR-powered KPI dashboard

**Future Phases** (not yet scheduled):

- Azure Functions (Epic #569 - Polyglot Architecture with Rust, C#, Go, TypeScript)

---

## Initiative 1: Sales + HR Management Web Application (Angular + API)

### Vision Statement

**As a business manager, I want a modern web application to manage stores, sales personnel, employees, and departments, so that I can perform CRUD operations efficiently with a clean, responsive interface and see real-time updates when data changes across Sales and HR domains.**

### Success Criteria

**Given that I am accessing the application, when I use Sales and HR modules, then I should have:**

- ✅ Single sign-on with Microsoft Entra ID (already configured in API)
- ✅ Responsive Angular UI with Tailwind CSS working on desktop and tablet
- ✅ Full CRUD for Stores (list, create, update, view details)
- ✅ Full CRUD for SalesPersons (list, create, update, view details)
- ✅ Full CRUD for Employees (list, create, update, view details)
- ✅ Full CRUD for Departments (list, create, update, view details)
- ✅ Real-time updates via SignalR when other users make changes
- ✅ Form validation with clear error messages matching API validation
- ✅ Loading states and error handling for all API calls
- ✅ Unified dashboard with Sales + HR metrics (stores, sales persons, employees, departments)

### Business Value

- **Working Demo**: Complete vertical slices for Sales + HR prove architecture works end-to-end
- **Foundation for Growth**: Pattern established for adding Products, Production, Purchasing later
- **Real-Time Collaboration**: SignalR shows off event-driven capabilities across domains
- **Modern UX**: Tailwind CSS provides polished look with minimal effort
- **TDD Practice**: Building with tests from the start ensures quality and confidence

### Technical Foundation (Updated)

- **Framework**: Angular v21.1.1 standalone components with Signals (zoneless)
- **Monorepo**: Nx 22.5.1 workspace with single app + shared libraries
- **Styling**: Tailwind CSS v4.2.0 + DaisyUI v5.5.19 (Alpine Circuit theme)
- **State Management**: NgRx SignalStore (`@ngrx/signals` v21.0.1) -- extends existing signal-based architecture
- **HTTP**: HTTP interceptors for MSAL JWT, correlation IDs, error handling, loading states
- **Real-Time**: @microsoft/signalr for live updates (planned in Feature #621)
- **Auth**: MSAL Angular v5.1.0 + MSAL Browser v5.3.0 (Entra ID redirect flow)
- **Testing**: Vitest (unit), Playwright (E2E), k6 (load)
- **Telemetry**: Azure Application Insights + OpenTelemetry

### Epics (Initiative 1)

#### Epic #560 -- Angular Foundation ✅ COMPLETE (2026-03-17)

| Order | ID   | Feature                                  | Status  |
| ----- | ---- | ---------------------------------------- | ------- |
| --    | #571 | Nx Workspace Setup                       | ✅ Done |
| --    | #572 | Tailwind CSS + DaisyUI Configuration     | ✅ Done |
| --    | #573 | Shared Component Library                 | ✅ Done |
| --    | #574 | Routing Configuration with Guards        | ✅ Done |
| --    | #575 | Microsoft Entra ID Authentication        | ✅ Done |
| --    | #576 | HTTP Client with Interceptors            | ✅ Done |
| --    | #578 | Environment Configuration                | ✅ Done |
| --    | #579 | Global Error Handling and Loading States | ✅ Done |
| --    | #580 | App Shell with Navigation                | ✅ Done |

> **Pivot**: Feature #577 (API Service Layer) was promoted to its own Epic -- the Data Interaction Layer (see below). This reduced Epic #560 from 10 to 9 features.

#### Epic #577 -- Data Interaction Layer (4/6 features complete)

| Order | ID   | Feature                                  | Status         |
| ----- | ---- | ---------------------------------------- | -------------- |
| --    | #617 | Shared Data Models & API Contracts       | ✅ Done        |
| --    | #639 | API Contract Corrections                 | ✅ Done        |
| --    | #618 | Enhanced HTTP Pipeline                   | ✅ Done        |
| --    | #619 | NgRx SignalStore Foundation              | ✅ Done        |
| 3     | #620 | Domain Data Access Libraries (Sales, HR) | 🔄 In Progress |
| 4     | #621 | Real-Time Communication (SignalR)        | 📋 Planned     |

> **Pivot**: State management upgraded from plain Angular Signals to NgRx SignalStore. The codebase is 100% signal-based; SignalStore extends that pattern with `rxMethod()` for HTTP bridging, `withRequestStatus()`, and `withPagination()`.

#### Future Epics (not yet broken down in ADO)

- **Sales CRUD UI** -- Stores, SalesPersons list/create/update with validation
- **HR CRUD UI** -- Employees, Departments list/create/update with validation
- **Real-Time Dashboard** -- SignalR integration, charts, live updates

### Dependencies

- **API**: Sales endpoints already exist (Stores, SalesPersons, SalesTerritories) — Phase 2.5 Wave 1 adds analytics + sub-resource CRUD
- **API**: HR endpoints already exist (Employees, Departments, Shifts) — Phase 2.5 Wave 2 adds transfer + pay + reporting
- **API**: Person foundation (Phase 2.5 Wave 3) enables email/phone management for all domain UIs
- **API**: Lookup endpoints (Phase 2.5 Wave 4) provide dropdown data for all forms
- **Auth**: Microsoft Entra ID already configured in API
- **Sequencing**: API Completion (Phase 2.5) should precede CRUD UI work for richer, lovable screens

---

## Initiative 2: Event-Driven Microservices + Production Infrastructure

### Vision Statement

**As a solution architect, I want to demonstrate impressive Azure Functions capabilities with 2-3 showcase scenarios and production-ready infrastructure, so that the application highlights event-driven architecture, polyglot microservices, and cloud-native operations within a 4-month timeline.**

### Success Criteria

**Given that we're showcasing Azure capabilities with limited time, when I complete this initiative, then the system should demonstrate:**

- ✅ **2-3 "Wow Factor" Azure Functions** showing off event-driven patterns (not 10+)
- ✅ **Polyglot Microservices** with at least 1 non-.NET function (Go or Node.js)
- ✅ **Event Grid + Service Bus** wiring for decoupled messaging
- ✅ **Docker Containerization** with docker-compose for local dev
- ✅ **Bicep IaC** deploying all Azure resources (App Service, Functions, SQL, Key Vault, App Insights)
- ✅ **Application Insights** with correlation IDs, custom metrics, dashboards
- ✅ **GitHub Actions CI/CD** for API, Frontend, and Functions
- ✅ **Security Hardening** (Managed Identities, Key Vault, HTTPS, minimal attack surface)

### Business Value

- **Showcase Excellence**: Impressive Azure Functions demos attract interest
- **Production-Ready**: IaC + CI/CD enables reliable deployments
- **Extensibility**: Event-driven foundation makes adding features easy later
- **Learning**: Polyglot architecture demonstrates best practices
- **Cost-Effective**: Serverless functions only cost when running

### Technical Foundation

- **.NET Functions**: Durable Functions for workflows, HTTP triggers, Service Bus triggers
- **Go Functions**: High-performance data processing (alternative: Node.js for integrations)
- **Messaging**: Azure Service Bus for reliable queues, Event Grid for pub/sub
- **IaC**: Azure Bicep modules (parameterized for dev/prod)
- **Containers**: Docker multi-stage builds, docker-compose, Azure Container Registry
- **CI/CD**: GitHub Actions with secrets from Key Vault
- **Monitoring**: App Insights custom metrics, correlation IDs, dashboards, alerts

### Epics (Initiative 2)

#### Epic #570 -- Simple IaC + CI/CD ✅ COMPLETE (2026-03-29)

| Order | ID   | Feature                           | Status  |
| ----- | ---- | --------------------------------- | ------- |
| --    | #643 | IaC with Bicep                    | ✅ Done |
| --    | #644 | Environment & Secrets Management  | ✅ Done |
| --    | #645 | GitHub Actions PR Validation      | ✅ Done |
| --    | #646 | Azure Pipelines CI/CD Enhancement | ✅ Done |
| --    | #647 | Docker Local Dev Environment      | ✅ Done |

> **Pivot**: Epic #566 (Docker Basics) has been superseded by Feature #647 within this epic. User will close #566 manually.
> **Architecture**: GitHub Actions for PR validation, Azure Pipelines for deployment. Angular hosted on Azure App Service (not Static Web Apps). Single B1 Linux plan shared by all App Services (~$13/mo). Budget: ~$18/mo total on MSDN subscription.

#### Epic #565 -- Testing Foundation (0/4 features complete)

| Order | ID   | Feature                                        | Status     |
| ----- | ---- | ---------------------------------------------- | ---------- |
| 10    | #669 | .NET Integration Tests (WebApplicationFactory) | 📋 Planned |
| 11    | #670 | k6 Load Testing Foundation                     | 📋 Planned |
| 12    | #671 | Playwright E2E Smoke Test Suite                | 📋 Planned |
| 13    | #672 | Angular Testing Foundation & Utilities         | 📋 Planned |

> **Scope clarification**: 151 existing .NET unit tests and 16 existing Angular spec files are NOT in scope. This epic adds integration tests, load tests, E2E smoke tests, and Angular testing utilities only.

#### Future Epics (not yet started)

- **Epic #569 -- Polyglot Azure Functions Architecture** (9 features: Rust, C#, Go, TypeScript functions)
  - BOM Cost Explosion Engine (Rust), Sales Order Saga (C# Durable), Inventory Processor (Go), Product Image Pipeline (Rust), Notification Hub (TypeScript), Vendor Health (Go), Employee Lifecycle (C# Durable), Reporting (C# Durable)

### "Wow Factor" Function Scenarios (Streamlined to 3)

#### Scenario 1: Intelligent Document Processing (Primary Showcase)

**As a sales manager, I want to upload store registration documents (PDFs), so that the system automatically extracts data and creates store records without manual entry.**

**Technical Stack**:

- Azure Blob Storage trigger when PDF uploaded
- .NET Function calls Azure Form Recognizer for OCR
- Validation logic checks extracted data
- Creates Store record via API call
- SignalR notification to frontend with results
- Complete in ~5-10 seconds with progress updates

**Why This**: Shows Azure AI, event-driven trigger, external service integration, real-time updates

#### Scenario 2: Real-Time Transaction Monitoring (Secondary Showcase)

**As a security analyst, I want suspicious transactions flagged in real-time, so that fraud is prevented before financial loss.**

**Technical Stack**:

- Event Grid publishes "OrderCreated" events
- .NET Durable Function orchestrates validation workflow
- Go function (or Node.js) performs high-speed rule checks
- If flagged: Teams/Slack webhook notification
- Results stored with correlation ID for audit

**Why This**: Shows Durable Functions, polyglot architecture, real-time alerting, external webhooks

#### Scenario 3: Scheduled Reporting & Data Export (Tertiary Showcase)

**As a manager, I want daily sales reports emailed automatically, so that I stay informed without manual work.**

**Technical Stack**:

- Timer-triggered function runs daily at 8am
- Query API for sales metrics
- Generate PDF/Excel report
- Email via SendGrid binding
- Store report in Blob Storage

**Why This**: Shows timer triggers, bindings, scheduled automation, external service integration

### Dependencies

- **Requires**: Initiative 1 (Frontend + API) for end-to-end scenarios
- **Deploys**: Functions alongside existing API infrastructure

---

## Execution Order: All Remaining Features

The precise order for completing remaining work (577 → 552 API Completion → 570 → 565 → UI):

### Phase 2: Data Interaction Layer (Epic #577)

| #     | Epic | Feature                                           | Status     | Depends On           |
| ----- | ---- | ------------------------------------------------- | ---------- | -------------------- |
| ~~3~~ | #577 | ~~Domain Data Access Libraries (Sales, HR) #620~~ | ✅ Done    | #619 (SignalStore)   |
| 4     | #577 | Real-Time Communication (SignalR) #621            | 📋 Planned | #618 (HTTP Pipeline) |

### Phase 2.5: API Completion (Epic #873) — successor to closed #552

**Wave 1: Store Manager Completion** (4 features, 12 stories)

| Feature                          | ID   | Stories       | Depends On           |
| -------------------------------- | ---- | ------------- | -------------------- |
| Store Contact Management         | #874 | #875-#877 (3) | Existing #691 (read) |
| Store Address Management         | #878 | #879-#881 (3) | Existing #690 (read) |
| Store Analytics & Insights       | #882 | #883-#885 (3) | --                   |
| Sales Person Assignment Tracking | #886 | #887-#889 (3) | DbUp #887 first      |

**Wave 2: HR Process Completion** (3 features, 5 stories — 2 deduplicated)

| Feature                      | ID   | Stories       | Notes                     |
| ---------------------------- | ---- | ------------- | ------------------------- |
| Employee Department Transfer | #890 | #891 (1)      | Story 2.2 → existing #751 |
| Employee Pay Management      | #892 | #893 (1)      | Story 2.4 → existing #750 |
| Department Reporting         | #894 | #895-#897 (3) | --                        |

**Wave 3: Person Foundation** (4 features, 11 stories)

| Feature                        | ID   | Stories       | Depends On   |
| ------------------------------ | ---- | ------------- | ------------ |
| Person Email Management        | #898 | #899-#902 (4) | --           |
| Person Phone Management        | #903 | #904-#907 (4) | --           |
| Person Directory & Search      | #908 | #909-#910 (2) | #898, #903   |
| PersonCreditCard DbContext Fix | #911 | #912 (1)      | -- (bug fix) |

**Wave 4: Lookup Endpoint Blitz** (2 features, 8 stories — 2 deduplicated, all parallelizable)

| Feature                     | ID   | Stories       | Notes                       |
| --------------------------- | ---- | ------------- | --------------------------- |
| Production Lookup Endpoints | #913 | #914-#917 (4) | Stories 4.1+4.2 → Done #699 |
| Sales Lookup Endpoints      | #918 | #919-#922 (4) | --                          |

**Reparented Enabler Features** (moved from #561/#562 to #873)

| Feature                                | ID   | Stories       | Original Parent |
| -------------------------------------- | ---- | ------------- | --------------- |
| Sales Database Views & Indexes         | #715 | #746-#748 (3) | was #561        |
| HR Additional API Endpoints            | #716 | #749-#753 (5) | was #562        |
| HR Database Views (Dashboard/OrgChart) | #722 | #771-#772 (2) | was #562        |

**Total under Epic #873**: 16 Features + 46 Stories (13 net-new Features + 36 net-new Stories + 3 reparented Features + 10 reparented Stories)

> **Full story details**: `docs/theBigPlan/api-completion-features.md`

### Phase 3: Simple IaC + CI/CD (Epic #570) ✅ COMPLETE

| #   | Epic | Feature                                | Status  | Depends On   |
| --- | ---- | -------------------------------------- | ------- | ------------ |
| 18  | #570 | Environment & Secrets Management #644  | ✅ Done | #643 (Bicep) |
| 19  | #570 | Azure Pipelines CI/CD Enhancement #646 | ✅ Done | #643, #644   |

### Phase 4: Testing Foundation (Epic #565)

| #   | Epic | Feature                                             | Status     | Depends On           |
| --- | ---- | --------------------------------------------------- | ---------- | -------------------- |
| 20  | #565 | Angular Testing Foundation & Utilities #672         | 📋 Planned | --                   |
| 21  | #565 | .NET Integration Tests (WebApplicationFactory) #669 | 📋 Planned | --                   |
| 22  | #565 | k6 Load Testing Foundation #670                     | 📋 Planned | --                   |
| 23  | #565 | Playwright E2E Smoke Test Suite #671                | 📋 Planned | Deployed app (ideal) |

### Phase 5: Sales & HR CRUD UI (Epics #561 + #562)

- Requires Phase 2.5 API Completion for rich, lovable screens
- See `epics-561-562-sales-hr-ui.md` in memory for story breakdown

**Note on parallelism**: Waves 1-3 are sequential (each builds on prior work). Wave 4 is fully parallel — each lookup story is independent and ideal for batch development. Phases 3 and 4 can overlap with Phase 2.5 if desired (no API dependencies).

---

## Summary: Progress and Roadmap

### Completed Work (17 features across 3 epics)

**Epic #560 -- Angular Foundation**: ✅ ALL 9 features done (2026-03-17)

- Nx Workspace, Tailwind/DaisyUI, Shared Component Library, Routing with Guards, Entra Auth, HTTP Interceptors, Environment Config, Error Handling/Loading, App Shell

**Epic #577 -- Data Interaction Layer**: 5 of 6 features done

- Shared Data Models, API Contract Corrections, Enhanced HTTP Pipeline, NgRx SignalStore Foundation, Domain Data Access Libraries (Sales & HR)

**Epic #570 -- Simple IaC + CI/CD**: ✅ ALL 5 features done (2026-03-29)

- Bicep IaC, Environment & Secrets Management, GitHub Actions PR Validation, Azure Pipelines CI/CD Enhancement, Docker Local Dev Environment

### Remaining Work (21 features across 3 epics)

- **Epic #577**: 1 feature remaining (#621 SignalR)
- **Epic #873 API Completion**: 16 features (13 net-new + 3 reparented enablers #715/#716/#722) / 46 stories (36 net-new + 10 reparented; 4 stories deduplicated against existing #699/#750/#751) — see `api-completion-features.md`
- **Epic #565**: 4 features (all new)

### Success Criteria for "Done"

- ✅ Angular app deployed with **Sales + HR management UI**
- ✅ Authentication working with Microsoft Entra ID
- ✅ Real-time SignalR updates between users
- ✅ **Solid test coverage** (E2E smoke tests, API integration tests, load tests, Angular testing utils)
- ✅ Everything containerized with **simple Docker setup** (Dockerfiles + docker-compose)
- ✅ Deployed to Azure via **basic Bicep IaC**
- ✅ **CI/CD pipelines** (GitHub Actions for PR validation, Azure Pipelines for deployment)
- ✅ Application Insights with correlation IDs and telemetry
- ✅ Security hardened (Key Vault, Managed Identities, HTTPS, Entra ID)

### What's NOT in Scope (Future Extensions)

- ❌ Full Products/Production/Purchasing CRUD (lookup endpoints ARE in scope via Wave 4; full CRUD is future)
- ❌ PWA/Offline capabilities
- ❌ Mobile apps (responsive web only)
- ❌ Multi-region deployment (single Azure region)
- ❌ Comprehensive E2E test coverage (only critical user paths)
- ❌ Complex Docker orchestration (Kubernetes, service mesh, etc.)
- ❌ Advanced CI/CD (blue-green deployments, canary releases, etc.)

---

## Pivots and Changes from Original Plan

This section documents significant deviations from the v1.0 plan (2026-01-17).

### Structural Changes

1. **Feature #577 promoted to Epic** -- "API Service Layer" was insufficient. ApiService already had CRUD methods. Expanded to a full Data Interaction Layer with 6 child features covering shared models, enhanced HTTP, NgRx SignalStore, domain data access, and SignalR.
2. **Epic #566 (Docker Basics) superseded** -- Absorbed into Feature #647 (Docker Local Dev Environment) under Epic #570. User will close #566 manually.
3. **Azure Functions scope expanded** -- Original plan called for 3 "wow factor" scenarios. Epic #569 now defines 9 features across 4 languages (Rust, C#, Go, TypeScript). This is a future-phase epic.

### Technology Decisions (vs. Original Plan)

1. **State management**: NgRx SignalStore replaces "plain Angular Signals" -- the codebase is 100% signal-based, and SignalStore extends (not replaces) that pattern.
2. **Auth**: MSAL Angular v5 with redirect flow (not popup). Token cache uses `BrowserCacheLocation.LocalStorage` for cross-tab SSO.
3. **Styling**: Tailwind CSS v4 is CSS-first (no `tailwind.config.js`). DaisyUI v5 uses `@plugin` directives. Alpine Circuit custom theme.
4. **Angular**: Zoneless (no Zone.js). Uses `provideAppInitializer` (not deprecated `APP_INITIALIZER`). `@angular/animations` removed -- Angular 21 uses native CSS animations.
5. **CI/CD split**: GitHub Actions for PR validation, Azure Pipelines for deployment (not GitHub Actions for everything).
6. **Deployment**: Zip push to App Service (not ACR/containers). Dockerfiles are for local dev only.
7. **Multi-API from the start**: `Environment.api` uses `{ primary: ApiEndpoint; [key: string]: ApiEndpoint }` pattern with per-API OAuth scopes.

### Scope Adjustments

- Original "Week-by-Week Plan" timelines were aspirational. Real delivery is feature-by-feature, not week-locked.
- Azure Functions (Epic #569) moved to future phases, replacing the 3 original scenarios with a more ambitious 9-feature polyglot architecture.
- Sales CRUD UI, HR CRUD UI, and Real-Time Dashboard epics have already been broken down in ADO (Epics #561, #562, #563). Per Phase 2.5 reconciliation (2026-04-27), three pre-existing API/DB enabler Features under those Epics (#715, #716, #722) were reparented to the new API Completion Epic #873 — UI Epics now cleanly hold UI work only.
- **API Completion pivot (2026-03-20 → executed 2026-04-27)**: Original plan said "Products/Production/Purchasing not in scope." Revised to round out the API _before_ building UI, so the Angular screens have rich data to work with. Phase 2.5 covers 4 waves: Store Manager enrichment (analytics, sub-resource CRUD, sales person assignment history), HR gap closure (department transfers, pay management, headcount reporting), Person foundation (email/phone CRUD, directory search), and lookup endpoints across Production + Sales schemas. **Implemented as Epic #873** (closing Epic #552 "Round 1"): 13 net-new Features + 3 reparented enabler Features = 16 Features; 36 net-new Stories + 10 reparented Stories = 46 Stories. 4 originally-planned stories were deduplicated against existing items: Story 2.2 → #751, Story 2.4 → #750, Stories 4.1+4.2 → Done #699. One new DB table (`Sales.StoreSalesPersonHistory`) via DbUp migration (Story #887).

---

## Markdown Documentation Structure

All work breakdown documents are in: `docs/theBigPlan/`

### File Organization

```
docs/theBigPlan/
├── README.md                                      # Overview and navigation guide
├── TheBigPlan.md                                  # This file - complete plan
├── api-completion-features.md                     # Phase 2.5: Epic #873 — 16 Features, 46 Stories (4 waves; 4 stories deduplicated)
├── initiatives/
│   ├── 01-sales-hr-web-application.md              # Initiative 1 complete details
│   └── 02-event-driven-microservices-infrastructure.md  # Initiative 2 complete details
├── epics/
│   ├── 1.1-angular-foundation.md                   # Epic 1: Angular setup
│   ├── 1.2-sales-crud-ui.md                        # Epic 2: Sales UI
│   ├── 1.3-hr-crud-ui.md                           # Epic 3: HR UI
│   ├── 1.4-realtime-dashboard.md                   # Epic 4: Dashboard
│   ├── 2.1-testing-foundation.md                   # Epic 5: Testing setup
│   ├── 2.2-docker-basics.md                        # Epic 6: Docker
│   ├── 2.3-document-processing-function.md         # Epic 7: OCR Function
│   ├── 2.4-fraud-detection-function.md             # Epic 8: Fraud Function
│   ├── 2.5-polyglot-integration-function.md        # Epic 9: Polyglot Function
│   └── 2.6-simple-iac-cicd.md                      # Epic 10: IaC/CI/CD
└── templates/
    ├── epic-template.md                            # Template for creating epics
    └── feature-template.md                         # Template for creating features
```

### Document Content Structure

Each markdown file follows this structure:

#### README.md

- Project overview
- 4-month timeline visualization
- Quick links to all initiatives and epics
- Success criteria summary

#### Initiative Files (2 files)

- Vision statement with Gherkin acceptance criteria
- Business value proposition
- Technical foundation
- List of epics under this initiative
- Dependencies and blockers

#### Epic Files (10 files)

- Epic title and overview
- User story format: "As a... I want... So that..."
- Gherkin acceptance criteria: "Given... When... Then..."
- Features list (8-10 features per epic)
- Each feature as a sub-heading with:
  - Feature description in Scrum format
  - Acceptance criteria in Gherkin format
  - Technical notes (key files, patterns, dependencies)
  - Testing requirements (unit, integration, E2E)
- Estimated story points for epic
- Week assignment from 16-week plan

---

**Document Version**: 2.4
**Last Updated**: 2026-04-27
**Status**: Active Execution — Epic #560 complete, #570 complete, #577 83% complete (#621 SignalR remaining), #552 closed Done, **#873 API Completion created** (16 Features / 46 Stories ready in ADO, 4 stories deduplicated against existing #699/#750/#751), #565 planned
