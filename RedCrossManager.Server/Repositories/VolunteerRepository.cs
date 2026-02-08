using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Repositories;

public class VolunteerRepository : IVolunteerRepository
{
    private readonly RedCrossDbContext _context;

    public VolunteerRepository(RedCrossDbContext context)
    {
        _context = context;
    }

    public async Task<Volunteer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Volunteers
            .Include(v => v.ParentalConsent)
            .Include(v => v.OnboardingSteps)
            .Include(v => v.Certifications)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Volunteer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Volunteers
            .Include(v => v.ParentalConsent)
            .Include(v => v.OnboardingSteps)
            .Include(v => v.Certifications)
            .FirstOrDefaultAsync(v => v.UserId == userId, cancellationToken);
    }

    public async Task<Volunteer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Volunteers
            .Include(v => v.ParentalConsent)
            .FirstOrDefaultAsync(v => v.Email == email, cancellationToken);
    }

    public async Task<List<Volunteer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Volunteers
            .Include(v => v.ParentalConsent)
            .OrderByDescending(v => v.RegisteredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Volunteer>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Volunteers
            .Include(v => v.ParentalConsent)
            .Where(v => ids.Contains(v.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Volunteer> AddAsync(Volunteer volunteer, CancellationToken cancellationToken = default)
    {
        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync(cancellationToken);
        return volunteer;
    }

    public async Task UpdateAsync(Volunteer volunteer, CancellationToken cancellationToken = default)
    {
        _context.Volunteers.Update(volunteer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Volunteers
            .AnyAsync(v => v.Email == email, cancellationToken);
    }
}
