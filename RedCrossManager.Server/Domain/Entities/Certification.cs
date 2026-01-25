namespace RedCrossManager.Server.Domain.Entities;

public class Certification
{
    public Guid Id { get; set; }
    public Guid VolunteerId { get; set; }
    public CertificationType Type { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid? DocumentId { get; set; }
    public required string Issuer { get; set; }
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

    // Navigation properties
    public Volunteer Volunteer { get; set; } = null!;
    public Document? Document { get; set; }
    public ICollection<TrainingEnrollment> TrainingEnrollments { get; set; } = new List<TrainingEnrollment>();
}

public enum CertificationType
{
    FirstAid,
    CPR,
    DisasterResponse,
    Other
}
