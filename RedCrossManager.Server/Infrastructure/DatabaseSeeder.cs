using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedRolesAsync(RedCrossDbContext dbContext)
    {
        // Check if roles already exist
        if (await dbContext.Roles.AnyAsync())
        {
            return; // Roles already seeded
        }

        var roles = new[]
        {
            new Role 
            { 
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), 
                Name = "Volunteer", 
                Description = "Standard volunteer role" 
            },
            new Role 
            { 
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), 
                Name = "Coordinator", 
                Description = "Coordinator with management permissions" 
            },
            new Role 
            { 
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), 
                Name = "Admin", 
                Description = "Administrator with full system access" 
            }
        };

        dbContext.Roles.AddRange(roles);
        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedAdminUserAsync(RedCrossDbContext dbContext)
    {
        const string adminEmail = "admin@croix-rouge.fr";
        const string adminPassword = "P@ssword";

        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            return;
        }

        var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var passwordHasher = new PasswordHasher<User>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, adminPassword);

            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();
        }

        var hasRole = await dbContext.UserRoles.AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);
        if (!hasRole)
        {
            dbContext.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            });

            await dbContext.SaveChangesAsync();
        }
    }
}
