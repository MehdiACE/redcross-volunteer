namespace RedCrossManager.Server.Services.Notifications;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendConfirmationEmailAsync(string to, string firstName, string languagePreference, CancellationToken cancellationToken = default)
    {
        var (subject, body) = languagePreference == "fr"
            ? ("Bienvenue à la Croix-Rouge", $"Bonjour {firstName},\n\nMerci de vous être inscrit comme bénévole à la Croix-Rouge. Nous avons reçu votre inscription et elle est en cours de traitement.\n\nÉquipe Croix-Rouge")
            : ("Welcome to Red Cross", $"Hello {firstName},\n\nThank you for registering as a Red Cross volunteer. We have received your registration and it is being processed.\n\nRed Cross Team");

        await SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task SendParentalConsentRequestAsync(string guardianEmail, string volunteerName, Guid volunteerId, string languagePreference, CancellationToken cancellationToken = default)
    {
        var consentUrl = $"{_configuration["AppBaseUrl"]}/consent/{volunteerId}";
        var (subject, body) = languagePreference == "fr"
            ? ("Consentement parental requis", $"Bonjour,\n\n{volunteerName} souhaite devenir bénévole à la Croix-Rouge. En tant que parent/tuteur, votre consentement est requis.\n\nVeuillez cliquer sur ce lien pour fournir votre consentement : {consentUrl}\n\nÉquipe Croix-Rouge")
            : ("Parental Consent Required", $"Hello,\n\n{volunteerName} wants to become a Red Cross volunteer. As a parent/guardian, your consent is required.\n\nPlease click this link to provide consent: {consentUrl}\n\nRed Cross Team");

        await SendEmailAsync(guardianEmail, subject, body, cancellationToken);
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // TODO: Integrate with SendGrid or another email provider
        // For now, just log the email
        _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        _logger.LogDebug("Email body: {Body}", body);
        
        await Task.CompletedTask;
    }
}
