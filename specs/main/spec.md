# Feature Specification: Volunteer Onboarding & Management System

**Feature Branch**: `001-volunteer-onboarding`  
**Created**: 2026-01-25  
**Status**: Draft  
**Input**: User description: "Application gestion bénévoles Croix-Rouge : onboarding (workflow stepper : inscription, formation, certification, assignation missions, documents), dashboard volontaire, gestion formations/assignments/certifications. Entités : Volunteer (profil, status), Training, Assignment, Document, OnboardingStep."

## UI/UX Requirements

- **Dark mode visual spec**: Dark mode MUST follow the provided onboarding design reference with **dark cards** and **dark form surfaces**, while **preserving the same accent color** (buttons, stepper, highlights) used in light mode.
- Applies to onboarding registration and stepper flows, and serves as the baseline for other forms/cards to keep visual consistency.

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Volunteer Registration & Profile Creation (Priority: P1)

A prospective volunteer visits the Red Cross platform and completes the initial registration process. They create their profile by providing personal information (name, email, phone, address, emergency contact), selecting their areas of interest (disaster relief, blood drives, community services, etc.), and indicating their availability. The system creates their volunteer account and assigns them "Pending" status.

**Why this priority**: Without registration, no volunteers can enter the system. This is the foundational entry point that enables all other workflows. Registration must be simple and welcoming to encourage volunteer participation.

**Independent Test**: Can be fully tested by completing the registration form, submitting it, and verifying a new volunteer profile exists with "Pending" status. Delivers a usable volunteer database even without training or assignment features.

**Acceptance Scenarios**:

1. **Given** a person visits the volunteer portal, **When** they click "Register as Volunteer" and complete all required fields (name, email, phone, address, emergency contact, areas of interest, availability), **Then** the system creates a new volunteer profile with "Pending" status and sends a confirmation email
2. **Given** a volunteer enters an email address already in the system, **When** they submit the registration form, **Then** the system displays an error message "This email is already registered" and provides a link to sign in or reset password
3. **Given** a volunteer completes the registration form, **When** they select multiple areas of interest (e.g., Disaster Relief + Blood Drives), **Then** the system saves all selected interests to their profile
4. **Given** a volunteer enters an invalid phone number format, **When** they submit the form, **Then** the system displays validation error "Please enter a valid phone number (e.g., 555-123-4567)"
5. **Given** a volunteer successfully registers, **When** registration is complete, **Then** they are automatically redirected to their dashboard with a welcome message and next steps for onboarding

---

### User Story 2 - Onboarding Workflow Stepper (Priority: P1)

A newly registered volunteer progresses through a guided onboarding workflow displayed as a visual stepper. The workflow includes sequential steps: (1) Document Verification - upload identification and background check consent, (2) Orientation Training - complete mandatory safety and Red Cross mission training, (3) Certification - obtain required certifications (First Aid, CPR if applicable), (4) Final Review - coordinator approval. The volunteer can see their progress, current step, and what's required to advance. The system tracks completion status for each step and updates the volunteer's overall status from "Pending" → "In Training" → "Active".

**Why this priority**: Onboarding is the core value proposition of this feature. Without a structured workflow, volunteer management becomes chaotic. This provides clarity for volunteers and accountability for coordinators.

**Independent Test**: Can be tested by walking a volunteer through all onboarding steps from registration to "Active" status. Delivers immediate value by standardizing the onboarding process and reducing coordinator manual tracking.

**Acceptance Scenarios**:

1. **Given** a volunteer with "Pending" status logs into their dashboard, **When** they view the onboarding section, **Then** they see a visual stepper showing 4 steps (Document Verification, Orientation, Certification, Final Review) with step 1 highlighted and actionable
2. **Given** a volunteer is on the "Document Verification" step, **When** they upload a government-issued ID and sign the background check consent form, **Then** the system marks the step as "Submitted" and advances them to "Orientation Training" step
3. **Given** a volunteer completes the Orientation Training (e.g., watches required videos, passes quiz), **When** they click "Complete Training", **Then** the system marks the step complete, awards the training certificate, and advances to "Certification" step
4. **Given** a volunteer uploads proof of First Aid and CPR certification, **When** a coordinator reviews and approves the documents, **Then** the system marks "Certification" complete and advances to "Final Review"
5. **Given** a coordinator reviews a volunteer's completed onboarding steps, **When** they approve the final review, **Then** the system changes volunteer status to "Active" and sends congratulations email with first mission opportunities
6. **Given** a volunteer is partway through onboarding, **When** they log out and return later, **Then** they resume at the exact step where they left off with all previous progress saved
7. **Given** a volunteer indicates they are under 18 years old, **When** they reach the onboarding step requiring parental/guardian consent, **Then** the system presents the consent form to the guardian for signature and does not allow advancement until consent is submitted and approved
8. **Given** a guardian receives a consent request for a minor volunteer, **When** they complete and submit the provided form, **Then** the system records the consent with timestamp and marks the onboarding step as "Submitted" pending coordinator approval
9. **Given** a minor volunteer without guardian consent attempts to proceed to Orientation Training, **When** they click "Continue", **Then** the system blocks progression and displays "Parental/guardian consent required before training"

---

### User Story 3 - Volunteer Dashboard & Status Overview (Priority: P2)

An active volunteer accesses their personalized dashboard showing an overview of their volunteer journey. The dashboard displays: current status badge (Pending/In Training/Active/Inactive), progress through onboarding (if incomplete), upcoming assignments with dates and locations, completed trainings and certifications with expiry dates, recent activity history, and quick action buttons (view assignments, browse available missions, update profile, upload documents).

**Why this priority**: The dashboard provides volunteers with visibility and control over their involvement. It reduces confusion about "what's next" and empowers volunteers to self-manage their participation. This improves retention and reduces coordinator support burden.

**Independent Test**: Can be tested by logging in as different volunteers (pending, in-training, active) and verifying each sees appropriate dashboard content. Delivers value by centralizing volunteer information and reducing "status check" emails to coordinators.

**Acceptance Scenarios**:

1. **Given** a volunteer with "Pending" status logs in, **When** they view their dashboard, **Then** they see a prominent onboarding progress card showing "Step 1 of 4: Document Verification" with a "Continue Onboarding" button
2. **Given** an active volunteer with upcoming assignments logs in, **When** they view their dashboard, **Then** they see a list of upcoming assignments (next 30 days) with dates, times, locations, and role descriptions
3. **Given** a volunteer has certifications expiring soon (within 60 days), **When** they view their dashboard, **Then** they see an alert banner "Your First Aid certification expires on [date] - Renew now" with a link to recertification options
4. **Given** a volunteer views their dashboard, **When** they look at the "My Trainings" section, **Then** they see all completed trainings with completion dates and downloadable certificates
5. **Given** a volunteer has no upcoming assignments, **When** they view their dashboard, **Then** they see a "Browse Available Missions" card encouraging them to volunteer for open opportunities

---

### User Story 4 - Training Management & Enrollment (Priority: P2)

Coordinators create and publish training courses (Orientation, First Aid, CPR, Disaster Response, etc.) with descriptions, schedules, capacity limits, and prerequisites. Volunteers browse available trainings, enroll in sessions, and receive calendar invitations. The system tracks enrollment, attendance, completion status, and issues certificates. Volunteers can view required vs. optional trainings based on their areas of interest and see training history.

**Why this priority**: Training is mandatory for volunteer activation. Digitizing training management eliminates spreadsheet chaos, ensures compliance with Red Cross standards, and provides volunteers with clear paths to qualification.

**Independent Test**: Can be tested by creating a training course, enrolling volunteers, marking attendance, and issuing certificates. Delivers value by automating training logistics that are currently manual.

**Acceptance Scenarios**:

1. **Given** a coordinator creates a "First Aid Basics" training, **When** they set the capacity to 20 participants and schedule it for March 15, **Then** the training appears in the volunteer training catalog with enrollment open
2. **Given** a volunteer browses available trainings, **When** they filter by "Required for Disaster Response", **Then** they see only trainings tagged as prerequisites for disaster response roles
3. **Given** a volunteer enrolls in a training session, **When** enrollment is confirmed, **Then** they receive a calendar invitation (.ics file) via email with training date, time, location, and joining instructions
4. **Given** a training session reaches capacity (e.g., 20/20 enrolled), **When** another volunteer attempts to enroll, **Then** they see "Session Full - Join Waitlist" and can opt-in to be notified if spots open
5. **Given** a coordinator marks attendance after a training session, **When** they mark volunteers as "Attended" and assign passing grades, **Then** the system generates certificates and updates volunteer profiles to show training completion
6. **Given** a volunteer completes a required training, **When** they view their dashboard, **Then** the training moves from "Required" to "Completed" and unlocks eligibility for related mission assignments

---

### User Story 5 - Mission Assignment & Scheduling (Priority: P3)

Coordinators create mission opportunities (blood drive events, disaster relief deployments, community assistance programs) with date/time, location, required certifications, number of volunteers needed, and role descriptions. Qualified volunteers receive notifications about relevant opportunities based on their interests, availability, and certifications. Volunteers can view mission details, apply/register for missions, and confirm their participation. Coordinators review applications, assign volunteers, and send confirmations. The system tracks assignments and sends reminders before mission dates.

**Why this priority**: Mission assignment is the end goal of volunteer onboarding - connecting volunteers with meaningful work. While critical long-term, the system provides value with registration and training alone in the short term. This is implemented after foundational infrastructure.

**Independent Test**: Can be tested by creating a mission, notifying qualified volunteers, accepting applications, and confirming assignments. Delivers value by matching volunteers to missions and reducing coordinator manual coordination.

**Acceptance Scenarios**:

1. **Given** a coordinator creates a "Blood Drive - Downtown Center" mission for March 20 requiring First Aid certification, **When** they publish the mission, **Then** all volunteers with active First Aid certification and "Blood Drives" interest receive notification emails
2. **Given** a volunteer receives a mission notification, **When** they click "View Details", **Then** they see mission description, date, time, location, required certifications, role expectations, and an "Apply" button
3. **Given** a volunteer applies for a mission, **When** they submit their application, **Then** the coordinator sees the application in their review queue with volunteer profile, relevant certifications, and past assignment history
4. **Given** a coordinator reviews applications for a mission needing 10 volunteers, **When** they select 10 applicants and click "Assign", **Then** selected volunteers receive confirmation emails and the mission appears on their dashboards as "Confirmed"
5. **Given** a volunteer is assigned to a mission 3 days away, **When** the system's reminder job runs, **Then** they receive an email reminder with mission details and a "Confirm Attendance" link
6. **Given** a volunteer's certification expires before their assigned mission date, **When** the expiry is detected, **Then** the system flags the assignment as "At Risk" for coordinator review and notifies the volunteer to renew certification

---

### User Story 6 - Document Management & Verification (Priority: P3)

Volunteers upload required documents (identification, background check consent, certifications, medical forms) to their profiles. Documents are organized by category, have expiry dates (where applicable), and show verification status (Pending/Approved/Rejected). Coordinators review uploaded documents, approve or request resubmission, and add notes. The system sends alerts when documents are expiring and prevents volunteers from mission assignments if required documents are missing or expired.

**Why this priority**: Document management is a compliance requirement but doesn't block basic system functionality. Volunteers can register and begin training while documents are being processed. This is important for regulatory compliance but can be implemented after core workflows.

**Independent Test**: Can be tested by uploading documents, coordinator review, approval/rejection flow, and expiry alerts. Delivers value by centralizing document storage and eliminating paper-based filing systems.

**Acceptance Scenarios**:

1. **Given** a volunteer is on the "Document Verification" onboarding step, **When** they upload a PDF of their driver's license, **Then** the document is saved to their profile under "Identification" category with status "Pending Review"
2. **Given** a coordinator reviews an uploaded ID document, **When** they verify it's valid and click "Approve", **Then** the document status changes to "Approved" and the volunteer is notified via email
3. **Given** a coordinator finds a document unclear (e.g., blurry photo), **When** they reject it with note "Please upload a clearer image", **Then** the volunteer receives notification with the rejection reason and can re-upload
4. **Given** a volunteer's First Aid certificate expires in 30 days, **When** the system's daily job runs, **Then** the volunteer receives an email alert "Your First Aid certification expires on [date] - Please upload renewed certificate"
5. **Given** a volunteer with an expired background check attempts to register for a mission, **When** they click "Apply", **Then** the system blocks the application and displays "Background check expired - Please upload renewed consent form before applying"
6. **Given** a volunteer views their Documents section, **When** they look at their certifications, **Then** they see a color-coded status: green for valid (>60 days until expiry), yellow for expiring soon (30-60 days), red for expired/missing

---

### User Story 7 - B1J Communication Dashboard (Priority: P2)

Coordinators need to communicate with youth volunteers (B1J) through a dedicated dashboard. The dashboard allows sending targeted messages by segment (age <18, onboarding status, mission assignment status) via email and, when available, SMS. Coordinators can draft messages, preview recipients, and send. Delivery status (queued, sent, failed) is visible. Volunteers receive messages in their preferred language (FR/EN) and can view a message history in their portal.

**Why this priority**: Clear communication with minors and their guardians reduces no-shows and compliance risk. Targeted messaging keeps B1J informed about consent requests, onboarding steps, and mission logistics.

**Independent Test**: Can be tested by sending a targeted message to a B1J segment, verifying delivery via email, optional SMS, and viewing the message in the volunteer portal history.

**Acceptance Scenarios**:

1. **Given** a coordinator selects the "B1J - Missing Consent" segment, **When** they compose a message and choose email channel, **Then** the system shows the list of recipient minors and their guardians and sends the email upon confirmation
2. **Given** a coordinator selects both email and SMS channels, **When** they send the message, **Then** the system dispatches email to volunteers/guardians and queues SMS for numbers that have opted in for SMS communication
3. **Given** a message is sent, **When** delivery completes, **Then** the coordinator dashboard shows status per recipient (sent/failed) and provides retry for failed SMS
4. **Given** a volunteer (or guardian) logs into the portal, **When** they view "Messages", **Then** they see the communication history with timestamps and content in their preferred language
5. **Given** a volunteer has not opted in to SMS, **When** the coordinator sends to both channels, **Then** the system skips SMS for that volunteer and logs email-only delivery

---

### Edge Cases

- What happens when a volunteer starts onboarding but never completes it? (System sends reminder emails after 7 days, 14 days, then marks profile as "Inactive" after 30 days of inactivity)
- How does the system handle volunteers who move to a different region? (Volunteer can update their address; system suggests local missions based on new location but retains all training/certification history)
- What if a volunteer's certification expires while they're assigned to a future mission? (System flags the assignment as "At Risk", notifies both volunteer and coordinator, and provides 14-day grace period to renew before auto-canceling assignment)
- How are scheduling conflicts handled when a volunteer is assigned to overlapping missions? (System checks for date/time conflicts during assignment and warns coordinator; volunteer can decline assignments that conflict)
- What happens if training capacity is reached but volunteers are already in waitlist? (When a spot opens (cancellation), system notifies waitlisted volunteers in order and grants 24-hour window to claim spot before moving to next person)
- How does the system handle volunteers with multiple active missions on the same day? (Permits multiple assignments if no time overlap; displays warning if missions are less than 2 hours apart to account for travel time)
- What if a volunteer doesn't show up for their assigned mission? (Coordinator marks as "No Show"; system logs attendance record and flags profile after 2+ no-shows for coordinator review)
- What if a minor's guardian does not respond to consent requests? (System sends reminders at 3 and 7 days, then marks onboarding as paused after 14 days and notifies coordinator)
- What if SMS delivery fails or a volunteer/guardian has not opted in? (System records failure, retries once, falls back to email-only if SMS opt-in is absent or opt-out is detected)

## Requirements *(mandatory)*


### Functional Requirements

**Volunteer Management**:
- **FR-001**: System MUST allow prospective volunteers to self-register by providing personal information (name, email, phone, address, emergency contact)
- **FR-002**: System MUST validate email format and enforce uniqueness (no duplicate email addresses)
- **FR-003**: System MUST track volunteer status (Pending, In Training, Active, Inactive) and enforce status transitions based on onboarding completion
- **FR-004**: System MUST allow volunteers to select multiple areas of interest from predefined categories (Disaster Relief, Blood Drives, Community Services, Youth Programs, Administrative Support)
- **FR-005**: System MUST allow volunteers to indicate their availability (days of week, time ranges, frequency)

**Onboarding Workflow**:
- **FR-006**: System MUST present a visual stepper workflow with 4 sequential steps: (1) Document Verification, (2) Orientation Training, (3) Certification, (4) Final Review
- **FR-007**: System MUST track completion status for each onboarding step (Not Started, In Progress, Submitted, Approved, Rejected)
- **FR-008**: System MUST prevent volunteers from advancing to next step until current step is approved
- **FR-009**: System MUST save volunteer progress and allow resuming onboarding from last completed step
- **FR-010**: System MUST transition volunteer status from "Pending" to "In Training" when first step is completed, and to "Active" when final review is approved
- **FR-011**: System MUST send automated reminder emails to volunteers with incomplete onboarding at 7-day and 14-day intervals

**Dashboard & Notifications**:
- **FR-012**: System MUST display personalized dashboard showing volunteer status, onboarding progress, upcoming assignments, certifications, and recent activity
- **FR-013**: System MUST send confirmation emails upon registration, training enrollment, mission assignment, and status changes
- **FR-014**: System MUST send reminder emails 3 days before assigned missions with mission details and confirmation link
- **FR-015**: System MUST display alerts on dashboard when certifications are expiring within 60 days

**Training Management**:
- **FR-016**: System MUST allow coordinators to create training courses with title, description, category (Orientation, First Aid, CPR, Disaster Response), schedule, location, capacity limit, and prerequisites
- **FR-017**: System MUST allow volunteers to browse available trainings, filter by category and date, and enroll in sessions
- **FR-018**: System MUST enforce training capacity limits and prevent enrollment when sessions are full
- **FR-019**: System MUST send calendar invitations (.ics files) to volunteers upon training enrollment
- **FR-020**: System MUST track training attendance and completion status
- **FR-021**: System MUST generate and store digital certificates upon training completion
- **FR-022**: System MUST maintain waitlist for full training sessions and notify waitlisted volunteers when spots become available

**Mission Assignment**:
- **FR-023**: System MUST allow coordinators to create mission opportunities with title, description, date/time, location, required certifications, number of volunteers needed, and role descriptions
- **FR-024**: System MUST notify qualified volunteers of relevant missions based on their interests, availability, and active certifications
- **FR-025**: System MUST allow volunteers to apply for missions and view application status (Pending, Accepted, Rejected)
- **FR-026**: System MUST allow coordinators to review applications, view volunteer profiles and history, and assign volunteers to missions
- **FR-027**: System MUST prevent volunteers from being assigned to time-overlapping missions without coordinator override
- **FR-028**: System MUST track mission attendance and allow coordinators to mark no-shows

**Document Management**:
- **FR-029**: System MUST allow volunteers to upload documents in common formats (PDF, JPG, PNG) with maximum file size of 10MB per document
- **FR-030**: System MUST organize documents by category (Identification, Background Check, Certifications, Medical Forms)
- **FR-031**: System MUST track document verification status (Pending, Approved, Rejected) and allow coordinator review with approval or rejection notes
- **FR-032**: System MUST track expiry dates for certifications and background checks
- **FR-033**: System MUST send automated alerts when documents are expiring within 30 days
- **FR-034**: System MUST prevent mission assignment if required documents are missing or expired

**Multilingual Support**:
- **FR-035**: System MUST support French and English languages for all user-facing content, with French as default language
- **FR-036**: System MUST allow users to switch languages via UI selector and persist language preference
- **FR-037**: System MUST send email notifications in the user's preferred language

**Security & Access Control**:
- **FR-038**: System MUST require authentication for all features except public registration page
- **FR-039**: System MUST implement role-based access control with minimum roles: Volunteer (view own data), Coordinator (manage volunteers/trainings/missions), Administrator (full system access)
- **FR-040**: System MUST enforce that volunteers can only view and edit their own profiles and assignments

**Parental Consent for Minors**:
- **FR-041**: System MUST capture date of birth during registration and flag volunteers under 18 as minors
- **FR-042**: System MUST collect parental/guardian contact information (name, email, phone) for minor volunteers
- **FR-043**: System MUST require a signed parental/guardian consent form before minors can advance beyond Document Verification or enroll in trainings/missions
- **FR-044**: System MUST provide a workflow to send the consent form to guardians, record submission timestamp, and store the signed form linked to the volunteer profile
- **FR-045**: System MUST track consent status (Not Requested, Requested, Submitted, Approved, Rejected) and block onboarding progression for minors until status is Approved
- **FR-046**: System MUST allow coordinators to resend consent requests and add reviewer notes when approving or rejecting submissions

**B1J Communication (Youth Volunteers)**:
- **FR-047**: System MUST allow coordinators to send targeted communications to youth volunteer segments (e.g., minors missing consent, minors in onboarding, minors assigned to missions)
- **FR-048**: System MUST support email as a required channel and SMS as an optional channel for recipients who have explicitly opted in to SMS
- **FR-049**: System MUST display delivery status per recipient (Queued, Sent, Failed) and allow retry for failed SMS deliveries
- **FR-050**: System MUST present message history to volunteers (and guardians where applicable) within the portal, showing content, channel, timestamp, and language
- **FR-051**: System MUST send communications in the recipient's preferred language (FR/EN) when available

### Key Entities

- **Volunteer**: Represents a Red Cross volunteer with profile information (name, contact details, emergency contact, areas of interest, availability), current status (Pending/In Training/Active/Inactive), onboarding progress, assigned roles, registration date, and language preference. Related to Assignments, Trainings, Documents, OnboardingSteps.

- **ParentalConsent**: Represents parental/guardian authorization for minor volunteers. Includes guardian identity (name, email, phone), consent form reference, submission timestamp, approval status (Requested/Submitted/Approved/Rejected), reviewer notes, and expiry/renewal date if required. Related to Volunteer.

- **OnboardingStep**: Represents one step in the volunteer onboarding workflow (Document Verification, Orientation Training, Certification, Final Review). Tracks completion status (Not Started/In Progress/Submitted/Approved/Rejected), submission date, approval date, reviewer notes, and links to required documents or trainings. Related to Volunteer.

- **Training**: Represents a training course or session with title, description, category (Orientation, First Aid, CPR, etc.), schedule (date/time), location, capacity limit, enrollment count, prerequisites, instructor, and published status. Related to TrainingEnrollments, Certifications.

- **TrainingEnrollment**: Represents a volunteer's enrollment in a training session. Tracks enrollment date, attendance status (Registered/Attended/No-Show/Cancelled), completion status, grade/score (if applicable), and certificate issue date. Links Volunteer to Training.

- **Certification**: Represents a certification earned by a volunteer (First Aid, CPR, Disaster Response, etc.). Includes certification type, issue date, expiry date, certificate document reference, issuing organization, and verification status. Related to Volunteer.

- **Assignment**: Represents a volunteer's assignment to a mission. Tracks mission details (title, description, date/time, location), role description, required certifications, assignment status (Pending/Confirmed/Completed/Cancelled/No-Show), confirmation status, attendance record, and hours worked. Links Volunteer to Mission.

- **Mission**: Represents a Red Cross mission or event requiring volunteers (blood drive, disaster relief, community program). Includes title, description, date/time, location, number of volunteers needed, required certifications, mission type, coordinator, and published status. Related to Assignments.

- **Document**: Represents an uploaded document in a volunteer's file. Tracks document category (Identification, Background Check, Certification, Medical Form), file name, file path/URL, upload date, expiry date (if applicable), verification status (Pending/Approved/Rejected), reviewer, review date, and reviewer notes. Related to Volunteer.

- **CommunicationMessage**: Represents an outbound communication to volunteers/guardians. Tracks audience segment criteria (e.g., minors missing consent), channel (email, SMS), language, content template, recipients, delivery status per recipient, send timestamp, and retry history. Related to Volunteer and ParentalConsent (for guardian recipients).

## Success Criteria *(mandatory)*


### Measurable Outcomes

**User Experience**:
- **SC-001**: Volunteers can complete initial registration in under 5 minutes (measured via analytics tracking from landing page to confirmation screen)
- **SC-002**: Volunteers can view all relevant information (status, assignments, trainings, certifications) on dashboard without navigating away (measured by single-page dashboard load time <2 seconds)
- **SC-003**: 90% of volunteers successfully complete onboarding without contacting coordinator for help (measured by completion rate vs. support ticket count)
- **SC-004**: Volunteers receive mission notifications within 5 minutes of coordinator publishing (measured by timestamp difference between publication and email send)

**System Performance**:
- **SC-005**: System supports 500 concurrent volunteer users during peak enrollment periods without performance degradation (response time remains <3 seconds for dashboard loads)
- **SC-006**: Document uploads complete successfully for files up to 10MB within 30 seconds on standard broadband connections
- **SC-007**: Automated email notifications (confirmations, reminders, alerts) are delivered within 10 minutes of triggering event

**Business Impact**:
- **SC-008**: Reduce coordinator time spent on manual onboarding tracking by 70% (measured by comparing pre-system vs. post-system hours logged for onboarding tasks)
- **SC-009**: Increase volunteer retention from registration to active status by 40% (measured by % of registered volunteers reaching "Active" status within 60 days)
- **SC-010**: Reduce volunteer no-show rate for assigned missions by 30% through automated reminders (measured by no-show % before vs. after system deployment)
- **SC-011**: 95% of certification expiries are renewed before expiry date due to automated alerts (measured by renewal completion rate among alerted volunteers)

**Compliance & Communications**:
- **SC-012**: 95% of minor volunteers obtain approved parental/guardian consent within 10 days of registration (measured by consent approval timestamps vs. registration dates)
- **SC-013**: 90% of B1J outbound messages are successfully delivered on the first attempt (combined email/SMS), with failures retried automatically
- **SC-014**: SMS opt-in recipients experience <5% delivery failure rate after retries (measured per sending batch)

**Adoption & Satisfaction**:
- **SC-015**: 80% of volunteers report onboarding process as "clear" or "very clear" in post-onboarding survey
- **SC-016**: Coordinators can create and publish a new training session in under 10 minutes (measured via user testing and analytics)
- **SC-017**: System achieves 85% volunteer adoption rate (active logins) within first 3 months of deployment among registered volunteers

### Assumed Defaults

**Language & Localization**:
- Default language is French (primary Red Cross Canada language); English is secondary. User can switch at any time.
- Email templates, validation messages, and error messages will be localized using resource files.

**Authentication & Security**:
- The application uses internal authentication with email/password and issues signed JWTs for access.
- Users and roles are stored in the database (tables for Users, Roles, and UserRoles). Role-based access controls map to Volunteer, Coordinator, and Admin.
- Password reset functionality follows standard email-based token flow with 1-hour expiry.
- Volunteer data is considered personally identifiable information (PII) and subject to standard privacy protection regulations (PIPEDA in Canada).

**Onboarding Workflow**:
- The 4-step onboarding workflow (Document Verification, Orientation, Certification, Final Review) is fixed for all volunteers. Minors add a required parental/guardian consent gate within Document Verification before proceeding.
- Coordinator approval is required for each onboarding step before volunteer can advance. Automated approval is not assumed.
- **Coordinator Review SLA**: Consent submissions and onboarding step approvals target 48-hour coordinator review window (best-effort; not guaranteed). System tracks review timestamps for audit; volunteer receives reminder email after 14 days of pending review.
- **Guardian Identity Verification**: Guardian consent requires email address verification (confirmation token sent to guardian email). System records verification status (NotVerified, EmailConfirmed, Rejected) but does not require notarization or advanced verification (e.g., govt ID).
- Background check processing happens externally (third-party service). System only tracks consent form upload and approval status, not actual check results.
- **Age Calculation**: A minor is defined using UTC date comparison: age = floor((UTC today - DateOfBirth) / 365.25 days). Consent remains valid until volunteer reaches UTC age 18 or for 12 months from request, whichever comes first.
- Guardians can be parents or legally authorized representatives; at least one guardian contact is required for minors.

**Training & Certifications**:
- Training sessions are in-person or virtual (Zoom/Teams link provided). System does not host embedded video training content in MVP.
- Certificates are PDF documents generated by the system with volunteer name, training name, completion date, and coordinator signature.
- First Aid and CPR certifications expire after 2 years (industry standard); other training expirations are configurable per training type.

**Mission Assignment**:
- Coordinators manually review and approve volunteer applications for missions. Automated matching based on skills/availability is not assumed for MVP.
- Volunteers can be assigned to maximum 5 concurrent active missions to prevent over-commitment (configurable limit).
- **Travel Buffer Enforcement**: System prevents volunteer from being assigned to two missions with less than 2-hour gap between end/start times (configurable per mission via `TravelBufferMinutes` field; default 120 minutes). Coordinator override available if needed.
- **Geographic Scope**: Mission notifications sent to all volunteers matching interest + certification requirements, regardless of location. Future enhancement: location-based filtering for mission discovery.
- Mission confirmation is optional. If volunteer doesn't confirm, assignment remains active unless volunteer explicitly cancels or coordinator removes them.

**Document Management**:
- **Document Expiry Mapping**: Identification (expires based on ID expiry), Background Check (expires 12 months from date, or 24 months if documented policy), Certification (expires per certification standard), Medical Form (no expiry), Consent Form (expires per consent expiry logic). System alerts on 30-day and 60-day pre-expiry thresholds.
- Documents are stored in Azure Blob Storage with virus scanning on upload. Local file system storage is development environment only.
- Accepted formats: PDF, JPG, PNG. File size limit: 10MB. Documents larger than 10MB must be compressed or split.
- Document retention policy: Documents are kept for 7 years after volunteer account becomes inactive (compliance with typical record retention regulations).

**Notifications**:
- Email is primary notification channel. SMS is supported for youth/B1J communications when recipients have explicitly opted in (tracked via `Volunteer.SmsOptIn` and `ParentalConsent.SmsOptIn` flags); otherwise, the system falls back to email-only.
- System will not send SMS to recipients without explicit opt-in or with missing phone numbers.
- **Retry Logic**: Failed email sends retried up to 3 times over 24 hours with exponential backoff. Failed SMS sends retried up to 2 times within 4 hours; no SMS attempted if opt-in absent. All delivery status logged for audit.

**Data Retention**:
- Volunteer accounts become "Inactive" after 365 days of no login activity. Inactive accounts receive reactivation email before status change.
- Inactive accounts are retained indefinitely unless volunteer requests deletion (GDPR/PIPEDA right to erasure).

## Scope Boundaries *(mandatory - defines what is NOT included)*

### Out of Scope for This Feature

**Not Included**:
- **Payment/Expense Reimbursement**: System does not handle volunteer expense claims or reimbursements. This is managed externally.
- **Volunteer Hours Tracking for Tax Receipts**: While system tracks mission hours, generating tax receipts or charitable donation documentation is not included.
- **Social Features**: Volunteer-to-volunteer messaging, forums, or community walls are not included. Communication happens via email and external channels.
- **Mobile Native Apps**: System is responsive web application. Native iOS/Android apps are not in scope.
- **Advanced Scheduling/Shift Management**: Complex shift rostering, time-off requests, and availability calendars beyond basic date/time selection are not included.
- **Background Check Processing**: System tracks consent and results but does not integrate with background check providers or process checks internally.
- **Embedded Training Content**: System manages training enrollment and certificates but does not host video training, quizzes, or learning management system (LMS) features.
- **Reporting & Analytics Dashboards**: Advanced analytics, custom reports, and data visualization dashboards for coordinators are not included. Basic data export (CSV) is sufficient for MVP.
- **Multi-Organization Support**: System is designed for single Red Cross organization. Supporting multiple independent organizations with separate data/users is out of scope.
- **Volunteer Recognition/Rewards**: Badges, points, leaderboards, or formal recognition programs are not included.

### Future Considerations (Potential Phase 2)

- Expanded SMS coverage beyond B1J communications (e.g., urgent mission alerts for all volunteers)
- Mobile native apps for field volunteers
- Integration with external calendar systems (Google Calendar, Outlook)
- Volunteer-to-volunteer shift swapping
- Advanced analytics and reporting dashboards for administrators
- Volunteer skills inventory and competency tracking
- Integration with third-party background check providers
- Multilingual support beyond French/English (Spanish, Mandarin, etc.)
