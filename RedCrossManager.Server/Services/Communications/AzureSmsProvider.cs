using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RedCrossManager.Server.Services.Communications;

/// <summary>
/// Azure Communication Services SMS provider implementation.
/// TODO: Add Azure.Communication.Sms NuGet package and implement actual SMS sending.
/// </summary>
public class AzureSmsProvider : ISmsProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureSmsProvider> _logger;

    public AzureSmsProvider(IConfiguration configuration, ILogger<AzureSmsProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> SendSmsAsync(string to, string body)
    {
        // TODO: Implement Azure Communication Services SMS integration
        // var connectionString = _configuration["AzureCommunicationServices:ConnectionString"];
        // var client = new SmsClient(connectionString);
        // var from = _configuration["AzureCommunicationServices:FromNumber"];
        // var response = await client.SendAsync(from, to, body);

        _logger.LogInformation("SIMULATED: Sending SMS to {To} with body length {BodyLength}", to, body.Length);

        // Simulate success
        await Task.CompletedTask;
        return (true, null);
    }
}
