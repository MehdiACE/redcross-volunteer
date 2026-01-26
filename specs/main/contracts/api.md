# API Contracts (v1)

Base path: `/api/v1`
Auth: JWT Bearer (Azure AD optional multi-scheme). All endpoints require auth except registration and health.
Content: JSON

## Volunteers
- POST `/volunteers/register` — public registration; body: firstName, lastName, email, phone, dateOfBirth, address, emergencyContact, areasOfInterest[], availability
- GET `/volunteers/me` — current volunteer profile
- PUT `/volunteers/me` — update profile fields
- POST `/volunteers/me/sms-opt-in` — opt in/out of SMS communications; body: { smsOptIn: boolean }
- GET `/volunteers/{id}` — admin/coordinator view
- PATCH `/volunteers/{id}/status` — change status (Coordinator/Admin)

## Onboarding
- GET `/onboarding/me` — get stepper state and pending actions
- POST `/onboarding/me/document` — upload doc metadata; returns upload URL
- POST `/onboarding/me/submit-step` — submit current step (DocumentVerification/Orientation/Certification/FinalReview)
- POST `/onboarding/{volunteerId}/review` — coordinator approve/reject step

## Parental Consent (Minors)
- POST `/consents/{volunteerId}/request` — coordinator triggers consent request (email/SMS to guardian)
- POST `/consents/{volunteerId}/submit` — guardian submits signed form (uploads doc reference)
- GET `/consents/{volunteerId}` — view consent status/history
- PATCH `/consents/{volunteerId}` — approve/reject with notes (Coordinator); body: { action: "Approve" | "Reject", reviewerNotes: string }

## Documents
- POST `/documents/upload-url` — get pre-signed URL for Blob; body: category, contentType, size
- PATCH `/documents/{id}/verify` — approve/reject with notes (Coordinator)
- GET `/documents/{volunteerId}` — list docs for volunteer (self or coordinator)

## Trainings
- POST `/trainings` — create training (Coordinator)
- GET `/trainings` — list/paginate/filter
- GET `/trainings/{id}` — details
- POST `/trainings/{id}/enroll` — enroll volunteer (self)
- POST `/trainings/{id}/waitlist` — join waitlist
- POST `/trainings/{id}/attendance` — mark attendance + grade (Coordinator)
- POST `/trainings/{id}/publish` — publish/unpublish (Coordinator)

## Certifications
- GET `/volunteers/{id}/certifications`
- POST `/volunteers/{id}/certifications` — add/update certification (Coordinator)

## Missions & Assignments
- POST `/missions` — create mission (Coordinator)
- GET `/missions` — list/filter
- GET `/missions/{id}` — details
- POST `/missions/{id}/apply` — volunteer apply/register
- POST `/missions/{id}/assign` — coordinator assigns volunteers
- POST `/assignments/{id}/confirm` — volunteer confirm attendance
- POST `/assignments/{id}/status` — coordinator mark Completed/NoShow/Cancelled

## Communications (B1J)
- POST `/communications` — create/send message (email required, SMS optional) with segment criteria
- GET `/communications` — list sent messages with delivery stats
- GET `/communications/{id}/recipients` — per-recipient delivery status and retry action
- GET `/communications/me` — volunteer/guardian message history

## Auth & Health
- POST `/auth/login`
- POST `/auth/refresh`
- GET `/health` (liveness), `/health/ready` (readiness)

## Common Schemas (DTOs)
- VolunteerDto: id, name, email, phone, status, language, areasOfInterest[], availability, isMinor, guardianRequired, smsOptIn
- OnboardingStepDto: stepType, status, requiredActions, documents[], notes
- ConsentDto: guardianName/email/phone, status, formUrl, submittedAt, reviewedAt, reviewerNotes, identityVerificationStatus, smsOptIn
- TrainingDto: id, title, description, category, schedule, capacity, prerequisites, published
- EnrollmentDto: id, status, attendanceStatus, completionStatus, certificateId
- MissionDto: id, title, description, location, startAt, endAt, requiredCertifications, volunteersNeeded, travelBufferMinutes, status
- AssignmentDto: id, missionId, volunteerId, status, roleDescription, reminderSentAt
- CommunicationMessageDto: id, segment, channel(s), language, subject, body, sentAt, deliverySummary
- CommunicationRecipientDto: id, recipientType, channel, status, lastError, deliveredAt
- ErrorResponseDto: { code: string, message: string, timestamp: UTC, details?: object } (standard error format for all endpoints)
