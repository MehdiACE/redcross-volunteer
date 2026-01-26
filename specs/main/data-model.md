# Data Model

## Entities & Fields

### Volunteer
- Id (GUID)
- FirstName, LastName
- Email (unique)
- Phone
- DateOfBirth
- Address (Street, City, State/Province, PostalCode, Country)
- EmergencyContactName, EmergencyContactPhone
- AreasOfInterest (collection)
- Availability (days/times, serialized structure)
- Status (Pending, InTraining, Active, Inactive)
- LanguagePreference (fr, en)
- RegisteredAt (UTC)
- LastLoginAt (UTC, nullable)
- IsMinor (derived from DateOfBirth at registration)
- GuardianContactId (nullable, links to ParentalConsent current guardian)
- SmsOptIn (bool, default false; volunteer has explicitly opted in to SMS communications)

### ParentalConsent
- Id (GUID)
- VolunteerId (FK)
- GuardianName
- GuardianEmail
- GuardianPhone
- ConsentStatus (NotRequested, Requested, Submitted, Approved, Rejected)
- ConsentFormUrl (Blob)
- SubmittedAt (UTC, nullable)
- ReviewedAt (UTC, nullable)
- ReviewerId (nullable)
- ReviewerNotes (nullable)
- ExpiresAt (UTC, nullable; defaults to min(turns18, +12 months))
- IdentityVerificationStatus (NotVerified, EmailConfirmed, Rejected; tracks guardian email verification)
- SmsOptIn (bool, default false; guardian has explicitly opted in to SMS communications)
- AuditTrail (JSON of status changes)

### OnboardingStep
- Id (GUID)
- VolunteerId (FK)
- StepType (DocumentVerification, OrientationTraining, Certification, FinalReview)
- Status (NotStarted, InProgress, Submitted, Approved, Rejected)
- StartedAt (UTC, nullable)
- SubmittedAt (UTC, nullable)
- ApprovedAt (UTC, nullable)
- ReviewerId (nullable)
- ReviewerNotes (nullable)
- RelatedDocumentIds (array)

### Document
- Id (GUID)
- VolunteerId (FK)
- Category (Identification, BackgroundCheck, Certification, MedicalForm, ConsentForm)
- FileName
- FileUrl (Blob)
- ContentType
- SizeBytes
- UploadedAt (UTC)
- ExpiresAt (UTC, nullable)
- VerificationStatus (Pending, Approved, Rejected)
- ReviewerId (nullable)
- ReviewerNotes (nullable)
- VirusScanStatus (Pending, Clean, Flagged)

### Training
- Id (GUID)
- Title
- Description
- Category (Orientation, FirstAid, CPR, DisasterResponse, Other)
- Location (string or virtual link)
- StartAt (UTC)
- EndAt (UTC)
- Capacity
- Prerequisites (list of CertificationType)
- Published (bool)
- CreatedBy
- CreatedAt (UTC)

### TrainingEnrollment
- Id (GUID)
- TrainingId (FK)
- VolunteerId (FK)
- EnrollmentStatus (Registered, Waitlisted, Cancelled)
- AttendanceStatus (Pending, Attended, NoShow)
- CompletionStatus (Pending, Passed, Failed)
- Grade (nullable)
- CertificateId (nullable FK -> Certification)
- EnrolledAt (UTC)
- AttendedAt (UTC, nullable)

### Certification
- Id (GUID)
- VolunteerId (FK)
- Type (FirstAid, CPR, DisasterResponse, Other)
- IssuedAt (UTC)
- ExpiresAt (UTC)
- DocumentId (nullable FK -> Document)
- Issuer
- VerificationStatus (Pending, Verified, Rejected)

### Mission
- Id (GUID)
- Title
- Description
- MissionType (BloodDrive, DisasterRelief, CommunityProgram, Other)
- Location
- StartAt (UTC)
- EndAt (UTC)
- RequiredCertifications (list of CertificationType)
- VolunteersNeeded (int)
- TravelBufferMinutes (int, default 120; enforces minimum gap between volunteer assignments to account for travel time)
- Published (bool)
- CreatedBy
- CreatedAt (UTC)

### Assignment
- Id (GUID)
- MissionId (FK)
- VolunteerId (FK)
- Status (Pending, Confirmed, Completed, Cancelled, NoShow, AtRisk)
- RoleDescription
- AssignedAt (UTC)
- ReminderSentAt (UTC, nullable)
- HoursWorked (decimal, nullable)
- Notes (nullable)

### CommunicationMessage
- Id (GUID)
- Segment (e.g., B1J Missing Consent, B1J In Onboarding, B1J Assigned)
- Channel (Email, SMS)
- Language (fr, en)
- Subject (for email)
- BodyTemplate
- SentAt (UTC)
- CreatedBy
- DeliverySummary (queued/sent/failed counts)

### CommunicationRecipient
- Id (GUID)
- MessageId (FK)
- RecipientType (Volunteer, Guardian)
- VolunteerId (FK)
- GuardianEmail/Phone (when applicable)
- Channel (Email, SMS)
- DeliveryStatus (Queued, Sent, Failed)
- LastError (nullable)
- RetriedCount (int)
- DeliveredAt (UTC, nullable)

## Relationships
- Volunteer 1—* OnboardingStep
- Volunteer 1—* Document
- Volunteer 1—* TrainingEnrollment; Training 1—* TrainingEnrollment
- Volunteer 1—* Certification
- Volunteer 1—* Assignment; Mission 1—* Assignment
- Volunteer 1—* ParentalConsent (current active consent per minor)
- ParentalConsent may reference ConsentForm Document
- CommunicationMessage 1—* CommunicationRecipient; CommunicationRecipient links to Volunteer and optionally guardian contact

## Validation Rules
- Email unique; valid format.
- Phone required for SMS opt-in; SMS requires explicit opt-in flag stored with volunteer/guardian.
- DateOfBirth required; minors (<18) require ParentalConsent.Approved before advancing past Document Verification or enrolling in training/missions.
- Document upload size ≤10MB; allowed types PDF/JPG/PNG; VirusScanStatus must be Clean to approve.
- Training capacity enforced; waitlist auto-promote on vacancy.
- Assignments cannot overlap times unless coordinator override is explicitly set.
- Certifications must be valid (not expired) for missions requiring them; otherwise assignment status AtRisk.

## State Transitions (selected)
- Volunteer.Status: Pending → InTraining → Active → Inactive
- OnboardingStep.Status: NotStarted → InProgress → Submitted → Approved/Rejected
- ParentalConsent.Status: NotRequested → Requested → Submitted → Approved/Rejected
- Assignment.Status: Pending → Confirmed → Completed / Cancelled / NoShow / AtRisk
- TrainingEnrollment: Registered → Attended/NoShow/Cancelled; Completion Pending → Passed/Failed
