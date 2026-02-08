using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.DTOs.Communications;

/// <summary>
/// Request to send a new communication message.
/// </summary>
public class SendCommunicationRequest
{
    /// <summary>Target segment (e.g., "B1J - Missing Consent")</summary>
    public required string Segment { get; set; }

    /// <summary>Delivery channels (Email, SMS, or both)</summary>
    public CommunicationChannel Channels { get; set; }

    /// <summary>Message language (fr/en)</summary>
    public required string Language { get; set; }

    /// <summary>Email subject (required if Email channel selected)</summary>
    public string? Subject { get; set; }

    /// <summary>Message body template with placeholders</summary>
    public required string BodyTemplate { get; set; }

    /// <summary>Optional: Specific volunteer IDs to target (if null, uses segment logic)</summary>
    public List<Guid>? RecipientVolunteerIds { get; set; }
}

/// <summary>
/// Communication message details.
/// </summary>
public class CommunicationMessageDto
{
    public Guid Id { get; set; }
    public string Segment { get; set; } = string.Empty;
    public CommunicationChannel Channels { get; set; }
    public string Language { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string BodyTemplate { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public Guid CreatedBy { get; set; }
    public int TotalRecipients { get; set; }
    public int QueuedCount { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public int BouncedCount { get; set; }
}

/// <summary>
/// Communication recipient details with delivery status.
/// </summary>
public class CommunicationRecipientDto
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public RecipientType RecipientType { get; set; }
    public Guid? VolunteerId { get; set; }
    public string? VolunteerName { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }
    public CommunicationChannel Channel { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public int RetriedCount { get; set; }
    public string? LastError { get; set; }
    public string MessageSubject { get; set; } = string.Empty;
    public DateTime MessageSentAt { get; set; }
}
