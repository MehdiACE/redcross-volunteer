using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Repositories;

public interface IOnboardingStepRepository
{
    Task<List<OnboardingStep>> GetByVolunteerIdAsync(Guid volunteerId, CancellationToken cancellationToken = default);
    Task<OnboardingStep?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OnboardingStep> AddAsync(OnboardingStep step, CancellationToken cancellationToken = default);
    Task UpdateAsync(OnboardingStep step, CancellationToken cancellationToken = default);
    Task<OnboardingStep?> GetByVolunteerAndTypeAsync(Guid volunteerId, StepType stepType, CancellationToken cancellationToken = default);
    Task<List<OnboardingStep>> GetPendingForReviewAsync(CancellationToken cancellationToken = default);
}
