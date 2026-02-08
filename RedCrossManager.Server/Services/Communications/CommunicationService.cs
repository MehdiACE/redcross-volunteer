using Microsoft.Extensions.Logging;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Repositories;
using System.Text.RegularExpressions;

namespace RedCrossManager.Server.Services.Communications;

public class CommunicationService : ICommunicationService
{
    private readonly ICommunicationRepository _communicationRepo;
    private readonly IVolunteerRepository _volunteerRepo;
    private readonly IEmailProvider _emailProvider;
    private readonly ISmsProvider _smsProvider;
    private readonly ILogger<CommunicationService> _logger;

    public CommunicationService(
        ICommunicationRepository communicationRepo,
        IVolunteerRepository volunteerRepo,
        IEmailProvider emailProvider,
        ISmsProvider smsProvider,
        ILogger<CommunicationService> logger)
    {
        _communicationRepo = communicationRepo;
        _volunteerRepo = volunteerRepo;
        _emailProvider = emailProvider;
        _smsProvider = smsProvider;
        _logger = logger;
    }

    public async Task<CommunicationMessage> SendCommunicationAsync(
        string segment,
        CommunicationChannel channels,
        string language,
        string subject,
        string bodyTemplate,
        IEnumerable<Guid>? recipientVolunteerIds,
        Guid userId)
    {
        // Validate
        if (channels.HasFlag(CommunicationChannel.Email) && string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Subject is required for email communications", nameof(subject));
        }

        // Create message
        var message = new CommunicationMessage
        {
            Id = Guid.NewGuid(),
            Segment = segment,
            Channels = channels,
            Language = language,
            Subject = subject,
            BodyTemplate = bodyTemplate,
            SentAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _communicationRepo.CreateMessageAsync(message);

        // Determine recipients
        IEnumerable<Volunteer> targetVolunteers;
        if (recipientVolunteerIds != null && recipientVolunteerIds.Any())
        {
            targetVolunteers = await _volunteerRepo.GetByIdsAsync(recipientVolunteerIds);
        }
        else
        {
            targetVolunteers = await GetVolunteersBySegmentAsync(segment);
        }

        // Create recipients
        var recipients = new List<CommunicationRecipient>();
        foreach (var volunteer in targetVolunteers)
        {
            // Email channel
            if (channels.HasFlag(CommunicationChannel.Email))
            {
                if (!string.IsNullOrWhiteSpace(volunteer.Email))
                {
                    recipients.Add(new CommunicationRecipient
                    {
                        Id = Guid.NewGuid(),
                        MessageId = message.Id,
                        RecipientType = RecipientType.Volunteer,
                        VolunteerId = volunteer.Id,
                        RecipientEmail = volunteer.Email,
                        Channel = CommunicationChannel.Email,
                        DeliveryStatus = DeliveryStatus.Queued
                    });
                }

                // For minors, also send to guardian email if available
                if (volunteer.IsMinor && volunteer.ParentalConsent != null 
                    && !string.IsNullOrWhiteSpace(volunteer.ParentalConsent.GuardianEmail))
                {
                    recipients.Add(new CommunicationRecipient
                    {
                        Id = Guid.NewGuid(),
                        MessageId = message.Id,
                        RecipientType = RecipientType.Guardian,
                        VolunteerId = volunteer.Id,
                        RecipientEmail = volunteer.ParentalConsent.GuardianEmail,
                        Channel = CommunicationChannel.Email,
                        DeliveryStatus = DeliveryStatus.Queued
                    });
                }
            }

            // SMS channel
            if (channels.HasFlag(CommunicationChannel.SMS))
            {
                if (volunteer.SmsOptIn && !string.IsNullOrWhiteSpace(volunteer.Phone))
                {
                    recipients.Add(new CommunicationRecipient
                    {
                        Id = Guid.NewGuid(),
                        MessageId = message.Id,
                        RecipientType = RecipientType.Volunteer,
                        VolunteerId = volunteer.Id,
                        RecipientPhone = volunteer.Phone,
                        Channel = CommunicationChannel.SMS,
                        DeliveryStatus = DeliveryStatus.Queued
                    });
                }

                // For minors, also send to guardian phone if opted in
                if (volunteer.IsMinor && volunteer.ParentalConsent != null 
                    && volunteer.ParentalConsent.SmsOptIn 
                    && !string.IsNullOrWhiteSpace(volunteer.ParentalConsent.GuardianPhone))
                {
                    recipients.Add(new CommunicationRecipient
                    {
                        Id = Guid.NewGuid(),
                        MessageId = message.Id,
                        RecipientType = RecipientType.Guardian,
                        VolunteerId = volunteer.Id,
                        RecipientPhone = volunteer.ParentalConsent.GuardianPhone,
                        Channel = CommunicationChannel.SMS,
                        DeliveryStatus = DeliveryStatus.Queued
                    });
                }
            }
        }

        if (recipients.Any())
        {
            await _communicationRepo.CreateRecipientsAsync(recipients);
        }

        _logger.LogInformation("Created communication {MessageId} for segment '{Segment}' with {RecipientCount} recipients",
            message.Id, segment, recipients.Count);

        return message;
    }

    public async Task<int> ProcessQueuedCommunicationsAsync(int maxRecipients = 100)
    {
        var queuedRecipients = await _communicationRepo.GetQueuedRecipientsAsync(maxRecipients);
        var successCount = 0;

        foreach (var recipient in queuedRecipients)
        {
            try
            {
                var message = recipient.Message;
                if (message == null)
                {
                    _logger.LogWarning("Recipient {RecipientId} has no associated message", recipient.Id);
                    continue;
                }

                // Replace template placeholders
                var personalizedBody = await PersonalizeMessageAsync(message.BodyTemplate, recipient);

                // Send via appropriate channel
                bool success;
                string? error = null;

                if (recipient.Channel == CommunicationChannel.Email)
                {
                    if (string.IsNullOrWhiteSpace(recipient.RecipientEmail))
                    {
                        _logger.LogWarning("Recipient {RecipientId} has no email address", recipient.Id);
                        continue;
                    }

                    (success, error) = await _emailProvider.SendEmailAsync(
                        recipient.RecipientEmail,
                        message.Subject ?? string.Empty,
                        personalizedBody);
                }
                else if (recipient.Channel == CommunicationChannel.SMS)
                {
                    if (string.IsNullOrWhiteSpace(recipient.RecipientPhone))
                    {
                        _logger.LogWarning("Recipient {RecipientId} has no phone number", recipient.Id);
                        continue;
                    }

                    (success, error) = await _smsProvider.SendSmsAsync(
                        recipient.RecipientPhone,
                        personalizedBody);
                }
                else
                {
                    _logger.LogWarning("Unknown channel {Channel} for recipient {RecipientId}",
                        recipient.Channel, recipient.Id);
                    continue;
                }

                // Update status
                if (success)
                {
                    recipient.DeliveryStatus = DeliveryStatus.Sent;
                    recipient.DeliveredAt = DateTime.UtcNow;
                    successCount++;
                }
                else
                {
                    recipient.DeliveryStatus = DeliveryStatus.Failed;
                    recipient.LastError = error ?? "Unknown error";
                    recipient.RetriedCount++;
                }

                await _communicationRepo.UpdateRecipientAsync(recipient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send communication to recipient {RecipientId}", recipient.Id);

                recipient.DeliveryStatus = DeliveryStatus.Failed;
                recipient.LastError = ex.Message;
                recipient.RetriedCount++;

                await _communicationRepo.UpdateRecipientAsync(recipient);
            }
        }

        _logger.LogInformation("Processed {TotalCount} queued communications, {SuccessCount} successful",
            queuedRecipients.Count(), successCount);

        return successCount;
    }

    public async Task<IEnumerable<CommunicationRecipient>> GetVolunteerCommunicationHistoryAsync(Guid volunteerId)
    {
        return await _communicationRepo.GetRecipientsByVolunteerIdAsync(volunteerId);
    }

    public async Task<(CommunicationMessage Message, Dictionary<DeliveryStatus, int> Stats)> GetCommunicationStatusAsync(Guid messageId)
    {
        var message = await _communicationRepo.GetMessageByIdAsync(messageId);
        if (message == null)
        {
            throw new ArgumentException($"Communication message {messageId} not found", nameof(messageId));
        }

        var stats = await _communicationRepo.GetDeliveryStatsAsync(messageId);

        return (message, stats);
    }

    public async Task<IEnumerable<CommunicationMessage>> GetRecentCommunicationsAsync(int count = 50)
    {
        return await _communicationRepo.GetRecentMessagesAsync(count);
    }

    private async Task<IEnumerable<Volunteer>> GetVolunteersBySegmentAsync(string segment)
    {
        // Segment-based targeting logic
        // Examples: "B1J - Missing Consent", "B1J - In Onboarding", "B1J - Assigned", "Active Volunteers"
        
        var allVolunteers = await _volunteerRepo.GetAllAsync();

        return segment switch
        {
            "B1J - Missing Consent" => allVolunteers.Where(v => v.IsMinor && v.Status == VolunteerStatus.Pending),
            "B1J - In Onboarding" => allVolunteers.Where(v => v.IsMinor && v.Status == VolunteerStatus.Pending),
            "B1J - Assigned" => allVolunteers.Where(v => v.IsMinor && v.Status == VolunteerStatus.Active),
            "Active Volunteers" => allVolunteers.Where(v => v.Status == VolunteerStatus.Active),
            _ => Enumerable.Empty<Volunteer>()
        };
    }

    private async Task<string> PersonalizeMessageAsync(string template, CommunicationRecipient recipient)
    {
        var volunteer = recipient.Volunteer;
        if (volunteer == null && recipient.VolunteerId.HasValue)
        {
            volunteer = await _volunteerRepo.GetByIdAsync(recipient.VolunteerId.Value);
        }

        if (volunteer == null)
        {
            return template;
        }

        var personalized = template;

        // Replace common placeholders
        personalized = Regex.Replace(personalized, @"\{FirstName\}", volunteer.FirstName, RegexOptions.IgnoreCase);
        personalized = Regex.Replace(personalized, @"\{LastName\}", volunteer.LastName, RegexOptions.IgnoreCase);
        personalized = Regex.Replace(personalized, @"\{FullName\}", $"{volunteer.FirstName} {volunteer.LastName}", RegexOptions.IgnoreCase);
        personalized = Regex.Replace(personalized, @"\{Email\}", volunteer.Email, RegexOptions.IgnoreCase);
        personalized = Regex.Replace(personalized, @"\{Phone\}", volunteer.Phone ?? string.Empty, RegexOptions.IgnoreCase);

        // Add consent link for minors
        if (volunteer.IsMinor)
        {
            var consentLink = $"https://app.redcross.ca/guardian-consent/{volunteer.Id}";
            personalized = Regex.Replace(personalized, @"\{ConsentLink\}", consentLink, RegexOptions.IgnoreCase);
        }

        return personalized;
    }
}
