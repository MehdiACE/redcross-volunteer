using AutoMapper;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Onboarding;
using RedCrossManager.Server.Repositories;

namespace RedCrossManager.Server.Services.Onboarding;

public interface IOnboardingService
{
    Task<OnboardingProgressDto> GetProgressAsync(Guid volunteerId, CancellationToken cancellationToken = default);
    Task<OnboardingProgressDto?> GetProgressByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task InitializeStepsAsync(Guid volunteerId, CancellationToken cancellationToken = default);
    Task<OnboardingStepDto> SubmitStepAsync(Guid volunteerId, SubmitStepDto dto, CancellationToken cancellationToken = default);
    Task<OnboardingStepDto> ReviewStepAsync(Guid stepId, Guid reviewerId, ReviewStepDto dto, CancellationToken cancellationToken = default);
}

public class OnboardingService : IOnboardingService
{
    private readonly IOnboardingStepRepository _stepRepository;
    private readonly IVolunteerRepository _volunteerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<OnboardingService> _logger;

    public OnboardingService(
        IOnboardingStepRepository stepRepository,
        IVolunteerRepository volunteerRepository,
        IMapper mapper,
        ILogger<OnboardingService> logger)
    {
        _stepRepository = stepRepository;
        _volunteerRepository = volunteerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OnboardingProgressDto> GetProgressAsync(Guid volunteerId, CancellationToken cancellationToken = default)
    {
        var volunteer = await _volunteerRepository.GetByIdAsync(volunteerId, cancellationToken);
        if (volunteer == null)
            throw new InvalidOperationException($"Volunteer {volunteerId} not found");

        var steps = await _stepRepository.GetByVolunteerIdAsync(volunteerId, cancellationToken);
        
        if (steps.Count == 0)
        {
            await InitializeStepsAsync(volunteerId, cancellationToken);
            steps = await _stepRepository.GetByVolunteerIdAsync(volunteerId, cancellationToken);
        }

        var stepDtos = _mapper.Map<List<OnboardingStepDto>>(steps);
        var completedCount = steps.Count(s => s.Status == StepStatus.Approved);
        var currentStep = steps.FirstOrDefault(s => s.Status != StepStatus.Approved)?.StepType.ToString() ?? "Completed";

        var volunteerInfo = new VolunteerBasicInfoDto(
            volunteer.FirstName,
            volunteer.LastName,
            volunteer.Email,
            volunteer.Phone
        );

        return new OnboardingProgressDto(
            volunteerId,
            volunteerInfo,
            stepDtos,
            completedCount,
            steps.Count,
            completedCount == steps.Count,
            volunteer.Status.ToString(),
            volunteer.IsMinor,
            volunteer.ParentalConsent?.ConsentStatus == Domain.Entities.ConsentStatus.Approved,
            volunteer.RegisteredAt,
            completedCount == steps.Count ? DateTime.UtcNow : null
        );
    }

    public async Task<OnboardingProgressDto?> GetProgressByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var volunteer = await _volunteerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (volunteer == null)
        {
            _logger.LogWarning("No volunteer profile found for user {UserId}", userId);
            return null;
        }

        var steps = await _stepRepository.GetByVolunteerIdAsync(volunteer.Id, cancellationToken);
        
        if (steps.Count == 0)
        {
            await InitializeStepsAsync(volunteer.Id, cancellationToken);
            steps = await _stepRepository.GetByVolunteerIdAsync(volunteer.Id, cancellationToken);
        }

        var stepDtos = _mapper.Map<List<OnboardingStepDto>>(steps);
        var completedCount = steps.Count(s => s.Status == StepStatus.Approved);

        var volunteerInfo = new VolunteerBasicInfoDto(
            volunteer.FirstName,
            volunteer.LastName,
            volunteer.Email,
            volunteer.Phone
        );

        return new OnboardingProgressDto(
            volunteer.Id,
            volunteerInfo,
            stepDtos,
            completedCount,
            steps.Count,
            completedCount == steps.Count,
            volunteer.Status.ToString(),
            volunteer.IsMinor,
            volunteer.ParentalConsent?.ConsentStatus == Domain.Entities.ConsentStatus.Approved,
            volunteer.RegisteredAt,
            completedCount == steps.Count ? DateTime.UtcNow : null
        );
    }

    public async Task InitializeStepsAsync(Guid volunteerId, CancellationToken cancellationToken = default)
    {
        var volunteer = await _volunteerRepository.GetByIdAsync(volunteerId, cancellationToken);
        if (volunteer == null)
            throw new InvalidOperationException($"Volunteer {volunteerId} not found");

        var stepTypes = new[] { StepType.DocumentVerification, StepType.OrientationTraining, StepType.Certification, StepType.FinalReview };
        
        foreach (var stepType in stepTypes)
        {
            var step = new OnboardingStep
            {
                Id = Guid.NewGuid(),
                VolunteerId = volunteerId,
                StepType = stepType,
                Status = StepStatus.NotStarted
            };
            await _stepRepository.AddAsync(step, cancellationToken);
        }

        _logger.LogInformation("Initialized onboarding steps for volunteer {VolunteerId}", volunteerId);
    }

    public async Task<OnboardingStepDto> SubmitStepAsync(Guid volunteerId, SubmitStepDto dto, CancellationToken cancellationToken = default)
    {
        var step = await _stepRepository.GetByIdAsync(dto.StepId, cancellationToken);
        if (step == null || step.VolunteerId != volunteerId)
            throw new InvalidOperationException("Step not found or access denied");

        step.Status = StepStatus.Submitted;
        step.SubmittedAt = DateTime.UtcNow;
        if (dto.DocumentIds != null && dto.DocumentIds.Count > 0)
        {
            step.RelatedDocumentIds = System.Text.Json.JsonSerializer.Serialize(dto.DocumentIds);
        }

        await _stepRepository.UpdateAsync(step, cancellationToken);
        _logger.LogInformation("Onboarding step {StepId} submitted by volunteer {VolunteerId}", dto.StepId, volunteerId);

        return _mapper.Map<OnboardingStepDto>(step);
    }

    public async Task<OnboardingStepDto> ReviewStepAsync(Guid stepId, Guid reviewerId, ReviewStepDto dto, CancellationToken cancellationToken = default)
    {
        var step = await _stepRepository.GetByIdAsync(stepId, cancellationToken);
        if (step == null)
            throw new InvalidOperationException("Step not found");

        step.Status = dto.Approved ? StepStatus.Approved : StepStatus.Rejected;
        step.ApprovedAt = dto.Approved ? DateTime.UtcNow : null;
        step.ReviewerId = reviewerId;
        step.ReviewerNotes = dto.ReviewerNotes;

        await _stepRepository.UpdateAsync(step, cancellationToken);
        _logger.LogInformation("Onboarding step {StepId} reviewed by {ReviewerId}: {Status}", stepId, reviewerId, step.Status);

        return _mapper.Map<OnboardingStepDto>(step);
    }
}
