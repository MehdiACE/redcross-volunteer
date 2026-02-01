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
}
