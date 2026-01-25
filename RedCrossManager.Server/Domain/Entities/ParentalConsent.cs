namespace RedCrossManager.Server.Domain.Entities;

public class ParentalConsent
{
    public Guid Id { get; set; }
    public Guid VolunteerId { get; set; }
    public required string GuardianName { get; set; }
    public required string GuardianEmail { get; set; }
    public required string GuardianPhone { get; set; }
    public ConsentStatus ConsentStatus { get; set; } = ConsentStatus.NotRequested;
    public string? ConsentFormUrl { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewerId { get; set; }
    public string? ReviewerNotes { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public IdentityVerificationStatus IdentityVerificationStatus { get; set; } = IdentityVerificationStatus.NotVerified;
    public bool SmsOptIn { get; set; } = false;
    public string? AuditTrail { get; set; } // JSON of status changes

    // Navigation properties
    public Volunteer Volunteer { get; set; } = null!;
}

public enum ConsentStatus
{
    NotRequested,
    Requested,
    Submitted,
    Approved,
    Rejected
}

public enum IdentityVerificationStatus
{
    NotVerified,
    EmailConfirmed,
    Rejected
}
