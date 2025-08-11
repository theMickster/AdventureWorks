# AdventureWorks Enterprise Application - Implementation Plan

**Project Vision**: Build a modern, production-ready AdventureWorks showcase demonstrating Angular v21, .NET 10.0 API with Clean Architecture, Azure Functions microservices, and cloud-native practices - achievable by solo developer + Claude Code.

**Scope Philosophy**: Focus on **one domain end-to-end** (Sales) with impressive Azure Functions scenarios, rather than broad but shallow coverage. Quality over quantity. Showcase best-in-class architecture that can be extended later.

**Current Status Summary** (as of 2026-03-19):

- ✅ Backend API: Sales, HR, Address Management fully implemented with CQRS + Clean Architecture
- ✅ Frontend Foundation: Angular Foundation (Epic #560) complete — all 9 features done
- 🔄 Frontend Data Layer: Data Interaction Layer (Epic #577) 67% complete — Feature #620 in progress
- ❌ Azure Functions: Planned as Epic #569 (Polyglot Azure Functions Architecture) - not yet started
- 🔄 Infrastructure: IaC + CI/CD (Epic #570) 60% complete — Bicep, PR validation, Docker done; secrets + CD remaining

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

**Phase 2 - Data Interaction Layer** (Epic #577) -- 67% COMPLETE (Feature #620 In Progress):

- ✅ Shared data models, API contract corrections, enhanced HTTP pipeline, NgRx SignalStore foundation
- 🔄 Domain data access libraries (Sales, HR) — in progress
- Remaining: SignalR real-time communication

**Phase 3 - Simple IaC + CI/CD** (Epic #570) -- 60% COMPLETE (3/5 features done):

- ✅ Bicep IaC, GitHub Actions PR validation, Docker local dev
- Remaining: secrets management, Azure Pipelines CI/CD

**Phase 4 - Testing Foundation** (Epic #565) -- NOT STARTED:

- .NET integration tests, k6 load testing, Playwright E2E smoke tests, Angular testing utilities

**Future Phases** (not yet scheduled):

- Sales CRUD UI, HR CRUD UI, Real-Time Dashboard
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

| Order | ID   | Feature                                  | Status        |
| ----- | ---- | ---------------------------------------- | ------------- |
| --    | #617 | Shared Data Models & API Contracts       | ✅ Done       |
| --    | #639 | API Contract Corrections                 | ✅ Done       |
| --    | #618 | Enhanced HTTP Pipeline                   | ✅ Done       |
| --    | #619 | NgRx SignalStore Foundation              | ✅ Done       |
| 3     | #620 | Domain Data Access Libraries (Sales, HR) | 🔄 In Progress |
| 4     | #621 | Real-Time Communication (SignalR)        | 📋 Planned    |

> **Pivot**: State management upgraded from plain Angular Signals to NgRx SignalStore. The codebase is 100% signal-based; SignalStore extends that pattern with `rxMethod()` for HTTP bridging, `withRequestStatus()`, and `withPagination()`.

#### Future Epics (not yet broken down in ADO)

- **Sales CRUD UI** -- Stores, SalesPersons list/create/update with validation
- **HR CRUD UI** -- Employees, Departments list/create/update with validation
- **Real-Time Dashboard** -- SignalR integration, charts, live updates

### Dependencies

- **API**: Sales endpoints already exist (Stores, SalesPersons, SalesTerritories)
- **API**: HR endpoints already exist (Employees, Departments, Shifts)
- **Auth**: Microsoft Entra ID already configured in API
- **No blockers**: Both domains fully implemented in backend, can start frontend immediately

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

#### Epic #570 -- Simple IaC + CI/CD (3/5 features complete)

| Order | ID   | Feature                           | Status     |
| ----- | ---- | --------------------------------- | ---------- |
| --    | #643 | IaC with Bicep                    | ✅ Done    |
| 6     | #644 | Environment & Secrets Management  | 📋 Planned |
| --    | #645 | GitHub Actions PR Validation      | ✅ Done    |
| 8     | #646 | Azure Pipelines CI/CD Enhancement | 📋 Planned |
| --    | #647 | Docker Local Dev Environment      | ✅ Done    |

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

The precise order for completing the 4 active epics (560 -> 577 -> 570 -> 565):

| #   | Epic | ID   | Feature                                        | Status         | Depends On           |
| --- | ---- | ---- | ---------------------------------------------- | -------------- | -------------------- |
| ~~1~~ | #560 | #573 | ~~Shared Component Library~~                 | ✅ Done        | #572 (DaisyUI)       |
| ~~2~~ | #560 | #574 | ~~Routing Configuration with Guards~~        | ✅ Done        | #575 (Auth)          |
| 3   | #577 | #620 | Domain Data Access Libraries (Sales, HR)       | 🔄 In Progress | #619 (SignalStore)   |
| 4   | #577 | #621 | Real-Time Communication (SignalR)              | 📋 Planned | #618 (HTTP Pipeline) |
| ~~5~~ | #570 | #643 | ~~IaC with Bicep~~                           | ✅ Done        | --                   |
| 6   | #570 | #644 | Environment & Secrets Management               | 📋 Planned | #643                 |
| ~~7~~ | #570 | #645 | ~~GitHub Actions PR Validation~~             | ✅ Done        | --                   |
| 8   | #570 | #646 | Azure Pipelines CI/CD Enhancement              | 📋 Planned | #643, #644           |
| ~~9~~ | #570 | #647 | ~~Docker Local Dev Environment~~             | ✅ Done        | --                   |
| 10  | #565 | #672 | Angular Testing Foundation & Utilities         | 📋 Planned | --                   |
| 11  | #565 | #669 | .NET Integration Tests (WebApplicationFactory) | 📋 Planned | --                   |
| 12  | #565 | #670 | k6 Load Testing Foundation                     | 📋 Planned | --                   |
| 13  | #565 | #671 | Playwright E2E Smoke Test Suite                | 📋 Planned | Deployed app (ideal) |

**Note on parallelism within epics**: Features #643, #645, and #647 in Epic #570 are complete. The remaining #644 and #646 are sequential (#646 depends on #644). All four features in Epic #565 are independent. The order above reflects the most logical solo-developer sequence.

---

## Summary: Progress and Roadmap

### Completed Work (16 features across 3 epics)

**Epic #560 -- Angular Foundation**: ✅ ALL 9 features done (2026-03-17)

- Nx Workspace, Tailwind/DaisyUI, Shared Component Library, Routing with Guards, Entra Auth, HTTP Interceptors, Environment Config, Error Handling/Loading, App Shell

**Epic #577 -- Data Interaction Layer**: 4 of 6 features done

- Shared Data Models, API Contract Corrections, Enhanced HTTP Pipeline, NgRx SignalStore Foundation

**Epic #570 -- Simple IaC + CI/CD**: 3 of 5 features done

- Bicep IaC, GitHub Actions PR Validation, Docker Local Dev Environment

### Remaining Work (8 features across 3 epics)

- **Epic #577**: 2 features remaining (#620 in progress, #621 planned)
- **Epic #570**: 2 features remaining (#644, #646)
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

- ❌ Products/Production/Purchasing domains (focus on Sales + HR only)
- ❌ PWA/Offline capabilities
- ❌ Mobile apps (responsive web only)
- ❌ Advanced search/filtering (basic pagination only)
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
- Sales CRUD UI, HR CRUD UI, and Real-Time Dashboard epics have not yet been broken down in ADO. They follow after Epics #560 and #577 are complete.

---

## Markdown Documentation Structure

All work breakdown documents are in: `docs/theBigPlan/`

### File Organization

```
docs/theBigPlan/
├── README.md                                      # Overview and navigation guide
├── TheBigPlan.md                                  # This file - complete plan
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

**Document Version**: 2.1
**Last Updated**: 2026-03-20
**Status**: Active Execution - Epic #560 complete, #577 in progress, #570 60% complete (#643/#645/#647 done), #565 planned
