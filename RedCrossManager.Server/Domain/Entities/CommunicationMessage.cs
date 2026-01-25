namespace RedCrossManager.Server.Domain.Entities;

public class CommunicationMessage
{
    public Guid Id { get; set; }
    public required string Segment { get; set; }
    public CommunicationChannel Channel { get; set; }
    public required string Language { get; set; }
    public string? Subject { get; set; }
    public required string BodyTemplate { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public string? DeliverySummary { get; set; } // JSON with queued/sent/failed counts

    // Navigation properties
    public ICollection<CommunicationRecipient> Recipients { get; set; } = new List<CommunicationRecipient>();
}

public enum CommunicationChannel
{
    Email,
    SMS
}
