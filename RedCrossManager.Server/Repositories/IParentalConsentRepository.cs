using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Repositories;

public interface IParentalConsentRepository
{
    Task<ParentalConsent?> GetByVolunteerIdAsync(Guid volunteerId, CancellationToken cancellationToken = default);
    Task<ParentalConsent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ParentalConsent> AddAsync(ParentalConsent consent, CancellationToken cancellationToken = default);
    Task UpdateAsync(ParentalConsent consent, CancellationToken cancellationToken = default);
    Task<List<ParentalConsent>> GetPendingReviewAsync(CancellationToken cancellationToken = default);
}
