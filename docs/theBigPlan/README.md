# AdventureWorks - The Big Plan

**4-Month Implementation Plan** for building a modern, production-ready enterprise application with Angular v21, .NET 10.0 API, and Azure Functions microservices.

## Quick Links

- [Complete Implementation Plan](TheBigPlan.md) - Full 4-month roadmap with all details

### Initiatives

- [Initiative 1: Sales + HR Management Web Application](initiatives/01-sales-hr-web-application.md)
- [Initiative 2: Event-Driven Microservices + Production Infrastructure](initiatives/02-event-driven-microservices-infrastructure.md)

### Epics - Initiative 1 (Frontend)

1. [Epic 1.1: Angular Foundation](epics/1.1-angular-foundation.md) - Weeks 1-2
2. [Epic 1.2: Sales CRUD UI](epics/1.2-sales-crud-ui.md) - Weeks 3-4
3. [Epic 1.3: HR CRUD UI](epics/1.3-hr-crud-ui.md) - Weeks 5-6
4. [Epic 1.4: Real-Time Dashboard](epics/1.4-realtime-dashboard.md) - Weeks 7-8

### Epics - Initiative 2 (Functions + Infrastructure)

5. [Epic 2.1: Testing Foundation](epics/2.1-testing-foundation.md) - Week 9
6. [Epic 2.2: Docker Basics](epics/2.2-docker-basics.md) - Week 10
7. [Epic 2.3: Intelligent Document Processing Function](epics/2.3-document-processing-function.md) - Weeks 11-12
8. [Epic 2.4: Real-Time Fraud Detection Function](epics/2.4-fraud-detection-function.md) - Week 13
9. [Epic 2.5: Polyglot Integration Function](epics/2.5-polyglot-integration-function.md) - Week 14
10. [Epic 2.6: Simple IaC + CI/CD](epics/2.6-simple-iac-cicd.md) - Weeks 15-16

### Templates

- [Epic Template](templates/epic-template.md) - Template for creating new epics
- [Feature Template](templates/feature-template.md) - Template for creating new features

---

## Project Overview

### Vision

Build a modern, production-ready AdventureWorks showcase demonstrating:
- Angular v21 with standalone components and Signals
- .NET 10.0 API with Clean Architecture and CQRS
- Azure Functions microservices (polyglot)
- Event-driven architecture with Event Grid + Service Bus
- Docker containerization and Azure deployment
- TDD approach with comprehensive testing

### Scope Philosophy

**Focus on quality over quantity**: Complete Sales + HR domains end-to-end with 3 impressive Azure Functions, rather than shallow coverage of many domains.

### Current Status

- ✅ Backend API: Sales, HR, Address Management fully implemented
- ❌ Frontend: Empty directory - need Angular skeleton + Sales UI
- ❌ Azure Functions: Empty directories - need 3 showcase scenarios
- ⚠️ Infrastructure: Need Docker + Bicep IaC

---

## 4-Month Timeline

### Month 1 - Foundation
**Weeks 1-4**: Angular skeleton, authentication, Sales CRUD UI, Docker basics

### Month 2 - Vertical Integration
**Weeks 5-8**: HR CRUD UI, Real-Time Dashboard with SignalR, first Azure Function (Document OCR)

### Month 3 - Event-Driven Showcase
**Weeks 9-12**: Testing foundation, Docker containerization, Document Processing Function complete

### Month 4 - Production Hardening
**Weeks 13-16**: Fraud Detection Function, Polyglot Integration, IaC + CI/CD, monitoring

---

## Success Criteria

### Must Have (Done = These Complete)

- ✅ Angular app with Sales + HR management UI
- ✅ Microsoft Entra ID authentication working
- ✅ 3 Azure Functions (Document OCR, Fraud Detection, Integration)
- ✅ Real-time SignalR updates
- ✅ Solid test coverage (E2E, integration, unit, load)
- ✅ Docker containerization (docker-compose)
- ✅ Azure deployment via Bicep IaC
- ✅ CI/CD pipelines (GitHub Actions)
- ✅ Application Insights monitoring
- ✅ Security hardened (Key Vault, Managed Identities)

### Not in Scope (Future Extensions)

- ❌ Products/Production/Purchasing domains
- ❌ PWA/Offline capabilities
- ❌ Mobile apps
- ❌ Advanced search/filtering
- ❌ Multi-region deployment
- ❌ Kubernetes orchestration
- ❌ Blue-green/canary deployments

---

## Estimated Effort

- **Total**: 700-900 story points
- **Timeline**: 16 weeks (4 months)
- **Approach**: Solo developer + Claude Code with TDD
- **Epics**: 10 epics (4 frontend, 6 backend/infra)
- **Features**: ~70-90 features
- **User Stories**: ~140-180 user stories

---

## Technology Stack

### Frontend
- Angular v21 (standalone components, Signals)
- Nx workspace monorepo
- Tailwind CSS + DaisyUI
- @microsoft/signalr
- Playwright (E2E testing)

### Backend
- .NET 10.0 REST API
- Entity Framework Core
- MediatR (CQRS)
- FluentValidation
- xUnit + Moq

### Azure Functions
- .NET 8 Functions
- Go Functions (custom handlers)
- Durable Functions
- Azure Form Recognizer
- Event Grid + Service Bus

### Infrastructure
- Docker + docker-compose
- Azure Bicep
- GitHub Actions
- Application Insights
- Azure Key Vault

---

## Getting Started

1. **Review**: Read [TheBigPlan.md](TheBigPlan.md) for complete context
2. **Choose**: Select an Epic to work on (start with 1.1)
3. **Plan**: Review Epic's features and acceptance criteria
4. **Build**: Implement features using TDD approach
5. **Test**: Ensure all tests pass before moving on
6. **Track**: Update Epic status as features complete

---

**Document Version**: 1.0
**Last Updated**: 2026-01-17
**Status**: Plan Approved - Ready for Implementation
