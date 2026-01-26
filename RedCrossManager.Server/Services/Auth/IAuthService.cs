using RedCrossManager.Server.DTOs.Auth;

namespace RedCrossManager.Server.Services.Auth;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(string email, string password, CancellationToken cancellationToken);
}
