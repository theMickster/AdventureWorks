# Initiative 1: Sales + HR Management Web Application

**Timeline**: Weeks 1-8 (Months 1-2)
**Status**: Not Started
**Owner**: Development Team

---

## Vision Statement

**As a business manager, I want a modern web application to manage stores, sales personnel, employees, and departments, so that I can perform CRUD operations efficiently with a clean, responsive interface and see real-time updates when data changes across Sales and HR domains.**

---

## Success Criteria (Gherkin Format)

**Given** that I am accessing the application as an authenticated user
**When** I use Sales and HR modules
**Then** I should have:

### Authentication & Security
- **Given** I navigate to the application
- **When** I am not authenticated
- **Then** I should be redirected to Microsoft Entra ID login
- **And** after successful authentication, I should be redirected to the dashboard

### Sales Management
- **Given** I am on the Stores page
- **When** I view the list of stores
- **Then** I should see all stores with pagination
- **And** I should be able to create a new store
- **And** I should be able to update an existing store
- **And** I should be able to view store details

- **Given** I am on the Sales Persons page
- **When** I view the list of sales persons
- **Then** I should see all sales persons with pagination
- **And** I should be able to create a new sales person
- **And** I should be able to update an existing sales person
- **And** I should be able to view sales person details

### HR Management
- **Given** I am on the Employees page
- **When** I view the list of employees
- **Then** I should see all employees with pagination
- **And** I should be able to create a new employee
- **And** I should be able to update an existing employee
- **And** I should be able to view employee details

- **Given** I am on the Departments page
- **When** I view the list of departments
- **Then** I should see all departments
- **And** I should be able to create a new department
- **And** I should be able to update an existing department
- **And** I should be able to view department details

### Real-Time Updates
- **Given** I am viewing a list of entities (stores, sales persons, employees, or departments)
- **When** another user creates, updates, or deletes an entity
- **Then** I should see the list update in real-time via SignalR
- **And** I should see a notification indicating what changed

### User Experience
- **Given** I am using the application on any device
- **When** I interact with forms and lists
- **Then** the UI should be responsive (desktop and tablet)
- **And** form validation should provide clear error messages
- **And** loading states should indicate when operations are in progress
- **And** error handling should show user-friendly messages for API failures

### Dashboard
- **Given** I am on the dashboard
- **When** I view the Sales + HR metrics
- **Then** I should see total counts for stores, sales persons, employees, and departments
- **And** I should see visual charts representing the data
- **And** the metrics should update in real-time when data changes

---

## Business Value

### Working Demo
- **Complete vertical slices** for Sales + HR domains prove architecture works end-to-end
- **Tangible deliverable** that stakeholders can see and interact with
- **Proof of concept** for Clean Architecture + CQRS patterns in a real application

### Foundation for Growth
- **Established patterns** for CRUD operations that can be replicated for Products, Production, Purchasing domains
- **Reusable components** (list views, forms, validation) accelerate future development
- **TypeScript interfaces** matching API contracts ensure type safety

### Real-Time Collaboration
- **SignalR integration** demonstrates event-driven capabilities across domains
- **Multi-user scenarios** show how the system handles concurrent updates
- **Push notifications** improve user experience by eliminating manual refreshes

### Modern UX
- **Tailwind CSS + DaisyUI** provides polished, professional look with minimal custom CSS
- **Angular v21 Signals** offer reactive state management without complexity of NgRx
- **Standalone components** follow latest Angular best practices for better tree-shaking

### TDD Practice
- **Building with tests** from the start ensures quality and confidence in changes
- **Playwright E2E tests** validate critical user flows work as expected
- **Unit tests** for components and services catch regressions early

---

## Technical Foundation

### Frontend Framework
- **Angular v21** - Latest version with standalone components
- **TypeScript 5.x** - Strict mode enabled for maximum type safety
- **RxJS 7.x** - Reactive programming for HTTP requests and SignalR streams

### Monorepo Structure
- **Nx workspace** - Single app with shared libraries
- **Feature libraries** - Organized by domain (sales, hr, shared)
- **Smart/Presentational** - Container components vs. UI components

### Styling
- **Tailwind CSS 4.x** - Utility-first CSS framework
- **DaisyUI** - Component library built on Tailwind (faster than building from scratch)
- **Responsive design** - Mobile-first approach with breakpoints

### State Management
- **Angular Signals** - Built-in reactive primitives (simpler than NgRx for this scope)
- **Service-based state** - Domain services hold state for their features
- **No global store** - Avoids complexity of Redux pattern for small app

### HTTP Communication
- **HTTP interceptors** - JWT token injection, correlation IDs, global error handling
- **API service layer** - Type-safe wrapper around HttpClient
- **DTOs/Interfaces** - Matching backend API contracts

### Real-Time
- **@microsoft/signalr** - Official SignalR client library
- **Hub connections** - Separate hubs for Sales and HR domains
- **Automatic reconnection** - Handle disconnects gracefully

### Testing
- **Playwright** - E2E tests for critical user flows only (not comprehensive coverage yet)
- **Jasmine + Karma** - Unit tests for components and services
- **TestBed** - Angular testing utilities for component integration tests

---

## Epics (4 Epics for Initiative 1)

### [Epic 1.1: Angular Foundation](../epics/1.1-angular-foundation.md)
**Timeline**: Weeks 1-2
**Estimated Story Points**: 80-100

**Scope**:
- Nx workspace setup with Angular v21
- Microsoft Entra ID authentication integration
- Routing configuration with guards
- HTTP client with interceptors
- Shared component library (buttons, inputs, modals, etc.)
- Tailwind CSS + DaisyUI configuration
- Basic error handling and loading states

**Key Deliverables**:
- Working Nx monorepo with single Angular app
- Login flow with Microsoft Entra ID
- Protected routes requiring authentication
- Reusable UI components styled with Tailwind
- HTTP service layer with correlation IDs

---

### [Epic 1.2: Sales CRUD UI](../epics/1.2-sales-crud-ui.md)
**Timeline**: Weeks 3-4
**Estimated Story Points**: 100-120

**Scope**:
- Stores list view with pagination
- Store create/update forms with validation
- Store detail view
- Sales Persons list view with pagination
- Sales Person create/update forms with validation
- Sales Person detail view
- TypeScript interfaces matching API DTOs
- Unit tests for all components and services

**Key Deliverables**:
- Complete CRUD for Stores domain
- Complete CRUD for Sales Persons domain
- Form validation matching backend rules
- Loading states and error handling
- Unit test coverage for critical paths

---

### [Epic 1.3: HR CRUD UI](../epics/1.3-hr-crud-ui.md)
**Timeline**: Weeks 5-6
**Estimated Story Points**: 100-120

**Scope**:
- Employees list view with pagination
- Employee create/update forms with validation
- Employee detail view
- Departments list view
- Department create/update forms with validation
- Department detail view
- TypeScript interfaces matching API DTOs
- Unit tests for all components and services

**Key Deliverables**:
- Complete CRUD for Employees domain
- Complete CRUD for Departments domain
- Form validation matching backend rules
- Loading states and error handling
- Unit test coverage for critical paths

---

### [Epic 1.4: Real-Time Dashboard](../epics/1.4-realtime-dashboard.md)
**Timeline**: Weeks 7-8
**Estimated Story Points**: 80-100

**Scope**:
- SignalR hub connection service
- Dashboard with Sales + HR metrics
- Real-time updates when entities change
- Visual charts (bar, pie, line) using chart library
- Toast notifications for real-time events
- Live counters for stores, sales persons, employees, departments

**Key Deliverables**:
- SignalR integration working end-to-end
- Dashboard displaying current metrics
- Real-time updates reflected immediately
- Charts visualizing data trends
- E2E test for real-time scenario

---

## Dependencies

### Backend API (Already Complete ✅)
- Sales endpoints: Stores, SalesPersons, SalesTerritories
- HR endpoints: Employees, Departments, Shifts
- Authentication: Microsoft Entra ID JWT validation
- SignalR hubs: Sales and HR domain hubs (needs implementation)

### External Services
- **Microsoft Entra ID**: Application registration with redirect URIs configured
- **Azure AD App Registration**: Client ID and tenant ID for Angular app

### No Blockers
- Both Sales and HR domains fully implemented in backend
- API already has CORS configured for local development
- Can start frontend implementation immediately

---

## Risks & Mitigations

### Risk: SignalR Hub Not Implemented Yet
- **Mitigation**: Epic 1.4 is scheduled for weeks 7-8, giving time to add SignalR to API
- **Action**: Add SignalR hub implementation to API during weeks 5-6 (during Epic 1.3)

### Risk: Microsoft Entra ID Configuration
- **Mitigation**: API already has Entra ID working; reuse same app registration for frontend
- **Action**: Add SPA redirect URIs to existing app registration

### Risk: Learning Curve for Angular v21 + Signals
- **Mitigation**: Angular v21 is evolution, not revolution; most concepts remain the same
- **Action**: Leverage Claude Code for scaffolding and best practices

### Risk: Tailwind CSS + DaisyUI Unfamiliarity
- **Mitigation**: DaisyUI provides pre-built components with excellent docs
- **Action**: Start with DaisyUI components, customize only when necessary

---

## Definition of Done

An epic is considered "Done" when:

### Functionality
- ✅ All features implemented and working in local dev environment
- ✅ CRUD operations complete for all entities in scope
- ✅ Form validation matches backend API validation rules
- ✅ Error handling displays user-friendly messages

### Testing
- ✅ Unit tests written for all components and services (TDD approach)
- ✅ Unit test coverage > 70% for feature code
- ✅ At least one E2E test covering critical user flow

### Code Quality
- ✅ TypeScript strict mode with no `any` types (except where unavoidable)
- ✅ Linting passes with no warnings (`nx lint`)
- ✅ Code follows Angular style guide
- ✅ No console errors in browser dev tools

### Documentation
- ✅ README updated with setup instructions if needed
- ✅ Component interfaces documented with JSDoc where non-obvious
- ✅ API service layer documented with TypeScript types

### Integration
- ✅ Communicates successfully with backend API
- ✅ Authentication working with Microsoft Entra ID
- ✅ Correlation IDs passed to backend for request tracing

---

## Success Metrics

At the end of Initiative 1 (Week 8), we should have:

### Quantitative
- **4 epics completed** (Angular Foundation, Sales CRUD, HR CRUD, Real-Time Dashboard)
- **30-40 features delivered**
- **60-80 user stories implemented**
- **300-400 story points completed**
- **~70%+ unit test coverage**
- **4-6 E2E tests covering critical flows**

### Qualitative
- **Working Angular application** deployed locally and accessible at http://localhost:4200
- **Authentication flow** working with Microsoft Entra ID
- **Sales + HR CRUD** fully functional with forms and validation
- **Real-time updates** demonstrating SignalR integration
- **Professional UI** using Tailwind CSS + DaisyUI
- **Positive user feedback** from stakeholders on demo

---

## Next Steps

1. **Review Epic 1.1** - Read detailed breakdown of Angular Foundation
2. **Set up development environment** - Install Node.js, Angular CLI, Nx CLI
3. **Start implementation** - Begin with Epic 1.1, Week 1 features
4. **Track progress** - Update epic status as features complete
5. **Demo early and often** - Show progress to stakeholders every 2 weeks

---

**Document Version**: 1.0
**Last Updated**: 2026-01-17
**Status**: Ready for Implementation
