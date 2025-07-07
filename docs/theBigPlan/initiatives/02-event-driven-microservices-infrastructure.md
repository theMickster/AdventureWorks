# Initiative 2: Event-Driven Microservices + Production Infrastructure

**Timeline**: Weeks 9-16 (Months 3-4)
**Status**: Not Started
**Owner**: Development Team

---

## Vision Statement

**As a solution architect, I want to demonstrate impressive Azure Functions capabilities with 2-3 showcase scenarios and production-ready infrastructure, so that the application highlights event-driven architecture, polyglot microservices, and cloud-native operations within a 4-month timeline.**

---

## Success Criteria (Gherkin Format)

**Given** that we're showcasing Azure capabilities with limited time
**When** I complete this initiative
**Then** the system should demonstrate:

### Azure Functions Showcase
- **Given** I have uploaded a store registration PDF document to Blob Storage
- **When** the Document Processing Function triggers
- **Then** the function should extract data using Azure Form Recognizer
- **And** validate the extracted data
- **And** create a Store record via API call
- **And** send a SignalR notification to the frontend with results
- **And** complete within 5-10 seconds

- **Given** a new order is created in the system
- **When** the Fraud Detection Function receives the OrderCreated event
- **Then** the Durable Function should orchestrate a validation workflow
- **And** a Go (or Node.js) function should perform high-speed rule checks
- **And** if flagged, send a Teams/Slack webhook notification
- **And** store results with correlation ID for audit

- **Given** it's 8am daily
- **When** the Scheduled Reporting Function triggers
- **Then** the function should query the API for sales metrics
- **And** generate a PDF/Excel report
- **And** email the report via SendGrid
- **And** store the report in Blob Storage

### Polyglot Microservices
- **Given** I need high-performance data processing
- **When** I implement a processing function
- **Then** I should have at least 1 non-.NET function (Go or Node.js)
- **And** it should integrate seamlessly with .NET functions

### Event-Driven Messaging
- **Given** events are published from the API or functions
- **When** subscribers listen for events
- **Then** Event Grid should deliver pub/sub events
- **And** Service Bus should handle reliable queue-based messaging
- **And** all messages should include correlation IDs

### Containerization
- **Given** I want to run the application locally
- **When** I use Docker
- **Then** I should have Dockerfiles for API and Frontend
- **And** I should have a docker-compose file for local development
- **And** all services should start with a single `docker-compose up` command

### Infrastructure as Code
- **Given** I want to deploy to Azure
- **When** I use Bicep templates
- **Then** the templates should create all required Azure resources
- **And** templates should be parameterized for dev/prod environments
- **And** deployment should be idempotent (can run multiple times)

### CI/CD Pipelines
- **Given** I push code to the main branch
- **When** GitHub Actions workflows trigger
- **Then** the code should be built and tested
- **And** if tests pass, it should deploy to Azure
- **And** deployment should use secrets from Key Vault

### Monitoring & Observability
- **Given** the application is running in Azure
- **When** I view Application Insights
- **Then** I should see correlation IDs for all requests
- **And** I should see custom metrics for business events
- **And** I should have dashboards visualizing key metrics
- **And** I should have alerts for critical failures

### Security Hardening
- **Given** the application is deployed to Azure
- **When** I review security posture
- **Then** all services should use Managed Identities
- **And** all secrets should be stored in Key Vault
- **And** all endpoints should use HTTPS
- **And** the attack surface should be minimized

---

## Business Value

### Showcase Excellence
- **Impressive Azure Functions** demos attract interest from stakeholders and potential clients
- **Real-world scenarios** (OCR, fraud detection, reporting) demonstrate practical business value
- **Event-driven architecture** shows modern, scalable design patterns

### Production-Ready
- **IaC + CI/CD** enables reliable, repeatable deployments to any environment
- **Monitoring dashboards** provide visibility into system health and performance
- **Security hardening** ensures compliance with enterprise standards

### Extensibility
- **Event-driven foundation** makes adding new features easy without coupling services
- **Polyglot architecture** allows choosing the best language/runtime for each task
- **Microservices pattern** enables independent scaling and deployment

### Learning
- **Best practices** demonstrated across containerization, IaC, CI/CD, monitoring
- **Polyglot architecture** shows how to integrate multiple languages/runtimes
- **Cloud-native patterns** applicable to any Azure-based project

### Cost-Effective
- **Serverless functions** only cost when running (pay-per-execution model)
- **Docker containers** optimize resource usage with multi-stage builds
- **Azure consumption-based pricing** keeps costs low for low-traffic scenarios

---

## Technical Foundation

### .NET Azure Functions
- **.NET 8** - LTS runtime for Azure Functions
- **Durable Functions** - Orchestration workflows for complex scenarios
- **HTTP triggers** - Expose functions as REST endpoints
- **Service Bus triggers** - Process messages from queues
- **Timer triggers** - Scheduled execution (cron-like)
- **Blob triggers** - React to file uploads

### Go Functions
- **Go 1.21+** - High-performance data processing
- **Custom handlers** - Azure Functions custom handler for Go
- **Fast execution** - Ideal for CPU-intensive rule checks
- **Alternative**: Node.js 20 for external API integrations

### Messaging
- **Azure Service Bus** - Reliable queue-based messaging with dead-letter queues
- **Azure Event Grid** - Pub/sub event distribution with filtering
- **Topics & Subscriptions** - Multiple consumers for same events

### IaC (Infrastructure as Code)
- **Azure Bicep** - Declarative infrastructure templates
- **Modular design** - Reusable modules for common resources
- **Parameterized** - Different values for dev/prod environments
- **Idempotent** - Safe to run multiple times

### Containers
- **Docker** - Containerization platform
- **Multi-stage builds** - Smaller final images
- **docker-compose** - Local development environment
- **Azure Container Registry** - Private registry for images

### CI/CD
- **GitHub Actions** - Workflow automation
- **Secrets from Key Vault** - No secrets in code
- **Matrix builds** - Test multiple configurations
- **Deployment slots** - Blue-green deployments (future)

### Monitoring
- **Application Insights** - Distributed tracing, logs, metrics
- **Custom metrics** - Business event tracking
- **Correlation IDs** - Trace requests across services
- **Dashboards** - Visualize key metrics
- **Alerts** - Notify on critical failures

---

## Epics (6 Epics for Initiative 2)

### [Epic 2.1: Testing Foundation](../epics/2.1-testing-foundation.md)
**Timeline**: Week 9
**Estimated Story Points**: 60-80

**Scope**:
- Unit test patterns for TDD approach
- API integration tests using WebApplicationFactory
- E2E test framework with Playwright
- Load testing setup with k6
- Test data builders and fixtures
- CI integration for test execution

**Key Deliverables**:
- TDD patterns documented and demonstrated
- Integration tests for critical API endpoints
- E2E tests for critical user flows
- Load test scenarios for API performance
- All tests running in CI pipeline

---

### [Epic 2.2: Docker Basics](../epics/2.2-docker-basics.md)
**Timeline**: Week 10
**Estimated Story Points**: 40-50

**Scope**:
- Dockerfile for .NET API (multi-stage build)
- Dockerfile for Angular frontend (nginx)
- docker-compose for local development
- Environment variable configuration
- Volume mounts for local development
- Health checks for services

**Key Deliverables**:
- Working Dockerfiles for API and Frontend
- docker-compose that starts entire stack
- Documentation for local Docker development
- Optimized image sizes with multi-stage builds

---

### [Epic 2.3: Intelligent Document Processing Function](../epics/2.3-document-processing-function.md)
**Timeline**: Weeks 11-12
**Estimated Story Points**: 100-120

**Scope**:
- Blob Storage trigger when PDF uploaded
- Azure Form Recognizer integration for OCR
- Data extraction and validation logic
- API call to create Store record
- SignalR notification to frontend
- Error handling and retry logic
- Unit tests for all components
- Integration test with mock Form Recognizer

**Key Deliverables**:
- Working Document Processing Function
- End-to-end OCR pipeline (upload → extract → validate → store)
- Real-time progress updates via SignalR
- Comprehensive test coverage
- Documentation of OCR patterns

---

### [Epic 2.4: Real-Time Fraud Detection Function](../epics/2.4-fraud-detection-function.md)
**Timeline**: Week 13
**Estimated Story Points**: 80-100

**Scope**:
- Event Grid subscription for OrderCreated events
- .NET Durable Function orchestration
- Go (or Node.js) function for rule checks
- Teams/Slack webhook integration
- Correlation ID tracking for audit
- Unit tests for orchestration and rules
- Integration test for event flow

**Key Deliverables**:
- Working Fraud Detection Function
- Durable Function orchestration workflow
- Polyglot integration (Go or Node.js)
- External webhook notifications
- Test coverage for critical paths

---

### [Epic 2.5: Polyglot Integration Function](../epics/2.5-polyglot-integration-function.md)
**Timeline**: Week 14
**Estimated Story Points**: 60-70

**Scope**:
- Go or Node.js function for external API integration
- HTTP trigger for RESTful interface
- External API communication (example: weather API, stock API)
- Error handling and retry logic
- Logging with correlation IDs
- Unit tests for function logic
- Integration test with mock external API

**Key Deliverables**:
- Working polyglot function (Go or Node.js)
- External API integration pattern
- Resilient error handling
- Documentation for polyglot functions

---

### [Epic 2.6: Simple IaC + CI/CD](../epics/2.6-simple-iac-cicd.md)
**Timeline**: Weeks 15-16
**Estimated Story Points**: 100-120

**Scope**:
- Bicep modules for all Azure resources
  - App Service (API)
  - Static Web App (Frontend)
  - Function Apps (3 functions)
  - SQL Database
  - Key Vault
  - Application Insights
  - Storage Account
  - Service Bus
  - Event Grid
- GitHub Actions workflows
  - Build and test API
  - Build and test Frontend
  - Build and test Functions
  - Deploy to Azure
- Secret management with Key Vault
- Environment-specific parameters (dev/prod)

**Key Deliverables**:
- Complete Bicep templates for all resources
- GitHub Actions workflows for CI/CD
- Successful deployment to Azure
- Documentation for deployment process

---

## "Wow Factor" Function Scenarios (3 Scenarios)

### Scenario 1: Intelligent Document Processing (Primary Showcase)

**User Story**:
**As a sales manager, I want to upload store registration documents (PDFs), so that the system automatically extracts data and creates store records without manual entry.**

**Technical Stack**:
- Azure Blob Storage trigger when PDF uploaded
- .NET Function calls Azure Form Recognizer for OCR
- Validation logic checks extracted data
- Creates Store record via API call
- SignalR notification to frontend with results
- Complete in ~5-10 seconds with progress updates

**Why This**:
- Shows Azure AI services integration
- Demonstrates event-driven trigger pattern
- External service integration (Form Recognizer)
- Real-time updates to frontend
- Practical business value (save manual data entry time)

**Demo Flow**:
1. User uploads PDF in Angular app
2. PDF saved to Blob Storage
3. Function triggers automatically
4. Form Recognizer extracts data
5. Function validates and creates Store
6. User sees success notification in real-time

---

### Scenario 2: Real-Time Transaction Monitoring (Secondary Showcase)

**User Story**:
**As a security analyst, I want suspicious transactions flagged in real-time, so that fraud is prevented before financial loss.**

**Technical Stack**:
- Event Grid publishes "OrderCreated" events
- .NET Durable Function orchestrates validation workflow
- Go function (or Node.js) performs high-speed rule checks
- If flagged: Teams/Slack webhook notification
- Results stored with correlation ID for audit

**Why This**:
- Shows Durable Functions orchestration
- Demonstrates polyglot architecture (Go + .NET)
- Real-time alerting with external webhooks
- Event-driven pub/sub pattern
- Business-critical scenario (fraud prevention)

**Demo Flow**:
1. Order created in system
2. Event Grid publishes OrderCreated event
3. Durable Function starts orchestration
4. Go function performs rule checks
5. If suspicious: webhook sent to Teams/Slack
6. Results logged for audit

---

### Scenario 3: Scheduled Reporting & Data Export (Tertiary Showcase)

**User Story**:
**As a manager, I want daily sales reports emailed automatically, so that I stay informed without manual work.**

**Technical Stack**:
- Timer-triggered function runs daily at 8am
- Query API for sales metrics
- Generate PDF/Excel report
- Email via SendGrid binding
- Store report in Blob Storage

**Why This**:
- Shows timer triggers (scheduled execution)
- Demonstrates bindings (SendGrid, Blob Storage)
- Scheduled automation pattern
- External service integration (SendGrid)
- Practical business value (automated reporting)

**Demo Flow**:
1. Timer trigger fires at 8am daily
2. Function queries API for sales data
3. Report generated (PDF or Excel)
4. Email sent via SendGrid
5. Report saved to Blob Storage
6. Manager receives email with report

---

## Dependencies

### Frontend + API (From Initiative 1)
- Angular app must be complete for end-to-end scenarios
- API must be deployed for functions to call
- SignalR hubs must be implemented for real-time updates

### Azure Resources
- **Azure subscription** with sufficient credits for Functions, Storage, Form Recognizer
- **Azure Form Recognizer** service created (for Document Processing)
- **SendGrid account** or Azure Communication Services (for email)
- **Teams/Slack webhook URL** (for fraud detection alerts)

### External Services
- **GitHub repository** with Actions enabled
- **Docker Desktop** installed for local development
- **Azure CLI** installed for deployment

---

## Risks & Mitigations

### Risk: Azure Form Recognizer Costs
- **Mitigation**: Use pre-built models (cheaper than custom training)
- **Action**: Test with small batch of documents to estimate costs

### Risk: Go Functions Unfamiliarity
- **Mitigation**: Go has excellent docs; custom handlers well-documented
- **Alternative**: Use Node.js if Go proves too challenging
- **Action**: Spike Epic 2.5 early if uncertain

### Risk: Bicep Learning Curve
- **Mitigation**: Bicep is simpler than ARM templates
- **Action**: Start with simple resources, build complexity gradually
- **Fallback**: Azure Portal can export resources as Bicep templates

### Risk: CI/CD Pipeline Complexity
- **Mitigation**: Start simple, add complexity incrementally
- **Action**: Use GitHub Actions marketplace for pre-built actions
- **Scope**: Avoid advanced patterns (blue-green, canary) for v1

### Risk: Timeline Pressure in Month 4
- **Mitigation**: Prioritize core functions, deprioritize "nice-to-haves"
- **Action**: Epic 2.5 (Polyglot Integration) can be descoped if needed
- **Buffer**: Week 16 has buffer time for overruns

---

## Definition of Done

An epic is considered "Done" when:

### Functionality
- ✅ All features implemented and working in local dev environment
- ✅ Azure Functions deployed and operational in Azure
- ✅ Event-driven workflows complete end-to-end
- ✅ Error handling gracefully manages failures

### Testing
- ✅ Unit tests written for all function logic (TDD approach)
- ✅ Integration tests validate function triggers and outputs
- ✅ E2E test demonstrates full user flow
- ✅ Load tests verify performance under expected load

### Infrastructure
- ✅ Bicep templates deploy all required resources
- ✅ CI/CD pipelines build, test, and deploy automatically
- ✅ Secrets managed via Key Vault (no secrets in code)
- ✅ Docker containers build and run successfully

### Monitoring
- ✅ Application Insights capturing logs and metrics
- ✅ Correlation IDs present in all requests
- ✅ Dashboard visualizing key metrics
- ✅ Alerts configured for critical failures

### Documentation
- ✅ README updated with deployment instructions
- ✅ Architecture diagrams showing event flows
- ✅ Runbooks for common operations (deploy, rollback, troubleshoot)

---

## Success Metrics

At the end of Initiative 2 (Week 16), we should have:

### Quantitative
- **6 epics completed** (Testing, Docker, 3 Functions, IaC/CI/CD)
- **40-50 features delivered**
- **80-100 user stories implemented**
- **400-500 story points completed**
- **3 Azure Functions** deployed and operational
- **~70%+ test coverage** across all functions

### Qualitative
- **Working Azure Functions** demonstrating event-driven patterns
- **Polyglot architecture** with Go (or Node.js) integration
- **Docker containerization** for local development
- **Azure deployment** via Bicep IaC
- **CI/CD pipelines** automating build, test, deploy
- **Application Insights** dashboards showing metrics
- **Positive stakeholder feedback** on impressive capabilities

---

## Next Steps

1. **Complete Initiative 1** - Frontend must be done before starting Initiative 2
2. **Review Epic 2.1** - Read detailed breakdown of Testing Foundation
3. **Set up Azure subscription** - Ensure credits available for Functions, Storage, etc.
4. **Install Docker Desktop** - Required for local development
5. **Start implementation** - Begin with Epic 2.1, Week 9 features
6. **Track progress** - Update epic status as features complete

---

**Document Version**: 1.0
**Last Updated**: 2026-01-17
**Status**: Ready for Implementation (After Initiative 1 Complete)
