using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace RedCrossManager.Server.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureAppConfiguration(Microsoft.AspNetCore.Hosting.WebHostBuilderContext context, IConfigurationBuilder configBuilder)
    {
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AllowedOrigins:0"] = "http://localhost:4200",
            ["Auth:Authority"] = "https://login.microsoftonline.com/test-tenant/v2.0",
            ["Auth:Audience"] = "api://redcrossmanager-server"
        });
    }
}
