namespace RedCrossManager.Server.Domain.Entities;

public class TrainingEnrollment
{
    public Guid Id { get; set; }
    public Guid TrainingId { get; set; }
    public Guid VolunteerId { get; set; }
    public EnrollmentStatus EnrollmentStatus { get; set; } = EnrollmentStatus.Registered;
    public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.Pending;
    public CompletionStatus CompletionStatus { get; set; } = CompletionStatus.Pending;
    public string? Grade { get; set; }
    public Guid? CertificateId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? AttendedAt { get; set; }

    // Navigation properties
    public Training Training { get; set; } = null!;
    public Volunteer Volunteer { get; set; } = null!;
    public Certification? Certificate { get; set; }
}

public enum EnrollmentStatus
{
    Registered,
    Waitlisted,
    Cancelled
}

public enum AttendanceStatus
{
    Pending,
    Attended,
    NoShow
}

public enum CompletionStatus
{
    Pending,
    Passed,
    Failed
}
