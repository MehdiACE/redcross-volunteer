namespace RedCrossManager.Server.Domain.Entities;

public class TrainingEnrollment
{
    public Guid Id { get; set; }
    public Guid TrainingId { get; set; }
    public Guid VolunteerId { get; set; }
    public string Status { get; set; } = "Enrolled"; // Enrolled, Waitlisted, Completed, Cancelled
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? AttendedAt { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTime? CertificateIssuedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Training Training { get; set; } = null!;
    public Volunteer Volunteer { get; set; } = null!;
}
