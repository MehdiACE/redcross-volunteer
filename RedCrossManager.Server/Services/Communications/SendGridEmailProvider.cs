using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RedCrossManager.Server.Services.Communications;

/// <summary>
/// SendGrid email provider implementation.
/// TODO: Add SendGrid NuGet package and implement actual email sending.
/// </summary>
public class SendGridEmailProvider : IEmailProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridEmailProvider> _logger;

    public SendGridEmailProvider(IConfiguration configuration, ILogger<SendGridEmailProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> SendEmailAsync(string to, string subject, string body)
    {
        // TODO: Implement SendGrid integration
        // var apiKey = _configuration["SendGrid:ApiKey"];
        // var client = new SendGridClient(apiKey);
        // var from = new EmailAddress(_configuration["SendGrid:FromEmail"], "Red Cross Manager");
        // var msg = MailHelper.CreateSingleEmail(from, new EmailAddress(to), subject, body, body);
        // var response = await client.SendEmailAsync(msg);

        _logger.LogInformation("SIMULATED: Sending email to {To} with subject '{Subject}'", to, subject);

        // Simulate success
        await Task.CompletedTask;
        return (true, null);
    }
}
