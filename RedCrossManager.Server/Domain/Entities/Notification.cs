namespace RedCrossManager.Server.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? VolunteerId { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public NotificationType Type { get; set; } = NotificationType.Info;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ActionUrl { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Volunteer? Volunteer { get; set; }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}
