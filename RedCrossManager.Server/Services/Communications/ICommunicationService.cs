using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Services.Communications;

/// <summary>
/// Service for sending targeted communications to volunteers and guardians.
/// Supports email (SendGrid) and SMS (Azure Communication Services).
/// </summary>
public interface ICommunicationService
{
    /// <summary>
    /// Send a communication to a segment of volunteers/guardians.
    /// </summary>
    /// <param name="segment">Target segment (e.g., "B1J - Missing Consent")</param>
    /// <param name="channels">Delivery channels (Email, SMS, or both)</param>
    /// <param name="language">Message language (fr/en)</param>
    /// <param name="subject">Email subject (required for Email channel)</param>
    /// <param name="bodyTemplate">Message body with placeholders like {FirstName}, {ConsentLink}</param>
    /// <param name="recipientVolunteerIds">Specific volunteer IDs to target (if null, uses segment logic)</param>
    /// <param name="userId">User ID sending the message</param>
    /// <returns>Created communication message with queued recipients</returns>
    Task<CommunicationMessage> SendCommunicationAsync(
        string segment,
        CommunicationChannel channels,
        string language,
        string subject,
        string bodyTemplate,
        IEnumerable<Guid>? recipientVolunteerIds,
        Guid userId);

    /// <summary>
    /// Process queued communication recipients and send via appropriate channel.
    /// This is called by a background worker to actually deliver messages.
    /// </summary>
    /// <param name="maxRecipients">Maximum number of recipients to process in this batch</param>
    /// <returns>Number of successfully sent messages</returns>
    Task<int> ProcessQueuedCommunicationsAsync(int maxRecipients = 100);

    /// <summary>
    /// Get communication history for a specific volunteer (both direct and guardian messages).
    /// </summary>
    Task<IEnumerable<CommunicationRecipient>> GetVolunteerCommunicationHistoryAsync(Guid volunteerId);

    /// <summary>
    /// Get detailed status for a specific communication message.
    /// </summary>
    Task<(CommunicationMessage Message, Dictionary<DeliveryStatus, int> Stats)> GetCommunicationStatusAsync(Guid messageId);

    /// <summary>
    /// Get recent communications (admin view).
    /// </summary>
    Task<IEnumerable<CommunicationMessage>> GetRecentCommunicationsAsync(int count = 50);
}
