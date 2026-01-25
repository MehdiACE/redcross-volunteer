namespace RedCrossManager.Server.Domain.Entities;

public class Training
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public TrainingCategory Category { get; set; }
    public required string Location { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int Capacity { get; set; }
    public string? Prerequisites { get; set; } // JSON list of CertificationType
    public bool Published { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<TrainingEnrollment> Enrollments { get; set; } = new List<TrainingEnrollment>();
}

public enum TrainingCategory
{
    Orientation,
    FirstAid,
    CPR,
    DisasterResponse,
    Other
}
