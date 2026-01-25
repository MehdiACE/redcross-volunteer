namespace RedCrossManager.Server.Domain.Entities;

public class Assignment
{
    public Guid Id { get; set; }
    public Guid MissionId { get; set; }
    public Guid VolunteerId { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
    public string? RoleDescription { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReminderSentAt { get; set; }
    public decimal? HoursWorked { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Mission Mission { get; set; } = null!;
    public Volunteer Volunteer { get; set; } = null!;
}

public enum AssignmentStatus
{
    Pending,
    Confirmed,
    Completed,
    Cancelled,
    NoShow,
    AtRisk
}
