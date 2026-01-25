# Research

## Decisions & Rationale

### .NET Version vs Constitution
- **Decision**: Target .NET 10 for backend with EF Core 10 to align with constitution.
- **Rationale**: Removes governance violation, gains latest runtime and EF features, keeps forward compatibility; requested stack is satisfied by newer version.
- **Alternatives considered**: Stay on .NET 8 for perceived stability—rejected to comply with constitution and avoid upgrade debt.

### Authentication & Authorization
- **Decision**: Implement JWT bearer auth with ASP.NET Core Identity; design for optional Azure AD (OpenID Connect) by abstracting auth config and enabling multi-scheme support.
- **Rationale**: JWT covers immediate need; Azure AD optionality requested; multi-scheme keeps future SSO easy.
- **Alternatives considered**: Azure AD only (drops local accounts; harder for volunteers without org accounts); Email-only magic links (weaker for admin/coord flows).

### Email & SMS Delivery
- **Decision**: Use SendGrid (email) + Azure Communication Services SMS (opt-in only) with provider abstraction; store delivery status per recipient.
- **Rationale**: Azure-native SMS/email pairing, good .NET SDK support, status callbacks for delivery reporting.
- **Alternatives considered**: Twilio (solid SMS, separate email), SMTP (lacks deliverability/telemetry), local pickup (dev-only).

### File Storage & Virus Scanning
- **Decision**: Azure Blob Storage for documents; local file system only for dev. Integrate AV scan hook (e.g., ClamAV container or provider scanning) before marking docs as approved.
- **Rationale**: Cloud-ready per constitution; scalable; keeps PII off app servers; scanning reduces risk.
- **Alternatives considered**: SQL FILESTREAM (tight coupling, storage cost), local disk (not cloud-ready), S3 (non-Azure stack).

### Parental Consent Workflow
- **Decision**: Treat consent as required onboarding gate for minors (<18). Send guardian link/email (and SMS if opted) to sign/upload the provided form; store signed PDF in Blob; track status (Requested/Submitted/Approved/Rejected).
- **Rationale**: Meets compliance for minors; keeps audit trail; aligns with spec acceptance scenarios.
- **Alternatives considered**: Manual offline consent (slow, no audit), implicit consent (non-compliant).

### Internationalization (FR/EN)
- **Decision**: Backend .resx for validation/errors/emails; frontend ngx-translate (fr/en). Accept-Language header and user preference drive language selection.
- **Rationale**: Constitution requires FR/EN parity; consistent UX.
- **Alternatives considered**: Single-language (violates constitution/spec); custom i18n plumbing (higher effort).

### CORS & Origins
- **Decision**: Dev: http://localhost:4200. Prod: env var list of allowed origins; no wildcards.
- **Rationale**: Security-first principle; prevents broad exposure.
- **Alternatives considered**: "*" (rejected, security risk).

### Health, Observability, and Metrics
- **Decision**: ASP.NET health checks (/health, /health/ready), Serilog with App Insights sink, structured logs. Frontend: basic runtime error logging to backend endpoint or App Insights.
- **Rationale**: Cloud-ready requirement; aids ops.
- **Alternatives considered**: Console-only logging (insufficient in Azure).

## Open Items (to confirm)
- Azure AD enablement timeline (optional vs required at go-live)
- SMS opt-in capture UX and legal text (per locale) for minors/guardians
- AV scanning approach (managed vs self-hosted ClamAV) — propose managed if available
