# RedCrossManager

Volunteer Onboarding & Management System for the Red Cross — end-to-end flow from registration to activation, training, missions, document verification, and youth (B1J) communications.

## Overview
- Guided onboarding stepper (Documents → Orientation → Certification → Final Review)
- Parental/guardian consent gate for minors (email verification)
- Volunteer dashboard (status, trainings, certifications, assignments, alerts)
- Training management (publish, enroll, attendance, certificates)
- Mission assignment (qualification filter, notifications, reminders)
- Document management (upload, verify, expiry alerts, blocking rules)
- B1J communications (email required; SMS optional with explicit opt-in)

## Tech Stack
- Backend: .NET 10 Web API, EF Core 10 (SQL Server), AutoMapper, FluentValidation, JWT (Azure AD optional), Serilog + App Insights
- Frontend: Angular 18, Angular Material 18, Tailwind CSS 3.1.0, RxJS, ngx-translate (FR/EN)
- Cloud/Infra: Azure App Service, Azure SQL, Azure Blob Storage, Azure Key Vault

## Repository Layout
```
specs/001-volunteer-onboarding/
├── plan.md            # Implementation plan
├── spec.md            # Feature specification
├── research.md        # Design research
├── data-model.md      # Entities & relationships
├── quickstart.md      # Prereqs & setup notes
├── contracts/         # API contracts (v1)
└── tasks.md           # Execution tasks

RedCrossManager.Server/   # Backend (to be scaffolded per tasks)
RedCrossManager.Client/   # Frontend (to be scaffolded per tasks)
```

## Quickstart
- Read specs/001-volunteer-onboarding/quickstart.md for prerequisites and environment setup.
- Verify prerequisites with the project script:

```powershell
.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks
```

## Development Workflow
- Constitution-driven: repository/service/controller/DTO layering; security-first; TDD; API contract integrity; FR/EN localization; Azure-ready; code quality gates.
- Contracts versioned at `/api/v1`; Swagger enabled in development.
- Tests-first per tasks in specs/001-volunteer-onboarding/tasks.md.

## Git Hooks (Format/Lint)
Pre-commit hooks run frontend formatting and backend `dotnet format` checks.

1) Install root dev dependencies:

```powershell
npm install
```

2) Install frontend dependencies (if needed):

```powershell
npm install --prefix RedCrossManager.Client
```

Hooks are installed via the root `prepare` script and run automatically on `git commit`.

## Links
- Feature Spec: specs/001-volunteer-onboarding/spec.md
- Plan: specs/001-volunteer-onboarding/plan.md
- Data Model: specs/001-volunteer-onboarding/data-model.md
- API Contracts: specs/001-volunteer-onboarding/contracts/api.md
- Tasks: specs/001-volunteer-onboarding/tasks.md

## Status
- Docs aligned to .NET/EF 10; tasks generated and refined.
- Code scaffolding to be implemented per tasks.

---
Maintainers: RedCrossManager Team
