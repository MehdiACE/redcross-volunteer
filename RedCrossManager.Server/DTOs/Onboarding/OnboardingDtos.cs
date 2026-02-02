namespace RedCrossManager.Server.DTOs.Onboarding;

public record OnboardingStepDto(
    Guid Id,
    Guid VolunteerId,
    string StepType,
    string Status,
    DateTime? StartedAt,
    DateTime? SubmittedAt,
    DateTime? ApprovedAt,
    string? ReviewerNotes
);

public record OnboardingProgressDto(
    Guid VolunteerId,
    VolunteerBasicInfoDto Volunteer,
    List<OnboardingStepDto> Steps,
    int CompletedCount,
    int TotalCount,
    bool IsComplete,
    string CurrentStatus,
    bool IsMinor,
    bool ParentalConsentApproved,
    DateTime StartedAt,
    DateTime? CompletedAt
);

public record VolunteerBasicInfoDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone
);

public record SubmitStepDto(
    Guid StepId,
    List<Guid>? DocumentIds
);

public record ReviewStepDto(
    bool Approved,
    string? ReviewerNotes
);

public record AdminOnboardingStepDto(
    Guid Id,
    Guid VolunteerId,
    string VolunteerName,
    string VolunteerEmail,
    string StepType,
    string Status,
    DateTime? SubmittedAt
);
