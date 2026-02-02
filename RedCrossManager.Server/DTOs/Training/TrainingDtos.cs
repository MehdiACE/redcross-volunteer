using System;

namespace RedCrossManager.Server.DTOs.Training
{
    public class CreateTrainingDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int MaxEnrollment { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LocationName { get; set; } = null!;
        public Guid CreatedByCoordinatorId { get; set; }
    }

    public class TrainingDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int MaxEnrollment { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LocationName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int EnrollmentCount { get; set; }
        public int AvailableSpots { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByCoordinatorId { get; set; }
    }

    public class TrainingDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int MaxEnrollment { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LocationName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int EnrollmentCount { get; set; }
        public int AvailableSpots { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EnrollTrainingDto
    {
        public Guid VolunteerId { get; set; }
        public string Status { get; set; } = "Enrolled";
    }

    public class TrainingEnrollmentDto
    {
        public Guid Id { get; set; }
        public Guid TrainingId { get; set; }
        public Guid VolunteerId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime EnrolledAt { get; set; }
        public string? CertificateNumber { get; set; }
        public DateTime? CertificateIssuedAt { get; set; }
    }

    public class MarkAttendanceDto
    {
        public Guid EnrollmentId { get; set; }
        public bool Attended { get; set; }
        public string? CertificateNumber { get; set; }
    }

    public class TrainingFilterDto
    {
        public string? Category { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public bool? AvailableSpotsOnly { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
