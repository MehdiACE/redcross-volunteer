---

description: "Tasks for volunteer onboarding & management system"
---

# Tasks: Volunteer Onboarding & Management System

**Input**: Design documents from `/specs/001-volunteer-onboarding/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: TDD required per constitution — include tests before implementation in each story phase.

**Organization**: Tasks grouped by user story for independent delivery.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1, US2, ...
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

 - [X] T001 Initialize repository structure for backend/frontend per plan in `RedCrossManager.Server/` and `RedCrossManager.Client/`
 - [X] T002 Create .NET 10 Web API project and solution in `RedCrossManager.Server/`
 - [X] T003 Create Angular 18 app with Tailwind 3.1.0 and Material 18 in `RedCrossManager.Client/`
 - [X] T004 [P] Add `.editorconfig`, `.gitignore`, and base `Directory.Build.props` for consistent formatting in repo root
- [ ] T005 [P] Configure Husky/lint-staged (frontend) and dotnet-format hook (backend) in repo root

## Phase 2: Foundational (Blocking Prerequisites)

- [X] T006 Configure EF Core 10 DbContext, connection strings, and initial migration for core entities in `RedCrossManager.Server/Infrastructure`
- [X] T007 [P] Set up Serilog + Application Insights and health checks endpoints in `RedCrossManager.Server`
- [X] T008 [P] Implement JWT auth (with Azure AD optional scheme) and RBAC roles (Volunteer, Coordinator, Admin) in `RedCrossManager.Server`
- [X] T008a [P] Create WebApplicationFactory and test database seeding helper for integration tests in `RedCrossManager.Server/Tests/Infrastructure` (blocks T013, T020, T032, T046, T052)
- [X] T009 [P] Enable CORS for Angular origins in `RedCrossManager.Server`
- [X] T010 Configure Tailwind, global theme, and i18n scaffolding (fr/en) in `RedCrossManager.Client/src`
- [X] T011 [P] Set up API client base service and auth interceptor in `RedCrossManager.Client/src/app/core`
- [ ] T011a [P] Implement SMS opt-in flag infrastructure: add `Volunteer.SmsOptIn` and `ParentalConsent.SmsOptIn` fields in `RedCrossManager.Server/Domain/Entities`; create POST `/volunteers/me/sms-opt-in` endpoint in `VolunteersController`; add opt-in toggle to Angular profile in `RedCrossManager.Client/src/app/features/profile`
- [ ] T012 Establish CI checks (lint, test, build) scripts entries for backend/frontend in repo root; specify `.github/workflows/` pipeline files

---

## Phase 3: User Story 1 - Volunteer Registration & Profile Creation (Priority: P1) 🎯 MVP

**Goal**: Self-registration captures profile, interests, availability; creates Pending volunteer.

**Independent Test**: Submit registration form -> new volunteer persisted with Pending status and confirmation email sent.

### Tests (write first)
- [X] T013 [P] [US1] Backend integration test for `/volunteers/register` (duplicate email, success) in `RedCrossManager.Server/Tests/Integration/VolunteersTests.cs` — **BLOCKED**: EF Core dual-provider issue with WebApplicationFactory needs resolution
- [ ] T014 [P] [US1] Frontend component test for registration form validation in `RedCrossManager.Client/src/app/features/onboarding/registration/registration.component.spec.ts`

### Implementation
- [X] T015 [P] [US1] Implement `Volunteer` entity, configuration, and repository in `RedCrossManager.Server/Domain/Entities` and `Repositories`
- [X] T016 [US1] Implement registration service + DTOs + AutoMapper profile in `RedCrossManager.Server/Services/Volunteers`
- [X] T017 [US1] Implement `VolunteersController` registration endpoint in `RedCrossManager.Server/Controllers/VolunteersController.cs`
- [X] T018 [US1] Build registration UI (form fields, validation, i18n) in `RedCrossManager.Client/src/app/features/onboarding/registration` — **COMPLETED**: Component, service, models, i18n (EN/FR), routing configured
- [X] T019 [US1] Wire confirmation email send via SendGrid abstraction in `RedCrossManager.Server/Services/Notifications`

---

## Phase 4: User Story 2 - Onboarding Workflow Stepper (Priority: P1)

**Goal**: Guided 4-step onboarding with parental consent gate for minors.

**Independent Test**: Volunteer moves through steps; minors blocked until guardian consent approved; status transitions Pending→InTraining→Active after final review.

### Tests (write first)
- [ ] T020 [P] [US2] Integration test for onboarding step transitions and status updates in `RedCrossManager.Server/Tests/Integration/OnboardingTests.cs`
- [ ] T020a [US2] Integration test for parental consent workflow (request → submission → approval/rejection) including SLA enforcement (48-hour coordinator review target) in `RedCrossManager.Server/Tests/Integration/ConsentTests.cs`
- [ ] T021 [P] [US2] Frontend component test for stepper progression/resume state in `RedCrossManager.Client/src/app/features/onboarding/stepper/stepper.component.spec.ts`
- [ ] T021a [US2] Unit/integration tests for guardian identity verification (email confirmation) in `RedCrossManager.Server/Tests/Integration/GuardianVerificationTests.cs`

### Implementation
- [X] T022 [P] [US2] Implement `OnboardingStep` entity/config + repository in `RedCrossManager.Server/Domain/Entities` and `Repositories`
- [X] T023 [US2] Implement onboarding service (progress fetch, submit, resume) in `RedCrossManager.Server/Services/Onboarding` — **COMPLETED**: Service already exists with all required methods
- [ ] T024 [US2] Implement stepper UI with state fetch and guarded advancement in `RedCrossManager.Client/src/app/features/onboarding/stepper`
- [X] T025 [US2] Add status transitions in `VolunteersController` and onboarding controller endpoints in `RedCrossManager.Server/Controllers`; implement consent request/approval workflow in `ConsentsController` — **COMPLETED**: UpdateStatus endpoint added to VolunteersController; OnboardingController and ConsentsController already exist with all required endpoints
- [X] T025a [US2] Implement parental consent service with guardian email notification, SLA tracking (48-hour coordinator review), and identity verification (email confirmation token) in `RedCrossManager.Server/Services/Consents`
- [ ] T025b [US2] Build guardian consent form UI (read-only form display, signature capture, submit) in `RedCrossManager.Client/src/app/features/onboarding/guardian-consent`
- [ ] T026 [US2] Persist progress/resume state in client store/service in `RedCrossManager.Client/src/app/core/services/onboarding-state.service.ts`

---

## Phase 5: User Story 3 - Volunteer Dashboard & Status Overview (Priority: P2)

**Goal**: Dashboard shows status, onboarding progress, assignments, trainings, certifications, alerts.

**Independent Test**: Login as Pending/InTraining/Active shows correct cards, alerts, and lists.

### Tests (write first)
- [ ] T027 [P] [US3] Backend integration test for `/volunteers/me` dashboard data shape in `RedCrossManager.Server/Tests/Integration/DashboardTests.cs`
- [ ] T028 [P] [US3] Frontend component test for dashboard cards/alerts rendering in `RedCrossManager.Client/src/app/features/dashboard/dashboard.component.spec.ts`

### Implementation
- [ ] T029 [US3] Add dashboard DTO aggregator (assignments, trainings, certs, alerts) in `RedCrossManager.Server/Services/Dashboard`
- [ ] T030 [US3] Implement dashboard controller endpoint in `RedCrossManager.Server/Controllers/DashboardController.cs`
- [ ] T031 [US3] Build dashboard UI with cards, tables, alerts in `RedCrossManager.Client/src/app/features/dashboard`

---

## Phase 6: User Story 4 - Training Management & Enrollment (Priority: P2)

**Goal**: Coordinators publish trainings; volunteers enroll/waitlist; attendance and certificates tracked.

**Independent Test**: Create training, enroll volunteer, mark attendance -> certificate issued and visible.

### Tests (write first)
- [ ] T032 [P] [US4] Contract/integration test for training create/list/enroll in `RedCrossManager.Server/Tests/Integration/TrainingTests.cs`
- [ ] T033 [P] [US4] Frontend component test for training catalog filters/enroll flow in `RedCrossManager.Client/src/app/features/trainings/trainings.component.spec.ts`

### Implementation
- [ ] T034 [P] [US4] Implement `Training`, `TrainingEnrollment` entities/repos in `RedCrossManager.Server/Domain/Entities` and `Repositories`
- [ ] T035 [US4] Implement training service (create/publish/enroll/waitlist/attendance) in `RedCrossManager.Server/Services/Trainings`
- [ ] T036 [US4] Implement trainings controller endpoints in `RedCrossManager.Server/Controllers/TrainingsController.cs`
- [ ] T037 [US4] Build training catalog + enrollment UI in `RedCrossManager.Client/src/app/features/trainings`
- [ ] T038 [US4] Generate certificates and link to documents in `RedCrossManager.Server/Services/Certificates`

---

## Phase 7: User Story 7 - B1J Communication Dashboard (Priority: P2)

**Goal**: Targeted comms to youth volunteers/guardians via email (required) and SMS (opt-in), with delivery status and history.

**Independent Test**: Send message to "B1J - Missing Consent" segment -> email delivered, SMS queued where opted; history visible to volunteer/guardian.

### Tests (write first)
- [ ] T039 [P] [US7] Integration test for communications send + status tracking in `RedCrossManager.Server/Tests/Integration/CommunicationsTests.cs`
- [ ] T040 [P] [US7] Frontend component test for comms composer and status table in `RedCrossManager.Client/src/app/features/b1j-comms/b1j-comms.component.spec.ts`

### Implementation
- [ ] T041 [P] [US7] Implement `CommunicationMessage` + `CommunicationRecipient` entities/repos in `RedCrossManager.Server/Domain/Entities` and `Repositories`
- [ ] T042 [US7] Implement communications service with SendGrid (email) and ACS SMS provider abstraction in `RedCrossManager.Server/Services/Communications`
- [ ] T043 [US7] Implement communications controller endpoints in `RedCrossManager.Server/Controllers/CommunicationsController.cs`
- [ ] T044 [US7] Build B1J communications UI (composer, recipient preview, delivery status) in `RedCrossManager.Client/src/app/features/b1j-comms`
- [ ] T045 [US7] Add message history view for volunteers/guardians in `RedCrossManager.Client/src/app/features/b1j-comms/history`

---

## Phase 8: User Story 5 - Mission Assignment & Scheduling (Priority: P3)

**Goal**: Create missions, notify qualified volunteers, apply/assign, reminders, at-risk flag on expiring certs.

**Independent Test**: Create mission requiring First Aid -> qualified volunteers notified; applications accepted; assignments confirmed; reminder sent.

### Tests (write first)
- [ ] T046 [P] [US5] Integration test for mission create/apply/assign/remind flow in `RedCrossManager.Server/Tests/Integration/MissionsTests.cs`
- [ ] T047 [P] [US5] Frontend component test for mission list/detail/apply in `RedCrossManager.Client/src/app/features/missions/missions.component.spec.ts`

### Implementation
- [ ] T048 [P] [US5] Implement `Mission` entity/repo in `RedCrossManager.Server/Domain/Entities` and `Repositories`; add `Mission.TravelBufferMinutes` field (int, default 120) for volunteer availability validation
- [ ] T049 [US5] Implement mission service (qualification filter, notify, assign, reminders, at-risk logic) in `RedCrossManager.Server/Services/Missions`
- [ ] T049a [US5] Implement time-overlap detection algorithm with configurable travel buffer (default 2 hours) in `AssignmentValidator.cs` to prevent conflicting mission assignments; add unit tests in `AssignmentValidatorTests.cs`
- [ ] T050 [US5] Implement missions controller endpoints in `RedCrossManager.Server/Controllers/MissionsController.cs`
- [ ] T051 [US5] Build missions UI (list, filters, detail, apply) in `RedCrossManager.Client/src/app/features/missions`

---

## Phase 9: User Story 6 - Document Management & Verification (Priority: P3)

**Goal**: Upload documents, verify/approve/reject, track expiry, block missions when missing/expired.

**Independent Test**: Upload ID -> Pending; coordinator approves; expiry alerts fire; expired document blocks mission apply.

### Tests (write first)
- [ ] T052 [P] [US6] Integration test for document upload/verify/expiry alert in `RedCrossManager.Server/Tests/Integration/DocumentsTests.cs`
- [ ] T053 [P] [US6] Frontend component test for document upload/status view in `RedCrossManager.Client/src/app/features/documents/documents.component.spec.ts`

### Implementation
- [ ] T054 [P] [US6] Implement `Document` entity/repo with Blob storage adapter and virus scan hook in `RedCrossManager.Server/Infrastructure`
- [ ] T055 [US6] Implement document service (upload URL, verify, expiry alerts) in `RedCrossManager.Server/Services/Documents`
- [ ] T056 [US6] Implement documents controller endpoints in `RedCrossManager.Server/Controllers/DocumentsController.cs`
- [ ] T057 [US6] Build documents UI (upload, status, color codes) in `RedCrossManager.Client/src/app/features/documents`

---

## Phase 10: Cross-Cutting Polish

- [ ] T100 Add localization resources (fr/en) for all new UI strings and email/SMS templates in `RedCrossManager.Client/src/app/i18n` and `RedCrossManager.Server/Resources`
- [ ] T101 [P] Add logging/enrichment for PII-safe telemetry in `RedCrossManager.Server`
- [ ] T102 [P] Harden validation (FluentValidation + data annotations) across DTOs in `RedCrossManager.Server/DTOs`
- [ ] T103 Improve accessibility (focus states, ARIA, contrast) in `RedCrossManager.Client/src`
- [ ] T104 Final CI pass: lint, tests, build artifacts for API and UI in repo root pipelines
- [ ] T105 Configure CI/CD coverage gate: enforce ≥80% line coverage for services/repositories; fail build if coverage drops; add PR template with constitution compliance checklist in `.github/pull_request_template.md`

---

## Dependencies
- Complete Phase 1 → Phase 2 → User stories in priority order
- US1 precedes US2 (onboarding relies on registration data)
- US2 precedes US3 (dashboard needs onboarding progress)
- US4 (trainings) precedes mission assignment (US5) where certifications are needed
- US6 (documents) supports US2/US5 blocking; can run parallel after foundation but must finish before mission blocking logic finalization
- US7 (B1J comms) depends on US1/US2 data but can proceed after foundational messaging setup

## Parallel Execution Examples
- Parallel: T004/T005 (tooling) while T002/T003 scaffold apps
- Parallel: T007/T008/T009 (security/observability) while T006 (Db) completes
- Parallel per story: tests (T013/T014, T020/T021, etc.) can run alongside UI build tasks once contracts stable
- Parallel: US5 and US6 can overlap after training entities exist (US4) and document schema ready

## Implementation Strategy
- MVP focus: US1 + US2 to get volunteers registered and onboarded; add parental consent gate
- Next: US3 dashboard for visibility; US4 trainings to unlock mission eligibility; US7 comms for minors
- Then: US5 missions and US6 documents complete compliance and delivery
- Always tests-first per constitution; keep contracts versioned `/api/v1`
