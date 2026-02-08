using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Services.Communications;

/// <summary>
/// Email provider abstraction for SendGrid.
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Send an email message.
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">HTML body content</param>
    /// <returns>Success status and error message if failed</returns>
    Task<(bool Success, string? Error)> SendEmailAsync(string to, string subject, string body);
}

/// <summary>
/// SMS provider abstraction for Azure Communication Services.
/// </summary>
public interface ISmsProvider
{
    /// <summary>
    /// Send an SMS message.
    /// </summary>
    /// <param name="to">Recipient phone number (E.164 format)</param>
    /// <param name="body">SMS body content (plain text)</param>
    /// <returns>Success status and error message if failed</returns>
    Task<(bool Success, string? Error)> SendSmsAsync(string to, string body);
}
