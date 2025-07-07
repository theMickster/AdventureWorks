# AdventureWorks Enterprise Application - 4-Month Implementation Plan

**Project Vision**: Build a modern, production-ready AdventureWorks showcase demonstrating Angular v21, .NET 10.0 API with Clean Architecture, Azure Functions microservices, and cloud-native practices - achievable by solo developer + Claude Code in 4 months.

**Scope Philosophy**: Focus on **one domain end-to-end** (Sales) with impressive Azure Functions scenarios, rather than broad but shallow coverage. Quality over quantity. Showcase best-in-class architecture that can be extended later.

**Current Status Summary**:
- ✅ Backend API: Sales, HR, Address Management fully implemented with CQRS + Clean Architecture
- ❌ Frontend: Empty directory - need Angular skeleton + Sales UI only
- ❌ Azure Functions: Empty directories - focus on 2-3 impressive showcase scenarios
- ⚠️ Infrastructure: Azure DevOps exists, need Docker + essential IaC

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

### Initiative Priority & Phasing Strategy

**Month 1 - Foundation**:
- Angular skeleton with authentication
- Sales UI (CRUD for Stores & SalesPersons)
- Docker containerization

**Month 2 - Vertical Integration**:
- Complete Sales management UI with dashboards
- First Azure Function (Document OCR Pipeline)
- SignalR real-time updates

**Month 3 - Event-Driven Showcase**:
- Second Azure Function (Fraud Detection or Inventory AI)
- Multi-language function (Go or Node.js integration)
- Event Grid + Service Bus wiring

**Month 4 - Production Hardening**:
- Bicep IaC for Azure deployment
- E2E Playwright tests
- Monitoring dashboards + alerts
- Performance optimization

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

### Technical Foundation
- **Framework**: Angular v21 standalone components with Signals
- **Monorepo**: Nx workspace with single app + shared libraries
- **Styling**: Tailwind CSS + DaisyUI component library (faster than building from scratch)
- **State Management**: Angular Signals (simpler than NgRx for this scope)
- **HTTP**: HTTP interceptors for JWT, correlation IDs, error handling
- **Real-Time**: @microsoft/signalr for live updates
- **Testing**: Playwright for critical user flows only (not comprehensive coverage yet)

### Epics (4 epics for Initiative 1)
1. **Angular Foundation** (Week 1-2) - Nx setup, auth, routing, HTTP client, shared components, Tailwind config
2. **Sales CRUD UI** (Week 3-4) - Store and SalesPerson list/create/update forms with validation
3. **HR CRUD UI** (Week 5-6) - Employee list/create/update, department management (API already exists)
4. **Real-Time Dashboard** (Week 7-8) - SignalR integration, dashboard with charts, live updates for Sales & HR

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

### Epics (6 epics for Initiative 2)
1. **Test-Driven Development Foundation** (Week 9) - Unit tests (TDD), API integration tests, E2E test framework (Playwright), load testing setup
2. **Docker Basics** (Week 10) - Simple Dockerfiles for API/Frontend, basic docker-compose for local dev
3. **Intelligent Document Pipeline Function** (Week 11-12) - Blob trigger → Azure Form Recognizer → validation → store results (with tests)
4. **Real-Time Fraud Detection Function** (Week 13) - Event Grid → rules engine → alert workflow with Durable Functions (with tests)
5. **Polyglot Integration Function** (Week 14) - Go or Node.js function for external API integration (with tests)
6. **Simple IaC + CI/CD** (Week 15-16) - Basic Bicep templates, straightforward GitHub Actions for deployment

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

## Summary: 4-Month Roadmap

### Total Scope (Streamlined)
- **2 Initiatives** (down from 4)
- **10 Epics** total (down from 40+)
  - Initiative 1: 4 epics (Angular Foundation, Sales CRUD, HR CRUD, Real-Time Dashboard)
  - Initiative 2: 6 epics (Testing Foundation, Docker Basics, 3 Azure Functions, Simple IaC/CI/CD)
- **~70-90 Features** (down from 400+)
- **~140-180 User Stories** (down from 800+)
- **Estimated: 700-900 story points**
- **Timeline: 16 weeks (4 months)** with solo dev + Claude Code using TDD approach

### Week-by-Week Plan
| Week | Focus | Deliverable |
|------|-------|-------------|
| 1-2 | Angular Foundation | Nx workspace, auth, routing, HTTP client, base components with Tailwind |
| 3-4 | Sales CRUD UI | Store & SalesPerson list/create/update with validation (TDD) |
| 5-6 | HR CRUD UI | Employee & Department list/create/update with validation (TDD) |
| 7-8 | Real-Time Dashboard | SignalR integration, charts, live updates for Sales & HR metrics |
| 9 | Testing Foundation | E2E framework (Playwright), API integration tests, load testing (k6), unit test patterns |
| 10 | Docker Basics | Simple Dockerfiles (API/Frontend), basic docker-compose for local dev |
| 11-12 | Document Processing Function | Blob trigger → Form Recognizer → validation → results (with tests) |
| 13 | Fraud Detection Function | Event Grid → Durable Function → alerting (with tests) |
| 14 | Polyglot Integration Function | Go or Node.js for external API integration (with tests) |
| 15-16 | Simple IaC + CI/CD | Basic Bicep templates, straightforward GitHub Actions for deploy |

### Success Criteria for "Done"
- ✅ Angular app deployed with **Sales + HR management UI**
- ✅ Authentication working with Microsoft Entra ID
- ✅ **3 impressive Azure Functions** demonstrating event-driven patterns (Document OCR, Fraud Detection, Integration)
- ✅ Real-time SignalR updates between users
- ✅ **Solid test coverage** (E2E tests for critical flows, API integration tests, unit tests via TDD, load tests)
- ✅ Everything containerized with **simple Docker setup** (Dockerfiles + docker-compose)
- ✅ Deployed to Azure via **basic Bicep IaC**
- ✅ **Simple CI/CD** pipelines (GitHub Actions for build/test/deploy)
- ✅ Application Insights dashboards showing metrics
- ✅ Security hardened (Key Vault, Managed Identities, HTTPS)

### What's NOT in Scope (Future Extensions)
- ❌ Products/Production/Purchasing domains (focus on Sales + HR only)
- ❌ PWA/Offline capabilities
- ❌ Mobile apps (responsive web only)
- ❌ Advanced search/filtering (basic pagination only)
- ❌ Multi-region deployment (single Azure region)
- ❌ Comprehensive E2E test coverage (only critical user paths)
- ❌ Complex Docker orchestration (Kubernetes, service mesh, etc.)
- ❌ Advanced CI/CD (blue-green deployments, canary releases, etc.)
- ❌ 10 Azure Functions scenarios (only 3 best ones)

---

## Next Steps in Planning Process

**User requested**: Continue planning iteratively through Epics → Features with TDD approach

**Next Phase**: Break down the **10 Epics** with detailed Gherkin acceptance criteria and feature lists:

**Initiative 1 - Frontend (4 Epics)**:
1. Angular Foundation (8-10 features) - Nx, auth, routing, HTTP, components, Tailwind
2. Sales CRUD UI (8-10 features) - Stores, SalesPersons list/create/update with TDD
3. HR CRUD UI (8-10 features) - Employees, Departments list/create/update with TDD
4. Real-Time Dashboard (6-8 features) - SignalR, charts, live updates for Sales & HR

**Initiative 2 - Functions + Infrastructure (6 Epics)**:
5. Testing Foundation (8-10 features) - E2E (Playwright), integration tests, load tests (k6), TDD patterns
6. Docker Basics (4-5 features) - Dockerfiles, docker-compose, local dev environment
7. Document Processing Function (8-10 features) - Blob trigger, Form Recognizer, validation (with tests)
8. Fraud Detection Function (6-8 features) - Event Grid, Durable Function, alerting (with tests)
9. Polyglot Integration Function (5-6 features) - Go/Node.js for external APIs (with tests)
10. Simple IaC + CI/CD (8-10 features) - Basic Bicep, straightforward GitHub Actions

After approval, I'll expand each Epic with full feature breakdowns in Scrum format ("As a... I want... So that...") with Gherkin acceptance criteria ("Given... When... Then...").

---

## Markdown Documentation Structure

All work breakdown documents are created in: `C:\source\mick\AdventureWorks\docs\theBigPlan\`

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

**Document Version**: 1.0
**Last Updated**: 2026-01-17
**Status**: Plan Approved - Ready for Epic Breakdown
