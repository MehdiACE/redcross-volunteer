using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace RedCrossManager.Server.Tests;

public class HealthChecksTests
{
    [Fact]
    public async Task Health_Endpoints_Return_200()
    {
        using var factory = new Infrastructure.TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var resp1 = await client.GetAsync("/health");
        var resp2 = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
    }
}
