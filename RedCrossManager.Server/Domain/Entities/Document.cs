namespace RedCrossManager.Server.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid VolunteerId { get; set; }
    public DocumentCategory Category { get; set; }
    public required string FileName { get; set; }
    public required string FileUrl { get; set; }
    public required string ContentType { get; set; }
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    public Guid? ReviewerId { get; set; }
    public string? ReviewerNotes { get; set; }
    public VirusScanStatus VirusScanStatus { get; set; } = VirusScanStatus.Pending;

    // Navigation properties
    public Volunteer Volunteer { get; set; } = null!;
}

public enum DocumentCategory
{
    Identification,
    BackgroundCheck,
    Certification,
    MedicalForm,
    ConsentForm
}

public enum VerificationStatus
{
    Pending,
    Approved,
    Rejected
}

public enum VirusScanStatus
{
    Pending,
    Clean,
    Flagged
}
