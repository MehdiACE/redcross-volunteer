using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Repositories;

public class ParentalConsentRepository : IParentalConsentRepository
{
    private readonly RedCrossDbContext _context;

    public ParentalConsentRepository(RedCrossDbContext context)
    {
        _context = context;
    }

    public async Task<ParentalConsent?> GetByVolunteerIdAsync(Guid volunteerId, CancellationToken cancellationToken = default)
    {
        return await _context.ParentalConsents
            .Include(c => c.Volunteer)
            .FirstOrDefaultAsync(c => c.VolunteerId == volunteerId, cancellationToken);
    }

    public async Task<ParentalConsent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ParentalConsents
            .Include(c => c.Volunteer)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<ParentalConsent> AddAsync(ParentalConsent consent, CancellationToken cancellationToken = default)
    {
        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync(cancellationToken);
        return consent;
    }

    public async Task UpdateAsync(ParentalConsent consent, CancellationToken cancellationToken = default)
    {
        _context.ParentalConsents.Update(consent);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ParentalConsent>> GetPendingReviewAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ParentalConsents
            .Include(c => c.Volunteer)
            .Where(c => c.ConsentStatus == ConsentStatus.Submitted)
            .OrderBy(c => c.SubmittedAt)
            .ToListAsync(cancellationToken);
    }
}
