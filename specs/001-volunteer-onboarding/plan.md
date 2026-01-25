# Implementation Plan: Volunteer Onboarding & Management System

**Branch**: `001-volunteer-onboarding` | **Date**: 2026-01-25 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-volunteer-onboarding/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Build a full-stack volunteer onboarding platform with guided onboarding stepper (documents, orientation, certifications, final review), parental consent flow for minors, training management, mission assignment, document verification, dashboards, and B1J communications (email required, SMS optional with opt-in). Backend: .NET 10 Web API + EF Core 10 (SQL Server) with Repository/Service/Controller/DTO/AutoMapper; JWT auth with Azure AD option; CORS for Angular. Frontend: Angular 18 + Material 18 (stepper, tables) + Tailwind for utilities, HttpClient + routing. Contracts versioned `/api/v1`. Migrations for SQL Server. Localization FR/EN. Structured logging and health checks for Azure readiness.

## Technical Context

**Language/Version**: Backend C# / .NET 10; Frontend Angular 18 (TypeScript 5+)  
**Primary Dependencies**: ASP.NET Core Web API, EF Core 10 (SQL Server), AutoMapper, FluentValidation, JWT auth with optional Azure AD, Serilog + Application Insights sink; Angular 18, Angular Material 18, Tailwind 3.1.0, RxJS, ngx-translate  
**Storage**: SQL Server (code-first migrations), Azure Blob Storage for documents (dev: local file system)  
**Testing**: Backend xUnit (or NUnit) + integration tests (WebApplicationFactory, Testcontainers SQL); Frontend Jest + Angular Testing Library; Contract tests for APIs  
**Target Platform**: Azure App Service (API), Azure SQL Database, Azure Blob Storage, Azure Static Web Apps/App Service for frontend  
**Project Type**: Web (separate backend/frontend projects)  
**Performance Goals**: p95 < 3s dashboard load; uploads ≤10MB in <30s; notifications sent <10m; onboarding completion flow responsive <1.5s per step  
**Constraints**: PII protection (encryption at rest, secrets in Key Vault), RBAC, CORS locked to Angular origins, WCAG 2.1 AA, minors’ parental consent gate, SMS only with opt-in  
**Scale/Scope**: ~500 concurrent volunteers during peaks; growth path to a few thousand active volunteers; 7 primary user stories + 51 functional requirements

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Layered architecture (Repository/Service/Controller/DTO) ✅ planned
- Security-first (JWT/Azure AD, RBAC, CORS whitelist, secrets in Key Vault, validation) ✅ planned
- TDD (tests first, coverage ≥80% for services/repos) ✅ planned
- API contract integrity (versioned /api/v1, OpenAPI, contract tests) ✅ planned
- Multilingual FR/EN (backend resources, frontend ngx-translate) ✅ planned
- Cloud-ready Azure (App Service, SQL, Blob, Key Vault, App Insights, health checks) ✅ planned
- Code quality (StyleCop, ESLint/Prettier, reviews) ✅ planned
- Version standard: Constitution mandates .NET 10; using .NET 10 (no violation).

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
RedCrossManager.Server/
├── Controllers/
├── DTOs/
├── Services/
├── Repositories/
├── Domain/Entities/
├── Infrastructure/ (EF configs, Migrations, Blob adapter)
├── Contracts/ (OpenAPI, contract tests)
└── Tests/
  ├── Unit/
  ├── Integration/
  └── Contract/

RedCrossManager.Client/
├── src/
│   ├── app/
│   │   ├── core/ (auth, http, guards, interceptors)
│   │   ├── features/
│   │   │   ├── onboarding/ (stepper, consent upload)
│   │   │   ├── trainings/
│   │   │   ├── missions/
│   │   │   ├── documents/
│   │   │   └── b1j-comms/
│   │   ├── shared/ (components, directives, pipes, validators)
│   │   └── i18n/ (fr.json, en.json)
│   ├── assets/
│   └── styles/ (Tailwind, theme)
└── tests/
  ├── unit/
  └── component/
```

**Structure Decision**: Web application with separate backend and frontend projects as above.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
