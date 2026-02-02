namespace RedCrossManager.Server.DTOs.Dashboard;

public record VolunteerDashboardDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime DateOfBirth,
    string Status,
    string LanguagePreference,
    DateTime RegisteredAt,
    bool IsMinor,
    bool SmsOptIn,
    DashboardOnboardingSummaryDto Onboarding,
    IReadOnlyList<DashboardAssignmentDto> UpcomingAssignments,
    IReadOnlyList<DashboardTrainingDto> Trainings,
    IReadOnlyList<DashboardCertificationDto> Certifications,
    IReadOnlyList<DashboardAlertDto> Alerts
);

public record DashboardOnboardingSummaryDto(
    int CompletedCount,
    int TotalCount,
    int CurrentStepNumber,
    string CurrentStep,
    bool IsComplete,
    bool IsMinor,
    bool ParentalConsentApproved
);

public record DashboardAssignmentDto(
    Guid Id,
    string Title,
    DateTime StartAt,
    DateTime EndAt,
    string Location,
    string Status,
    string? RoleDescription
);

public record DashboardTrainingDto(
    Guid Id,
    string Title,
    string Category,
    DateTime StartAt,
    DateTime EndAt,
    string Status,
    string? CertificateUrl
);

public record DashboardCertificationDto(
    Guid Id,
    string Type,
    DateTime IssuedAt,
    DateTime ExpiresAt,
    string Status
);

public record DashboardAlertDto(
    string Type,
    string Message,
    DateTime? DueAt,
    string? ActionUrl
);
