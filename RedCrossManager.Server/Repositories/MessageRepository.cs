using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Messages;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly RedCrossDbContext _context;

    public MessageRepository(RedCrossDbContext context)
    {
        _context = context;
    }

    public async Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken = default)
    {
        var messages = await _context.Messages
            .Where(m => 
                (m.FromUserId == userId && m.ToUserId == otherUserId) ||
                (m.FromUserId == otherUserId && m.ToUserId == userId)
            )
            .Include(m => m.FromUser)
            .Include(m => m.ToUser)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(MapToDto).ToList();
    }

    public async Task<List<MessageDto>> GetInboxAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var messages = await _context.Messages
            .Where(m => m.ToUserId == userId)
            .Include(m => m.FromUser)
            .Include(m => m.ToUser)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(MapToDto).ToList();
    }

    public async Task<List<MessageDto>> GetSentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var messages = await _context.Messages
            .Where(m => m.FromUserId == userId)
            .Include(m => m.FromUser)
            .Include(m => m.ToUser)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(MapToDto).ToList();
    }

    public async Task<Message?> GetByIdAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Include(m => m.FromUser)
            .Include(m => m.ToUser)
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Where(m => m.ToUserId == userId && !m.IsRead)
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Message message, CancellationToken cancellationToken = default)
    {
        _context.Messages.Update(message);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private MessageDto MapToDto(Message message)
    {
        return new MessageDto(
            message.Id,
            message.FromUserId,
            message.FromUser?.Email ?? "Unknown",
            message.ToUser?.Email ?? message.ToVolunteer?.Email ?? "System",
            message.Content,
            message.IsRead,
            message.CreatedAt,
            message.ReadAt
        );
    }
}
