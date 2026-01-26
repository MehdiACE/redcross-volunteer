using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AllowedOrigins:0"] = "http://localhost:4200",
                ["Auth:Authority"] = "https://login.microsoftonline.com/test-tenant/v2.0",
                ["Auth:Audience"] = "api://redcrossmanager-server",
                ["AppBaseUrl"] = "http://localhost:4200",
                // Override connection string to prevent SQL Server usage
                ["ConnectionStrings:DefaultConnection"] = "InMemory",
                // JWT settings for tests
                ["Jwt:Key"] = "test-secret-key-that-is-long-enough-for-hs256-algorithm",
                ["Jwt:Issuer"] = "http://localhost:5000",
                ["Jwt:Audience"] = "api://redcrossmanager",
                ["Jwt:AccessTokenMinutes"] = "60"
            });
        });

        builder.ConfigureServices((context, services) =>
        {
            // Remove the DbContext service and all EF options
            var dbContextDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(RedCrossDbContext));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Remove DbContextOptions registrations
            services.RemoveAll(typeof(DbContextOptions<RedCrossDbContext>));
            services.RemoveAll(typeof(DbContextOptions));

            // Add in-memory database for testing
            services.AddDbContext<RedCrossDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid()));
        });

        builder.UseEnvironment("Test");
    }

    /// <summary>
    /// Seeds test database with default roles and optional test users.
    /// Call this in test setup to initialize required data.
    /// </summary>
    public async Task SeedDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RedCrossDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        // Ensure database is created
        await dbContext.Database.EnsureCreatedAsync();

        // Seed roles if they don't exist
        if (!dbContext.Roles.Any())
        {
            var roles = new[]
            {
                new Role { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "Volunteer", Description = "Standard volunteer role" },
                new Role { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "Coordinator", Description = "Coordinator with management permissions" },
                new Role { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "Admin", Description = "Administrator with full system access" }
            };

            dbContext.Roles.AddRange(roles);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Creates a test user with the specified email and role(s).
    /// </summary>
    public async Task<User> CreateTestUserAsync(string email, string password, params string[] roleNames)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RedCrossDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        // Check if user already exists
        var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
        {
            return existingUser;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.HashPassword(null!, password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);

        // Assign roles
        if (roleNames.Length > 0)
        {
            var roles = await dbContext.Roles.Where(r => roleNames.Contains(r.Name)).ToListAsync();
            foreach (var role in roles)
            {
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            }
        }

        await dbContext.SaveChangesAsync();
        return user;
    }
}
