namespace RedCrossManager.Server.DTOs.Consents;

public record ParentalConsentDto(
    Guid Id,
    Guid VolunteerId,
    string GuardianName,
    string GuardianEmail,
    string GuardianPhone,
    string ConsentStatus,
    string IdentityVerificationStatus,
    DateTime? SubmittedAt,
    DateTime? ReviewedAt,
    DateTime? ExpiresAt,
    bool SmsOptIn
);

public record RequestConsentDto(
    string GuardianName,
    string GuardianEmail,
    string GuardianPhone
);

public record SubmitConsentDto(
    string ConsentFormUrl
);

public record ReviewConsentDto(
    bool Approved,
    string? ReviewerNotes
);
