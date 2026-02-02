using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Notifications;
using RedCrossManager.Server.Repositories;

namespace RedCrossManager.Server.Services.Notifications;

public interface INotificationService
{
    Task<List<NotificationDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<NotificationDto>> GetForVolunteerAsync(Guid volunteerId, CancellationToken cancellationToken = default);
    Task<NotificationDto> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NotificationDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetByUserIdAsync(userId, cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<List<NotificationDto>> GetForVolunteerAsync(Guid volunteerId, CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetByVolunteerIdAsync(volunteerId, cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            VolunteerId = dto.VolunteerId,
            Title = dto.Title,
            Message = dto.Message,
            Type = Enum.TryParse<NotificationType>(dto.Type, true, out var type) ? type : NotificationType.Info,
            ActionUrl = dto.ActionUrl,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        await _repository.AddAsync(notification, cancellationToken);
        return Map(notification);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null || notification.UserId != userId)
        {
            return false;
        }

        notification.IsRead = true;
        await _repository.UpdateAsync(notification, cancellationToken);
        return true;
    }

    private static NotificationDto Map(Notification notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Type.ToString(),
            notification.IsRead,
            notification.CreatedAt,
            notification.ActionUrl
        );
    }
}
