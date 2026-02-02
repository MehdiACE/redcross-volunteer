namespace RedCrossManager.Server.DTOs.Notifications;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt,
    string? ActionUrl
);

public record CreateNotificationDto(
    Guid? UserId,
    Guid? VolunteerId,
    string Title,
    string Message,
    string Type,
    string? ActionUrl
);
