using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RedCrossManager.Server.DTOs.Auth;
using RedCrossManager.Server.Infrastructure;
using RedCrossManager.Server.Tests.Infrastructure;
using Xunit;

namespace RedCrossManager.Server.Tests.Integration;

public class AuthTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public AuthTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndRoles()
    {
        // Arrange: Initialize database and create test user
        await _factory.SeedDatabaseAsync();
        var testUser = await _factory.CreateTestUserAsync("coordinator@redcross.local", "SecurePassword123!", "Coordinator", "Admin");

        var loginRequest = new LoginRequestDto(
            Email: "coordinator@redcross.local",
            Password: "SecurePassword123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.NotEmpty(result.AccessToken);
        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
        Assert.NotEmpty(result.Roles);
        Assert.Contains("Coordinator", result.Roles);
        Assert.Contains("Admin", result.Roles);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange: Initialize database and create test user
        await _factory.SeedDatabaseAsync();
        await _factory.CreateTestUserAsync("volunteer@redcross.local", "CorrectPassword123!", "Volunteer");

        var loginRequest = new LoginRequestDto(
            Email: "volunteer@redcross.local",
            Password: "WrongPassword123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonexistentUser_ReturnsUnauthorized()
    {
        // Arrange: Initialize database (no test user created)
        await _factory.SeedDatabaseAsync();

        var loginRequest = new LoginRequestDto(
            Email: "nonexistent@redcross.local",
            Password: "AnyPassword123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInactiveUser_ReturnsUnauthorized()
    {
        // Arrange: Initialize database and create inactive test user
        await _factory.SeedDatabaseAsync();
        var testUser = await _factory.CreateTestUserAsync("inactive@redcross.local", "SecurePassword123!", "Volunteer");

        // Manually set user as inactive
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RedCrossDbContext>();
        testUser.IsActive = false;
        dbContext.Users.Update(testUser);
        await dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequestDto(
            Email: "inactive@redcross.local",
            Password: "SecurePassword123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_VolunteerRole_ContainsVolunteerInRoles()
    {
        // Arrange: Initialize database and create volunteer user
        await _factory.SeedDatabaseAsync();
        await _factory.CreateTestUserAsync("volunteer@redcross.local", "SecurePassword123!", "Volunteer");

        var loginRequest = new LoginRequestDto(
            Email: "volunteer@redcross.local",
            Password: "SecurePassword123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(result);
        Assert.Equal(new[] { "Volunteer" }, result.Roles);
    }

    [Fact]
    public async Task Login_UpdatesLastLoginAtTimestamp()
    {
        // Arrange: Initialize database and create test user
        await _factory.SeedDatabaseAsync();
        var testUser = await _factory.CreateTestUserAsync("admin@redcross.local", "SecurePassword123!", "Admin");
        var beforeLoginTime = DateTime.UtcNow;

        var loginRequest = new LoginRequestDto(
            Email: "admin@redcross.local",
            Password: "SecurePassword123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify LastLoginAt was updated in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RedCrossDbContext>();
        var updatedUser = dbContext.Users.First(u => u.Email == "admin@redcross.local");
        Assert.NotNull(updatedUser.LastLoginAt);
        Assert.True(updatedUser.LastLoginAt >= beforeLoginTime);
        Assert.True(updatedUser.LastLoginAt <= DateTime.UtcNow);
    }
}
