using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Messages;

namespace RedCrossManager.Server.Repositories;

public interface IMessageRepository
{
    Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken = default);
    Task<List<MessageDto>> GetInboxAsync(Guid userId, Guid? volunteerId, CancellationToken cancellationToken = default);
    Task<List<MessageDto>> GetSentAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Message?> GetByIdAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, Guid? volunteerId, CancellationToken cancellationToken = default);
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
    Task UpdateAsync(Message message, CancellationToken cancellationToken = default);
}
