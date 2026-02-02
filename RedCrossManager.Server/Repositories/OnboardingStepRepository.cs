using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Repositories;

public class OnboardingStepRepository : IOnboardingStepRepository
{
    private readonly RedCrossDbContext _context;

    public OnboardingStepRepository(RedCrossDbContext context)
    {
        _context = context;
    }

    public async Task<List<OnboardingStep>> GetByVolunteerIdAsync(Guid volunteerId, CancellationToken cancellationToken = default)
    {
        return await _context.OnboardingSteps
            .Where(s => s.VolunteerId == volunteerId)
            .OrderBy(s => s.StepType)
            .ToListAsync(cancellationToken);
    }

    public async Task<OnboardingStep?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.OnboardingSteps
            .Include(s => s.Volunteer)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<OnboardingStep> AddAsync(OnboardingStep step, CancellationToken cancellationToken = default)
    {
        _context.OnboardingSteps.Add(step);
        await _context.SaveChangesAsync(cancellationToken);
        return step;
    }

    public async Task UpdateAsync(OnboardingStep step, CancellationToken cancellationToken = default)
    {
        _context.OnboardingSteps.Update(step);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<OnboardingStep?> GetByVolunteerAndTypeAsync(Guid volunteerId, StepType stepType, CancellationToken cancellationToken = default)
    {
        return await _context.OnboardingSteps
            .FirstOrDefaultAsync(s => s.VolunteerId == volunteerId && s.StepType == stepType, cancellationToken);
    }

    public async Task<List<OnboardingStep>> GetPendingForReviewAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OnboardingSteps
            .Include(s => s.Volunteer)
            .Where(s => s.Status == StepStatus.Submitted)
            .OrderBy(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);
    }
}
