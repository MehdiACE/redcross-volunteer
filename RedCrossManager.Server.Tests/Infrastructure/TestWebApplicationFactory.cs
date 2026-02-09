using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;
using RedCrossManager.Server.Services.Auth;
using System.Text;

namespace RedCrossManager.Server.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DatabaseRoot = new();
    private readonly string _databaseName = $"RedCrossManager_TestDb_{Guid.NewGuid():N}";

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
                options.UseInMemoryDatabase(_databaseName, DatabaseRoot));

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var jwtOptions = context.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });
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

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RedCrossDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        await SeedDatabaseAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RedCrossDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
    }

    public async Task<Dictionary<string, string>> SeedTestUsersAsync()
    {
        await SeedDatabaseAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RedCrossDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var adminUser = await CreateTestUserAsync("admin@test.com", "Password123!", "Admin");
        var coordinatorUser = await CreateTestUserAsync("coordinator@test.com", "Password123!", "Coordinator");
        var volunteerUser = await CreateTestUserAsync("volunteer@test.com", "Password123!", "Volunteer");

        var volunteer = await dbContext.Volunteers.FirstOrDefaultAsync(v => v.UserId == volunteerUser.Id);
        if (volunteer == null)
        {
            volunteer = new Volunteer
            {
                Id = volunteerUser.Id,
                FirstName = "Test",
                LastName = "Volunteer",
                Email = volunteerUser.Email,
                Phone = "+15145550000",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                AddressStreet = "123 Test St",
                AddressCity = "Montreal",
                AddressStateProvince = "QC",
                AddressPostalCode = "H2X 1Y4",
                AddressCountry = "Canada",
                EmergencyContactName = "Test Contact",
                EmergencyContactPhone = "+15145550001",
                AreasOfInterest = "[]",
                Availability = "{}",
                LanguagePreference = "en",
                Status = VolunteerStatus.Active,
                UserId = volunteerUser.Id
            };

            dbContext.Volunteers.Add(volunteer);
            await dbContext.SaveChangesAsync();
        }

        var adminToken = tokenService.CreateToken(adminUser, new[] { "Admin" }).Token;
        var coordinatorToken = tokenService.CreateToken(coordinatorUser, new[] { "Coordinator" }).Token;
        var volunteerToken = tokenService.CreateToken(volunteerUser, new[] { "Volunteer" }).Token;

        return new Dictionary<string, string>
        {
            ["admin"] = adminToken,
            ["coordinator"] = coordinatorToken,
            ["volunteer"] = volunteerToken,
            ["adminId"] = adminUser.Id.ToString(),
            ["coordinatorId"] = coordinatorUser.Id.ToString(),
            ["volunteerId"] = volunteer.Id.ToString()
        };
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
        var existingUser = await dbContext.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
        {
            if (roleNames.Length > 0)
            {
                var existingRoleIds = existingUser.UserRoles.Select(ur => ur.RoleId).ToHashSet();
                var rolesToAssign = await dbContext.Roles
                    .Where(r => roleNames.Contains(r.Name) && !existingRoleIds.Contains(r.Id))
                    .ToListAsync();

                foreach (var role in rolesToAssign)
                {
                    existingUser.UserRoles.Add(new UserRole { UserId = existingUser.Id, RoleId = role.Id });
                }

                if (rolesToAssign.Count > 0)
                {
                    await dbContext.SaveChangesAsync();
                }
            }

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
