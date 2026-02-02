namespace RedCrossManager.Server.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid FromUserId { get; set; }
    public Guid? ToUserId { get; set; }
    public Guid? ToVolunteerId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public User? FromUser { get; set; }
    public User? ToUser { get; set; }
    public Volunteer? ToVolunteer { get; set; }
}
