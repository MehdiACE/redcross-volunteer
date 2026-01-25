using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
                ["ConnectionStrings:DefaultConnection"] = "InMemory"
            });
        });

        builder.ConfigureServices((context, services) =>
        {
            // Remove all DbContext-related registrations
            services.RemoveAll(typeof(DbContextOptions<RedCrossDbContext>));
            services.RemoveAll(typeof(DbContextOptions));
            services.RemoveAll(typeof(RedCrossDbContext));

            // Add in-memory database for testing
            services.AddDbContext<RedCrossDbContext>((serviceProvider, options) =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });
        });

        builder.UseEnvironment("Test");
    }
}
