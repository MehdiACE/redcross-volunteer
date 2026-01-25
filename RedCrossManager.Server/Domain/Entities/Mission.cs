namespace RedCrossManager.Server.Domain.Entities;

public class Mission
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public MissionType MissionType { get; set; }
    public required string Location { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? RequiredCertifications { get; set; } // JSON list of CertificationType
    public int VolunteersNeeded { get; set; }
    public int TravelBufferMinutes { get; set; } = 120;
    public bool Published { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}

public enum MissionType
{
    BloodDrive,
    DisasterRelief,
    CommunityProgram,
    Other
}
