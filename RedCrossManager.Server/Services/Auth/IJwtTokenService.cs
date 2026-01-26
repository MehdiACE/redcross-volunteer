using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Services.Auth;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user, IReadOnlyList<string> roles);
}
