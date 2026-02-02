using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Training;
using RedCrossManager.Server.Repositories;

namespace RedCrossManager.Server.Services.Trainings
{
    public interface ITrainingService
    {
        Task<TrainingDto> CreateTrainingAsync(CreateTrainingDto dto);
        Task<TrainingDto> GetTrainingAsync(Guid id);
        Task<List<TrainingDto>> GetAllPublishedTrainingsAsync();
        Task<List<TrainingDto>> GetFilteredTrainingsAsync(TrainingFilterDto filter);
        Task<TrainingDetailDto> GetTrainingDetailAsync(Guid id);
        Task<TrainingEnrollmentDto> EnrollVolunteerAsync(Guid trainingId, EnrollTrainingDto dto);
        Task<TrainingEnrollmentDto> MarkAttendanceAsync(Guid trainingId, MarkAttendanceDto dto);
        Task<List<TrainingEnrollmentDto>> GetTrainingEnrollmentsAsync(Guid trainingId);
        Task<List<TrainingEnrollmentDto>> GetVolunteerTrainingsAsync(Guid volunteerId);
    }

    public class TrainingService : ITrainingService
    {
        private readonly ITrainingRepository _trainingRepository;
        private readonly ITrainingEnrollmentRepository _enrollmentRepository;
        private readonly ILogger<TrainingService> _logger;

        public TrainingService(
            ITrainingRepository trainingRepository,
            ITrainingEnrollmentRepository enrollmentRepository,
            ILogger<TrainingService> logger)
        {
            _trainingRepository = trainingRepository;
            _enrollmentRepository = enrollmentRepository;
            _logger = logger;
        }

        public async Task<TrainingDto> CreateTrainingAsync(CreateTrainingDto dto)
        {
            try
            {
                var training = new Training
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Description = dto.Description,
                    Category = dto.Category,
                    MaxEnrollment = dto.MaxEnrollment,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    LocationName = dto.LocationName,
                    Status = "Published",
                    CreatedByCoordinatorId = dto.CreatedByCoordinatorId
                };

                await _trainingRepository.CreateAsync(training);

                _logger.LogInformation($"Training created: {training.Id} - {training.Title}");

                return MapToDto(training);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create training");
                throw;
            }
        }

        public async Task<TrainingDto> GetTrainingAsync(Guid id)
        {
            var training = await _trainingRepository.GetByIdAsync(id);
            if (training == null)
            {
                throw new KeyNotFoundException($"Training not found: {id}");
            }

            return MapToDto(training);
        }

        public async Task<List<TrainingDto>> GetAllPublishedTrainingsAsync()
        {
            var trainings = await _trainingRepository.GetAllPublishedAsync();
            return trainings.Select(MapToDto).ToList();
        }

        public async Task<List<TrainingDto>> GetFilteredTrainingsAsync(TrainingFilterDto filter)
        {
            var trainings = await _trainingRepository.GetByFilterAsync(
                filter.Category,
                filter.StartDateFrom,
                filter.StartDateTo,
                filter.AvailableSpotsOnly);

            return trainings
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<TrainingDetailDto> GetTrainingDetailAsync(Guid id)
        {
            var training = await _trainingRepository.GetByIdAsync(id);
            if (training == null)
            {
                throw new KeyNotFoundException($"Training not found: {id}");
            }

            var enrolledCount = training.Enrollments.Count(e => e.Status == "Enrolled");
            var availableSpots = training.MaxEnrollment - enrolledCount;

            return new TrainingDetailDto
            {
                Id = training.Id,
                Title = training.Title,
                Description = training.Description,
                Category = training.Category,
                MaxEnrollment = training.MaxEnrollment,
                StartDate = training.StartDate,
                EndDate = training.EndDate,
                LocationName = training.LocationName,
                Status = training.Status,
                EnrollmentCount = enrolledCount,
                AvailableSpots = Math.Max(0, availableSpots),
                CreatedAt = training.CreatedAt
            };
        }

        public async Task<TrainingEnrollmentDto> EnrollVolunteerAsync(Guid trainingId, EnrollTrainingDto dto)
        {
            try
            {
                var training = await _trainingRepository.GetByIdAsync(trainingId);
                if (training == null)
                {
                    throw new KeyNotFoundException($"Training not found: {trainingId}");
                }

                // Check if already enrolled
                var existing = await _enrollmentRepository.GetByTrainingAndVolunteerAsync(trainingId, dto.VolunteerId);
                if (existing != null)
                {
                    throw new InvalidOperationException("Volunteer is already enrolled in this training");
                }

                // Check enrollment capacity
                var enrolledCount = training.Enrollments.Count(e => e.Status == "Enrolled");
                var status = enrolledCount >= training.MaxEnrollment ? "Waitlisted" : "Enrolled";

                var enrollment = new TrainingEnrollment
                {
                    Id = Guid.NewGuid(),
                    TrainingId = trainingId,
                    VolunteerId = dto.VolunteerId,
                    Status = status
                };

                await _enrollmentRepository.CreateAsync(enrollment);

                _logger.LogInformation($"Volunteer {dto.VolunteerId} enrolled in training {trainingId} with status {status}");

                return MapEnrollmentToDto(enrollment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to enroll volunteer {dto.VolunteerId} in training {trainingId}");
                throw;
            }
        }

        public async Task<TrainingEnrollmentDto> MarkAttendanceAsync(Guid trainingId, MarkAttendanceDto dto)
        {
            try
            {
                var enrollment = await _enrollmentRepository.GetByIdAsync(dto.EnrollmentId);
                if (enrollment == null || enrollment.TrainingId != trainingId)
                {
                    throw new KeyNotFoundException($"Enrollment not found: {dto.EnrollmentId}");
                }

                if (dto.Attended)
                {
                    enrollment.Status = "Completed";
                    enrollment.AttendedAt = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(dto.CertificateNumber))
                    {
                        enrollment.CertificateNumber = dto.CertificateNumber;
                        enrollment.CertificateIssuedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    enrollment.Status = "Cancelled";
                }

                await _enrollmentRepository.UpdateAsync(enrollment);

                _logger.LogInformation($"Attendance marked for enrollment {dto.EnrollmentId}: Attended={dto.Attended}");

                return MapEnrollmentToDto(enrollment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark attendance");
                throw;
            }
        }

        public async Task<List<TrainingEnrollmentDto>> GetTrainingEnrollmentsAsync(Guid trainingId)
        {
            var enrollments = await _enrollmentRepository.GetByTrainingIdAsync(trainingId);
            return enrollments.Select(MapEnrollmentToDto).ToList();
        }

        public async Task<List<TrainingEnrollmentDto>> GetVolunteerTrainingsAsync(Guid volunteerId)
        {
            var enrollments = await _enrollmentRepository.GetByVolunteerIdAsync(volunteerId);
            return enrollments.Select(MapEnrollmentToDto).ToList();
        }

        private TrainingDto MapToDto(Training training)
        {
            var enrolledCount = training.Enrollments.Count(e => e.Status == "Enrolled");
            var availableSpots = training.MaxEnrollment - enrolledCount;

            return new TrainingDto
            {
                Id = training.Id,
                Title = training.Title,
                Description = training.Description,
                Category = training.Category,
                MaxEnrollment = training.MaxEnrollment,
                StartDate = training.StartDate,
                EndDate = training.EndDate,
                LocationName = training.LocationName,
                Status = training.Status,
                EnrollmentCount = enrolledCount,
                AvailableSpots = Math.Max(0, availableSpots),
                CreatedAt = training.CreatedAt,
                CreatedByCoordinatorId = training.CreatedByCoordinatorId
            };
        }

        private TrainingEnrollmentDto MapEnrollmentToDto(TrainingEnrollment enrollment)
        {
            return new TrainingEnrollmentDto
            {
                Id = enrollment.Id,
                TrainingId = enrollment.TrainingId,
                VolunteerId = enrollment.VolunteerId,
                Status = enrollment.Status,
                EnrolledAt = enrollment.EnrolledAt,
                CertificateNumber = enrollment.CertificateNumber,
                CertificateIssuedAt = enrollment.CertificateIssuedAt
            };
        }
    }
}
