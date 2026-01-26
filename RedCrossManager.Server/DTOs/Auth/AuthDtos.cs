namespace RedCrossManager.Server.DTOs.Auth;

public record LoginRequestDto(
    string Email,
    string Password
);

public record LoginResponseDto(
    Guid UserId,
    string AccessToken,
    DateTime ExpiresAtUtc,
    IReadOnlyList<string> Roles
);
