using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Repositories;

public class CommunicationRepository : ICommunicationRepository
{
    private readonly RedCrossDbContext _context;

    public CommunicationRepository(RedCrossDbContext context)
    {
        _context = context;
    }

    public async Task<CommunicationMessage?> GetMessageByIdAsync(Guid id)
    {
        return await _context.CommunicationMessages
            .Include(m => m.Recipients)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<CommunicationMessage>> GetRecentMessagesAsync(int count = 50)
    {
        return await _context.CommunicationMessages
            .Include(m => m.Recipients)
            .OrderByDescending(m => m.SentAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<CommunicationMessage>> GetMessagesBySegmentAsync(string segment)
    {
        return await _context.CommunicationMessages
            .Include(m => m.Recipients)
            .Where(m => m.Segment == segment)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<CommunicationMessage> CreateMessageAsync(CommunicationMessage message)
    {
        _context.CommunicationMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task UpdateMessageAsync(CommunicationMessage message)
    {
        _context.CommunicationMessages.Update(message);
        await _context.SaveChangesAsync();
    }

    public async Task<CommunicationRecipient?> GetRecipientByIdAsync(Guid id)
    {
        return await _context.CommunicationRecipients
            .Include(r => r.Message)
            .Include(r => r.Volunteer)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<CommunicationRecipient>> GetRecipientsByMessageIdAsync(Guid messageId)
    {
        return await _context.CommunicationRecipients
            .Include(r => r.Volunteer)
            .Where(r => r.MessageId == messageId)
            .ToListAsync();
    }

    public async Task<IEnumerable<CommunicationRecipient>> GetRecipientsByVolunteerIdAsync(Guid volunteerId)
    {
        return await _context.CommunicationRecipients
            .Include(r => r.Message)
            .Where(r => r.VolunteerId == volunteerId)
            .OrderByDescending(r => r.Message.SentAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CommunicationRecipient>> GetQueuedRecipientsAsync(int maxCount = 100)
    {
        return await _context.CommunicationRecipients
            .Include(r => r.Message)
            .Include(r => r.Volunteer)
            .Where(r => r.DeliveryStatus == DeliveryStatus.Queued)
            .OrderBy(r => r.Message.SentAt)
            .Take(maxCount)
            .ToListAsync();
    }

    public async Task<CommunicationRecipient> CreateRecipientAsync(CommunicationRecipient recipient)
    {
        _context.CommunicationRecipients.Add(recipient);
        await _context.SaveChangesAsync();
        return recipient;
    }

    public async Task CreateRecipientsAsync(IEnumerable<CommunicationRecipient> recipients)
    {
        _context.CommunicationRecipients.AddRange(recipients);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRecipientAsync(CommunicationRecipient recipient)
    {
        _context.CommunicationRecipients.Update(recipient);
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<DeliveryStatus, int>> GetDeliveryStatsAsync(Guid messageId)
    {
        return await _context.CommunicationRecipients
            .Where(r => r.MessageId == messageId)
            .GroupBy(r => r.DeliveryStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }
}
