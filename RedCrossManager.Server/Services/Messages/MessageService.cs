using Microsoft.Extensions.Logging;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Messages;
using RedCrossManager.Server.Repositories;

namespace RedCrossManager.Server.Services.Messages;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IVolunteerRepository _volunteerRepository;
    private readonly ILogger<MessageService> _logger;

    public MessageService(
        IMessageRepository messageRepository,
        IVolunteerRepository volunteerRepository,
        ILogger<MessageService> logger
    )
    {
        _messageRepository = messageRepository;
        _volunteerRepository = volunteerRepository;
        _logger = logger;
    }

    public async Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken = default)
    {
        return await _messageRepository.GetConversationAsync(userId, otherUserId, cancellationToken);
    }

    public async Task<List<MessageDto>> GetInboxAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _messageRepository.GetInboxAsync(userId, cancellationToken);
    }

    public async Task<List<MessageDto>> GetSentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _messageRepository.GetSentAsync(userId, cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _messageRepository.GetUnreadCountAsync(userId, cancellationToken);
    }

    public async Task<MessageDto> SendMessageAsync(Guid fromUserId, CreateMessageDto dto, CancellationToken cancellationToken = default)
    {
        if (!dto.ToUserId.HasValue && !dto.ToVolunteerId.HasValue)
            throw new InvalidOperationException("Either ToUserId or ToVolunteerId must be provided");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            FromUserId = fromUserId,
            ToUserId = dto.ToUserId,
            ToVolunteerId = dto.ToVolunteerId,
            Content = dto.Content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message, cancellationToken);
        _logger.LogInformation("Message sent from {FromUserId} to {ToUserId}", fromUserId, dto.ToUserId ?? dto.ToVolunteerId);

        // Reload to get navigation properties
        var savedMessage = await _messageRepository.GetByIdAsync(message.Id, cancellationToken);
        if (savedMessage == null)
            throw new InvalidOperationException("Failed to retrieve saved message");

        return new MessageDto(
            savedMessage.Id,
            savedMessage.FromUserId,
            savedMessage.FromUser?.Email ?? "Unknown",
            savedMessage.ToUser?.Email ?? "Volunteer",
            savedMessage.Content,
            savedMessage.IsRead,
            savedMessage.CreatedAt,
            savedMessage.ReadAt
        );
    }

    public async Task<MessageDto> SendToVolunteerAsync(Guid fromUserId, Guid volunteerId, string content, CancellationToken cancellationToken = default)
    {
        var volunteer = await _volunteerRepository.GetByIdAsync(volunteerId, cancellationToken);
        if (volunteer == null)
            throw new InvalidOperationException($"Volunteer {volunteerId} not found");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            FromUserId = fromUserId,
            ToVolunteerId = volunteerId,
            Content = content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message, cancellationToken);
        _logger.LogInformation("Message sent from {FromUserId} to volunteer {VolunteerId}", fromUserId, volunteerId);

        var savedMessage = await _messageRepository.GetByIdAsync(message.Id, cancellationToken);
        if (savedMessage == null)
            throw new InvalidOperationException("Failed to retrieve saved message");

        return new MessageDto(
            savedMessage.Id,
            savedMessage.FromUserId,
            savedMessage.FromUser?.Email ?? "Unknown",
            volunteer.Email,
            savedMessage.Content,
            savedMessage.IsRead,
            savedMessage.CreatedAt,
            savedMessage.ReadAt
        );
    }

    public async Task MarkAsReadAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken);
        if (message == null)
            throw new InvalidOperationException($"Message {messageId} not found");

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        await _messageRepository.UpdateAsync(message, cancellationToken);
        _logger.LogInformation("Message {MessageId} marked as read", messageId);
    }
}
