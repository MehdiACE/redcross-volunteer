namespace RedCrossManager.Server.Services.Notifications;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string to, string firstName, string languagePreference, CancellationToken cancellationToken = default);
    Task SendParentalConsentRequestAsync(string guardianEmail, string volunteerName, Guid volunteerId, string languagePreference, CancellationToken cancellationToken = default);
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
