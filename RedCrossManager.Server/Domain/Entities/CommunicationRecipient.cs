namespace RedCrossManager.Server.Domain.Entities;

public class CommunicationRecipient
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public RecipientType RecipientType { get; set; }
    public Guid? VolunteerId { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }
    public CommunicationChannel Channel { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Queued;
    public string? LastError { get; set; }
    public int RetriedCount { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // Navigation properties
    public CommunicationMessage Message { get; set; } = null!;
    public Volunteer? Volunteer { get; set; }
}

public enum RecipientType
{
    Volunteer,
    Guardian
}

public enum DeliveryStatus
{
    Queued,
    Sent,
    Failed,
    Bounced
}
