using RedCrossManager.Server.DTOs.Dashboard;
using RedCrossManager.Server.DTOs.Onboarding;
using RedCrossManager.Server.Repositories;
using RedCrossManager.Server.Services.Onboarding;

namespace RedCrossManager.Server.Services.Dashboard;

public interface IDashboardService
{
    Task<VolunteerDashboardDto?> GetDashboardByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class DashboardService : IDashboardService
{
    private readonly IVolunteerRepository _volunteerRepository;
    private readonly IOnboardingService _onboardingService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IVolunteerRepository volunteerRepository,
        IOnboardingService onboardingService,
        ILogger<DashboardService> logger)
    {
        _volunteerRepository = volunteerRepository;
        _onboardingService = onboardingService;
        _logger = logger;
    }

    public async Task<VolunteerDashboardDto?> GetDashboardByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var volunteer = await _volunteerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (volunteer == null)
        {
            _logger.LogWarning("No volunteer profile found for user {UserId}", userId);
            return null;
        }

        var progress = await _onboardingService.GetProgressByUserIdAsync(userId, cancellationToken);
        var onboardingSummary = BuildOnboardingSummary(progress);

        return new VolunteerDashboardDto(
            Id: volunteer.Id,
            FirstName: volunteer.FirstName,
            LastName: volunteer.LastName,
            Email: volunteer.Email,
            Phone: volunteer.Phone,
            DateOfBirth: volunteer.DateOfBirth,
            Status: volunteer.Status.ToString(),
            LanguagePreference: volunteer.LanguagePreference,
            RegisteredAt: volunteer.RegisteredAt,
            IsMinor: volunteer.IsMinor,
            SmsOptIn: volunteer.SmsOptIn,
            Onboarding: onboardingSummary,
            UpcomingAssignments: Array.Empty<DashboardAssignmentDto>(),
            Trainings: Array.Empty<DashboardTrainingDto>(),
            Certifications: Array.Empty<DashboardCertificationDto>(),
            Alerts: Array.Empty<DashboardAlertDto>()
        );
    }

    private static DashboardOnboardingSummaryDto BuildOnboardingSummary(OnboardingProgressDto? progress)
    {
        if (progress == null)
        {
            return new DashboardOnboardingSummaryDto(
                CompletedCount: 0,
                TotalCount: 0,
                CurrentStepNumber: 0,
                CurrentStep: "NotStarted",
                IsComplete: false,
                IsMinor: false,
                ParentalConsentApproved: false
            );
        }

        var steps = progress.Steps;
        var currentStepIndex = steps.FindIndex(step => !string.Equals(step.Status, "Approved", StringComparison.OrdinalIgnoreCase));
        var currentStep = currentStepIndex >= 0 ? steps[currentStepIndex].StepType : "Completed";
        var currentStepNumber = currentStepIndex >= 0 ? currentStepIndex + 1 : steps.Count;

        return new DashboardOnboardingSummaryDto(
            CompletedCount: progress.CompletedCount,
            TotalCount: progress.TotalCount,
            CurrentStepNumber: currentStepNumber,
            CurrentStep: currentStep,
            IsComplete: progress.IsComplete,
            IsMinor: progress.IsMinor,
            ParentalConsentApproved: progress.ParentalConsentApproved
        );
    }
}
