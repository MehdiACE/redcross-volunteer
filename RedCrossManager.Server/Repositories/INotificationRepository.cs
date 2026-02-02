using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Repositories;

public interface INotificationRepository
{
    Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Notification>> GetByVolunteerIdAsync(Guid volunteerId, CancellationToken cancellationToken = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
}
