using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Auth;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Services.Auth;

public class AuthService : IAuthService
{
    private readonly RedCrossDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _tokenService;

    public AuthService(RedCrossDbContext dbContext, IPasswordHasher<User> passwordHasher, IJwtTokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var (token, expiresAtUtc) = _tokenService.CreateToken(user, roles);

        return new LoginResponseDto(user.Id, token, expiresAtUtc, roles);
    }
}
