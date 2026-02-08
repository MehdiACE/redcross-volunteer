using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Repositories;

public interface ICommunicationRepository
{
    Task<CommunicationMessage?> GetMessageByIdAsync(Guid id);
    Task<IEnumerable<CommunicationMessage>> GetRecentMessagesAsync(int count = 50);
    Task<IEnumerable<CommunicationMessage>> GetMessagesBySegmentAsync(string segment);
    Task<CommunicationMessage> CreateMessageAsync(CommunicationMessage message);
    Task UpdateMessageAsync(CommunicationMessage message);
    
    Task<CommunicationRecipient?> GetRecipientByIdAsync(Guid id);
    Task<IEnumerable<CommunicationRecipient>> GetRecipientsByMessageIdAsync(Guid messageId);
    Task<IEnumerable<CommunicationRecipient>> GetRecipientsByVolunteerIdAsync(Guid volunteerId);
    Task<IEnumerable<CommunicationRecipient>> GetQueuedRecipientsAsync(int maxCount = 100);
    Task<CommunicationRecipient> CreateRecipientAsync(CommunicationRecipient recipient);
    Task CreateRecipientsAsync(IEnumerable<CommunicationRecipient> recipients);
    Task UpdateRecipientAsync(CommunicationRecipient recipient);
    Task<Dictionary<DeliveryStatus, int>> GetDeliveryStatsAsync(Guid messageId);
}
