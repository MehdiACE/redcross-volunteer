using AutoMapper;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Consents;
using RedCrossManager.Server.Repositories;
using RedCrossManager.Server.Services.Notifications;
using System.Text.Json;

namespace RedCrossManager.Server.Services.Consents;

public interface IConsentService
{
    Task<ParentalConsentDto> RequestConsentAsync(Guid volunteerId, RequestConsentDto dto, CancellationToken cancellationToken = default);
    Task<ParentalConsentDto> SubmitConsentAsync(Guid volunteerId, SubmitConsentDto dto, CancellationToken cancellationToken = default);
    Task<ParentalConsentDto> ReviewConsentAsync(Guid consentId, Guid reviewerId, ReviewConsentDto dto, CancellationToken cancellationToken = default);
    Task<ParentalConsentDto?> GetByVolunteerIdAsync(Guid volunteerId, CancellationToken cancellationToken = default);
    Task<List<ParentalConsentDto>> GetPendingReviewAsync(CancellationToken cancellationToken = default);
}

public class ConsentService : IConsentService
{
    private readonly IParentalConsentRepository _repository;
    private readonly IVolunteerRepository _volunteerRepository;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<ConsentService> _logger;

    public ConsentService(
        IParentalConsentRepository repository,
        IVolunteerRepository volunteerRepository,
        IEmailService emailService,
        IMapper mapper,
        ILogger<ConsentService> logger)
    {
        _repository = repository;
        _volunteerRepository = volunteerRepository;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ParentalConsentDto> RequestConsentAsync(Guid volunteerId, RequestConsentDto dto, CancellationToken cancellationToken = default)
    {
        var volunteer = await _volunteerRepository.GetByIdAsync(volunteerId, cancellationToken);
        if (volunteer == null)
            throw new InvalidOperationException($"Volunteer {volunteerId} not found");

        if (!volunteer.IsMinor)
            throw new InvalidOperationException("Parental consent not required for adult volunteers");

        var existing = await _repository.GetByVolunteerIdAsync(volunteerId, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException("Consent request already exists");

        var consent = new ParentalConsent
        {
            Id = Guid.NewGuid(),
            VolunteerId = volunteerId,
            GuardianName = dto.GuardianName,
            GuardianEmail = dto.GuardianEmail,
            GuardianPhone = dto.GuardianPhone,
            ConsentStatus = ConsentStatus.Requested,
            IdentityVerificationStatus = IdentityVerificationStatus.NotVerified,
            ExpiresAt = CalculateExpiryDate(volunteer.DateOfBirth),
            AuditTrail = JsonSerializer.Serialize(new[] { new { Status = "Requested", Timestamp = DateTime.UtcNow } })
        };

        var created = await _repository.AddAsync(consent, cancellationToken);

        // Send email to guardian
        await _emailService.SendParentalConsentRequestAsync(
            dto.GuardianEmail,
            $"{volunteer.FirstName} {volunteer.LastName}",
            volunteerId,
            volunteer.LanguagePreference,
            cancellationToken);

        _logger.LogInformation("Parental consent requested for volunteer {VolunteerId}, guardian email: {GuardianEmail}", volunteerId, dto.GuardianEmail);

        return _mapper.Map<ParentalConsentDto>(created);
    }

    public async Task<ParentalConsentDto> SubmitConsentAsync(Guid volunteerId, SubmitConsentDto dto, CancellationToken cancellationToken = default)
    {
        var consent = await _repository.GetByVolunteerIdAsync(volunteerId, cancellationToken);
        if (consent == null)
            throw new InvalidOperationException("Consent request not found");

        consent.ConsentStatus = ConsentStatus.Submitted;
        consent.ConsentFormUrl = dto.ConsentFormUrl;
        consent.SubmittedAt = DateTime.UtcNow;
        
        var auditTrail = JsonSerializer.Deserialize<List<object>>(consent.AuditTrail ?? "[]") ?? new List<object>();
        auditTrail.Add(new { Status = "Submitted", Timestamp = DateTime.UtcNow });
        consent.AuditTrail = JsonSerializer.Serialize(auditTrail);

        await _repository.UpdateAsync(consent, cancellationToken);
        _logger.LogInformation("Parental consent submitted for volunteer {VolunteerId}", volunteerId);

        return _mapper.Map<ParentalConsentDto>(consent);
    }

    public async Task<ParentalConsentDto> ReviewConsentAsync(Guid consentId, Guid reviewerId, ReviewConsentDto dto, CancellationToken cancellationToken = default)
    {
        var consent = await _repository.GetByIdAsync(consentId, cancellationToken);
        if (consent == null)
            throw new InvalidOperationException("Consent not found");

        consent.ConsentStatus = dto.Approved ? ConsentStatus.Approved : ConsentStatus.Rejected;
        consent.ReviewedAt = DateTime.UtcNow;
        consent.ReviewerId = reviewerId;
        consent.ReviewerNotes = dto.ReviewerNotes;

        var auditTrail = JsonSerializer.Deserialize<List<object>>(consent.AuditTrail ?? "[]") ?? new List<object>();
        auditTrail.Add(new { Status = dto.Approved ? "Approved" : "Rejected", Timestamp = DateTime.UtcNow, ReviewerId = reviewerId });
        consent.AuditTrail = JsonSerializer.Serialize(auditTrail);

        await _repository.UpdateAsync(consent, cancellationToken);
        _logger.LogInformation("Parental consent {ConsentId} reviewed by {ReviewerId}: {Status}", consentId, reviewerId, consent.ConsentStatus);

        return _mapper.Map<ParentalConsentDto>(consent);
    }

    public async Task<ParentalConsentDto?> GetByVolunteerIdAsync(Guid volunteerId, CancellationToken cancellationToken = default)
    {
        var consent = await _repository.GetByVolunteerIdAsync(volunteerId, cancellationToken);
        return consent == null ? null : _mapper.Map<ParentalConsentDto>(consent);
    }

    public async Task<List<ParentalConsentDto>> GetPendingReviewAsync(CancellationToken cancellationToken = default)
    {
        var consents = await _repository.GetPendingReviewAsync(cancellationToken);
        return _mapper.Map<List<ParentalConsentDto>>(consents);
    }

    private DateTime CalculateExpiryDate(DateTime dateOfBirth)
    {
        var turns18 = dateOfBirth.AddYears(18);
        var twelveMonthsFromNow = DateTime.UtcNow.AddYears(1);
        return turns18 < twelveMonthsFromNow ? turns18 : twelveMonthsFromNow;
    }
}
