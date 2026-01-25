namespace RedCrossManager.Server.Domain.Entities;

public class OnboardingStep
{
    public Guid Id { get; set; }
    public Guid VolunteerId { get; set; }
    public StepType StepType { get; set; }
    public StepStatus Status { get; set; } = StepStatus.NotStarted;
    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ReviewerId { get; set; }
    public string? ReviewerNotes { get; set; }
    public string? RelatedDocumentIds { get; set; } // JSON array

    // Navigation properties
    public Volunteer Volunteer { get; set; } = null!;
}

public enum StepType
{
    DocumentVerification,
    OrientationTraining,
    Certification,
    FinalReview
}

public enum StepStatus
{
    NotStarted,
    InProgress,
    Submitted,
    Approved,
    Rejected
}
