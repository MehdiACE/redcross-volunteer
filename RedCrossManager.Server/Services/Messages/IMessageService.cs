using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Messages;

namespace RedCrossManager.Server.Services.Messages;

public interface IMessageService
{
    Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken = default);
    Task<List<MessageDto>> GetInboxAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<MessageDto>> GetSentAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<MessageDto> SendMessageAsync(Guid fromUserId, CreateMessageDto dto, CancellationToken cancellationToken = default);
    Task<MessageDto> SendToVolunteerAsync(Guid fromUserId, Guid volunteerId, string content, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid messageId, CancellationToken cancellationToken = default);
}
