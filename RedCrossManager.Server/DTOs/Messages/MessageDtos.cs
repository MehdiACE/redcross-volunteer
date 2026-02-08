namespace RedCrossManager.Server.DTOs.Messages;

public record MessageDto(
    Guid Id,
    Guid FromUserId,
    string FromUserName,
    string? ToUserName,
    string Content,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt
);

public record CreateMessageDto(
    Guid? ToUserId,
    Guid? ToVolunteerId,
    string Content
);

public record SendToVolunteerDto(
    Guid VolunteerId,
    string Content
);

public record ComposeMessageDto(
    string Subject,
    string Body,
    string RecipientType,
    List<Guid> RecipientIds
);
