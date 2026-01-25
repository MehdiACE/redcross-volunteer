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
    List<OnboardingStepDto> Steps,
    int CompletedCount,
    int TotalCount,
    bool IsComplete,
    string? CurrentStep
);

public record SubmitStepDto(
    Guid StepId,
    List<Guid>? DocumentIds
);

public record ReviewStepDto(
    bool Approved,
    string? ReviewerNotes
);
