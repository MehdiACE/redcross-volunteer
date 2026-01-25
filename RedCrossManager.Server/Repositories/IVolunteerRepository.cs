using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Repositories;

public interface IVolunteerRepository
{
    Task<Volunteer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Volunteer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<Volunteer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Volunteer> AddAsync(Volunteer volunteer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Volunteer volunteer, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
