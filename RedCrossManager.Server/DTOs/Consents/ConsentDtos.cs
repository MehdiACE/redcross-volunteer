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
    string GuardianFullName,
    string GuardianEmail,
    string GuardianPhone
);

public record GuardianInfoDto(
    string FullName,
    string Email,
    string Phone,
    string Relationship
);

public record SubmitConsentDto(
    GuardianInfoDto GuardianInfo,
    bool GuardianAgreement,
    bool DataProcessingAgreement,
    string Signature
);

public record ReviewConsentDto(
    string Action,
    string? ReviewerNotes
);
