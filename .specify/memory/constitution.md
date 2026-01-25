<!--
SYNC IMPACT REPORT
==================
Version: 0.0.0 → 1.0.0
Change Type: MAJOR - Initial constitution ratification
Date: 2026-01-25

Modified Principles:
- NEW: I. Layered Architecture Pattern
- NEW: II. Security-First Development
- NEW: III. Test-Driven Development (TDD)
- NEW: IV. API Contract Integrity
- NEW: V. Multilingual Support
- NEW: VI. Cloud-Ready Architecture
- NEW: VII. Code Quality & Maintainability

Added Sections:
- Technology Stack Standards
- Development Workflow & Quality Gates

Templates Status:
✅ plan-template.md - Reviewed, compatible (Constitution Check section present)
✅ spec-template.md - Reviewed, compatible (User scenarios and requirements align)
✅ tasks-template.md - Reviewed, compatible (Test-first workflow supported)

Follow-up Actions:
- None - All placeholders filled
- Constitution ready for immediate use

Commit Message:
docs: ratify constitution v1.0.0 (RedCrossManager governance & principles)
-->

# RedCrossManager Constitution

## Core Principles

### I. Layered Architecture Pattern

All features MUST follow the Repository/Service/Controller/DTO pattern:
- **Repository Layer**: Data access logic only, isolated from business rules. Each entity has one repository implementing IRepository<T> interface.
- **Service Layer**: Business logic, orchestration, validation. Services consume repositories, never direct DbContext access.
- **Controller Layer**: HTTP concerns only (routing, request/response mapping). Controllers delegate to services, return DTOs never entities.
- **DTO Layer**: Data Transfer Objects with AutoMapper for entity↔DTO conversion. DTOs define API contracts, entities define database schema.
- **No shortcuts**: Direct DbContext injection into controllers is forbidden. Business logic in controllers is forbidden.

**Rationale**: Separation of concerns enables independent testing of data access, business logic, and API surface. Pattern consistency reduces cognitive load across codebase.

### II. Security-First Development

Security controls are NON-NEGOTIABLE requirements for all features:
- **Authentication**: JWT tokens OR Azure AD integration required for all protected endpoints. Anonymous access MUST be explicitly justified in spec.
- **Authorization**: Role-Based Access Control (RBAC) using ASP.NET Core Identity or Azure AD roles. Volunteers, Coordinators, Administrators as minimum roles.
- **CORS**: Strict origin whitelist for Angular client. Development: localhost:4200. Production: configured domain only. No wildcard (*) allowed.
- **Data Protection**: Sensitive data (personal info, certifications) encrypted at rest. Connection strings in Azure Key Vault or user secrets, never in appsettings.json.
- **Validation**: All DTOs annotated with validation attributes. FluentValidation for complex rules. Input sanitization mandatory.

**Rationale**: Volunteer data is sensitive. Breaches damage trust in humanitarian organization. Security as afterthought is unacceptable.

### III. Test-Driven Development (TDD) (NON-NEGOTIABLE)

Red-Green-Refactor cycle is mandatory for all implementation:
- **Backend**: xUnit or NUnit. Tests written FIRST → User approved → Tests fail → Implementation → Tests pass → Refactor.
- **Frontend**: Jest for unit tests, Jasmine/Karma for Angular components. Same cycle applies.
- **Test Coverage**: Minimum 80% line coverage for services and repositories. Controllers covered via integration tests.
- **Test Types**:
  - Unit tests: Services, repositories, utilities (isolated, mocked dependencies)
  - Integration tests: API endpoints (in-memory database, real HTTP calls)
  - Component tests: Angular components, forms, routing
- **No exceptions**: Feature PRs without passing tests are automatically rejected. No "we'll add tests later."

**Rationale**: TDD ensures correctness, prevents regressions, documents expected behavior. Untested code is broken code awaiting discovery.

### IV. API Contract Integrity

API contracts are binding agreements between backend and frontend:
- **Contract Definition**: All endpoints documented in contracts/ folder with request/response DTOs, status codes, error formats.
- **Contract Testing**: Contract tests verify endpoint signature, status codes, response schema. Written before implementation.
- **Versioning**: API version in route (e.g., /api/v1/volunteers). Breaking changes require new version (v2), deprecated endpoints maintained for 2 releases.
- **Breaking Change Definition**: Property removal, type change, required field addition, endpoint removal. Non-breaking: optional field addition, new endpoint.
- **OpenAPI/Swagger**: All APIs documented via Swashbuckle. Swagger UI enabled in development, disabled in production.

**Rationale**: Frontend depends on stable backend contracts. Contract breaks cause runtime failures. Explicit versioning prevents surprise failures.

### V. Multilingual Support (FR/EN)

Application MUST support French and English equally:
- **Backend**: Resource files (.resx) for validation messages, email templates, error messages. Accept-Language header determines language.
- **Frontend**: ngx-translate with JSON translation files (fr.json, en.json). Language selector in UI. User preference persisted.
- **Content**: All user-facing strings externalized. No hardcoded French or English in code. Default language: French (primary Red Cross Canada language).
- **Validation Messages**: Localized via resource files, never inline strings.

**Rationale**: Red Cross serves diverse communities. Accessibility includes language accessibility. Single-language application excludes volunteers.

### VI. Cloud-Ready Architecture (Azure-Optimized)

Application MUST be deployable to Azure with minimal configuration:
- **Configuration**: Environment-based appsettings (Development, Staging, Production). Secrets in Azure Key Vault, not environment variables.
- **Database**: Connection strings support Azure SQL Database. Migrations runnable in CI/CD pipeline.
- **Storage**: Documents/files use Azure Blob Storage abstraction. Local file system for development only.
- **Logging**: Structured logging (Serilog) with Application Insights sink. Log levels: Development=Debug, Production=Information.
- **Health Checks**: /health endpoint for liveness probe, /health/ready for readiness probe. Checks database, blob storage connectivity.

**Rationale**: On-premise hosting is deprecated. Azure provides scalability, backup, security. Architecture locked to single cloud is acceptable for humanitarian org budget.

### VII. Code Quality & Maintainability

Code quality standards prevent technical debt accumulation:
- **Naming Conventions**: PascalCase for classes/methods (C#), camelCase for properties/variables (TypeScript), descriptive names (no abbreviations except standard: DTO, API, SQL).
- **Code Reviews**: All PRs require approval from one senior developer. Review checklist: tests present, principles followed, no security issues.
- **Linting**: ESLint for Angular (airbnb config), StyleCop for .NET. CI pipeline fails on lint errors.
- **Formatting**: Prettier for TypeScript/HTML/CSS, EditorConfig for .NET. Consistent formatting non-negotiable.
- **Refactoring**: Red-Green-Refactor includes refactoring step. Extract duplicated code, simplify complex methods.
- **YAGNI Principle**: Build what's specified, not what might be needed. Speculative features are prohibited.

**Rationale**: Code is read 10× more than written. Consistency reduces onboarding time. Technical debt compounds interest.

## Technology Stack Standards

These technology choices are fixed for project duration:

**Backend**:
- .NET 10 with C# 12 or later
- ASP.NET Core Web API
- Entity Framework 10 (Code-First approach)
- SQL Server 2019 or Azure SQL Database
- AutoMapper for DTO mapping
- Serilog for structured logging
- xUnit or NUnit for testing

**Frontend**:
- Angular 18 with TypeScript 5+
- Angular Material 18 for UI components
- Tailwind CSS 3.1.0 for utility styling
- RxJS for reactive programming
- ngx-translate for internationalization
- Jest for unit testing

**Deployment**:
- Azure App Service for backend
- Azure Static Web Apps or App Service for frontend
- Azure SQL Database
- Azure Blob Storage for documents
- Azure Key Vault for secrets
- Application Insights for monitoring

**Rationale**: Technology churn is expensive. Standardization enables knowledge sharing, reusable patterns, predictable hiring.

## Development Workflow & Quality Gates

**Branch Strategy**:
- main: Production-ready code only
- develop: Integration branch for features
- Feature branches: feature/###-feature-name (### from issue number)

**Quality Gates** (automated CI checks):
1. All tests pass (unit, integration, contract)
2. Code coverage ≥80% for services/repositories
3. Linting passes (ESLint, StyleCop)
4. Build succeeds for both backend and frontend
5. Security scan passes (OWASP dependency check)

**Definition of Done**:
- Code implemented and reviewed
- Tests written (TDD) and passing
- Documentation updated (README, API docs)
- No lint errors or warnings
- Feature deployed to staging environment
- User acceptance testing completed

**Review Requirements**:
- PRs ≤400 lines of code (split larger changes)
- PR description links to issue and spec
- Constitution compliance verified by reviewer
- No "TODO" or "FIXME" comments in production code

## Governance

This constitution is the supreme governing document for RedCrossManager development. All code, architecture decisions, and processes MUST comply with these principles.

**Amendment Process**:
1. Proposal documented with rationale and impact analysis
2. Review by technical lead and stakeholders
3. If approved, version incremented per semantic versioning:
   - MAJOR: Principle removal, redefinition, or incompatible governance change
   - MINOR: New principle added or significant expansion
   - PATCH: Clarifications, wording improvements, typo fixes
4. Migration plan created for existing code (if applicable)
5. All affected templates and documentation updated

**Enforcement**:
- All PRs MUST pass automated quality gates
- Code reviewers MUST verify constitution compliance
- Principle violations MUST be justified in writing or rejected
- Unjustified complexity MUST be simplified or rejected

**Runtime Guidance**:
- Developers and AI agents MUST consult this constitution before implementation
- When specifications conflict with constitution, constitution takes precedence
- Ambiguities resolved in favor of principles (e.g., if TDD unclear, write tests first)

**Version**: 1.0.0 | **Ratified**: 2026-01-25 | **Last Amended**: 2026-01-25
