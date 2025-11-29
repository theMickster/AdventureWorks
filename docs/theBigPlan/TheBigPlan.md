# AdventureWorks Enterprise Application - Implementation Plan

## Current Snapshot

- ✅ Backend API Round 1: Epic #552 closed (Done)
- ✅ Backend API Round 2: Epic #873 (API Completion) done (Done 2026-05-22)
- ✅ Frontend Foundation: Epic #560 complete (all 9 features done)
- ✅ Frontend Data Layer: Epic #577 complete (all 6 features done)
- ✅ Infrastructure: Epic #570 complete (all 5 features done)
- 🚧 Testing Foundation: Epic #565 in progress (#669, #923 done; #670, #671, #672 remaining)
- 🚧 UI pipeline in ADO: #561 in progress (US-739 done); #562 and #563 currently New
- 📋 Additional Initiative #559 backlog epics: #935 (Customer), #936 (Work Orders/Manufacturing), #937 (Purchasing/Vendor)
- 📋 Future infrastructure/function backlog: #569, #567, #568 (#566 marked Removed)

---

## Initiative 1: Sales + HR Management Web Application (Angular + API)

### Technical Foundation

- **Framework**: Angular v21.1.1 standalone components with Signals (zoneless)
- **Monorepo**: Nx 22+ workspace
- **Styling**: Tailwind CSS v4.2.0 + DaisyUI v5.5.19
- **State Management**: NgRx SignalStore (`@ngrx/signals` v21.0.1)
- **HTTP**: Interceptors for MSAL JWT, correlation IDs, error/loading handling
- **Real-Time**: `@microsoft/signalr`
- **Auth**: MSAL Angular v5.1.0 + MSAL Browser v5.3.0
- **Testing**: Vitest, Playwright, k6
- **Telemetry**: Azure Application Insights + OpenTelemetry

### Epic Status (Initiative 1)

- **ADO rollup**: Epics — Done 4, In Progress 1, New 5. Features — Done 36, In Progress 8, New 16.

#### Epic #560 -- Angular Foundation ✅ COMPLETE (2026-03-17)

- ✅ #571 Nx Workspace Setup
- ✅ #572 Tailwind CSS + DaisyUI Configuration
- ✅ #573 Shared Component Library
- ✅ #574 Routing Configuration with Guards
- ✅ #575 Microsoft Entra ID Authentication
- ✅ #576 HTTP Client with Interceptors
- ✅ #578 Environment Configuration
- ✅ #579 Global Error Handling and Loading States
- ✅ #580 App Shell with Navigation

#### Epic #577 -- Data Interaction Layer ✅ COMPLETE (2026-05-25)

- ✅ #617 Shared Data Models & API Contracts
- ✅ #639 API Contract Corrections
- ✅ #618 Enhanced HTTP Pipeline
- ✅ #619 NgRx SignalStore Foundation
- ✅ #620 Domain Data Access Libraries (Sales, HR)
- ✅ #621 Real-Time Communication (SignalR)

#### Epic #873 -- API Completion ✅ COMPLETE (2026-05-22)

- ✅ **Wave 1**: Store Manager Completion (#874, #878, #882, #886)
- ✅ **Wave 2**: HR Process Completion (#890, #892, #894)
- ✅ **Wave 3**: Person Foundation (#898, #903, #908, #911)
- ✅ **Wave 4**: Lookup Endpoint Blitz (#913, #918)
- ✅ Reparented enablers under #873: #715, #716, #722
- ✅ Additional enablers: #669 (.NET API integration tests), #709 (sales order paginated list), US-727 dashboard KPI endpoint

#### Additional Initiative 1 Epics (ADO status)

- 🚧 #561 Sales CRUD UI — In Progress (8 child features); ✅ Feature 713 (Sales Dashboard) done
- 📋 #562 HR CRUD UI — New (5 child features)
- 📋 #563 Real-Time Dashboard — New (5 child features)
- 📋 #935 Customer Module — New (4 child features)
- 📋 #936 Work Orders / Manufacturing Module — New (4 child features)
- 📋 #937 Purchasing / Vendor Management Module — New (4 child features)

### Dependencies and Sequencing (Initiative 1)

- API Completion (#873) is complete and enables richer UI work in Epics #561 and #562.
- Person foundation and lookup endpoints from #873 are available for form and search screens.
- SignalR real-time infrastructure is available and being used by dashboard work.

---

## Initiative 2: Event-Driven Microservices + Production Infrastructure

### Epic Status (Initiative 2)

- **ADO rollup**: Epics — Done 1, In Progress 1, New 3, Removed 1. Features — Done 7, New 12.

#### Epic #570 -- Simple IaC + CI/CD ✅ COMPLETE (2026-03-29)

- ✅ #643 IaC with Bicep
- ✅ #644 Environment & Secrets Management
- ✅ #645 GitHub Actions PR Validation
- ✅ #646 Azure Pipelines CI/CD Enhancement
- ✅ #647 Docker Local Dev Environment

#### Epic #565 -- Testing Foundation 🚧 IN PROGRESS

- ✅ #669 .NET Integration Tests (WebApplicationFactory)
- ✅ #923 Local Database Reset Tooling for Test Workflows
- 📋 #670 k6 Load Testing Foundation
- 📋 #671 Playwright E2E Smoke Test Suite
- 📋 #672 Angular Testing Foundation & Utilities

#### Epic #569 -- Polyglot Azure Functions Architecture 📋 FUTURE

- Future phase epic for Azure Functions microservice scenarios.

#### Additional Initiative 2 Epics (ADO status)

- 📋 #567 Intelligent Document Processing Function — New
- 📋 #568 Real-Time Fraud Detection Function — New
- ❌ #566 Docker Basics — Removed

---

## Execution Queue

### Now

1. Continue Phase 5 Sales + HR CRUD UI work (Epics #561 and #562).
2. Continue Real-Time Dashboard work (Epic #563).
3. Complete remaining Testing Foundation features (#670, #671, #672).

### Later

1. Start Azure Functions work under Epic #569 after current UI/testing priorities.

---

## Completed Milestones (Compressed)

- ✅ Epic #560 complete
- ✅ Epic #577 complete
- ✅ Epic #873 complete
- ✅ Epic #570 complete
- 🚧 Epic #565 partially complete
- 🚧 Phase 5 UI/dashboard in progress

---

## Out of Scope (Current Phase)

- Full Products/Production/Purchasing CRUD (lookup endpoints are complete; full CRUD is future).
- PWA/offline capabilities.
- Mobile apps beyond responsive web.
- Multi-region deployment.
- Advanced deployment strategies (blue-green/canary).

---

## References

- ADO Initiative #559 (Web App)
- ADO Initiative #564 (Microservices + Infrastructure)

---

**Document Version**: 3.2  
**Last Updated**: 2026-06-26  
**Status**: Active execution
