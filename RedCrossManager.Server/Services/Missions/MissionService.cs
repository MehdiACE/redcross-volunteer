using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Missions;
using RedCrossManager.Server.Repositories;

namespace RedCrossManager.Server.Services.Missions;

public interface IMissionService
{
    Task<MissionDto> CreateMissionAsync(CreateMissionDto dto);
    Task<MissionDto> GetMissionAsync(Guid id);
    Task<List<MissionDto>> GetMissionsAsync(bool publishedOnly = true);
    Task<AssignmentDto> ApplyToMissionAsync(Guid missionId, ApplyMissionDto dto);
    Task<List<AssignmentDto>> AssignVolunteersAsync(Guid missionId, AssignVolunteersDto dto);
    Task<AssignmentDto> ConfirmAssignmentAsync(Guid assignmentId, Guid volunteerId);
    Task<AssignmentDto> UpdateAssignmentStatusAsync(Guid assignmentId, UpdateAssignmentStatusDto dto);
}

public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IVolunteerRepository _volunteerRepository;
    private readonly IAssignmentValidator _assignmentValidator;
    private readonly ILogger<MissionService> _logger;

    public MissionService(
        IMissionRepository missionRepository,
        IAssignmentRepository assignmentRepository,
        IVolunteerRepository volunteerRepository,
        IAssignmentValidator assignmentValidator,
        ILogger<MissionService> logger)
    {
        _missionRepository = missionRepository;
        _assignmentRepository = assignmentRepository;
        _volunteerRepository = volunteerRepository;
        _assignmentValidator = assignmentValidator;
        _logger = logger;
    }

    public async Task<MissionDto> CreateMissionAsync(CreateMissionDto dto)
    {
        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            MissionType = Enum.TryParse<MissionType>(dto.MissionType, true, out var missionType)
                ? missionType
                : MissionType.Other,
            Location = dto.Location,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt,
            RequiredCertifications = SerializeCertifications(dto.RequiredCertifications),
            VolunteersNeeded = dto.VolunteersNeeded,
            TravelBufferMinutes = dto.TravelBufferMinutes,
            Published = dto.Published,
            CreatedBy = dto.CreatedByCoordinatorId,
            CreatedAt = DateTime.UtcNow
        };

        await _missionRepository.CreateAsync(mission);
        _logger.LogInformation("Mission created {MissionId}", mission.Id);

        return await MapToDtoAsync(mission);
    }

    public async Task<MissionDto> GetMissionAsync(Guid id)
    {
        var mission = await _missionRepository.GetByIdAsync(id);
        if (mission == null)
        {
            throw new KeyNotFoundException($"Mission not found: {id}");
        }

        return await MapToDtoAsync(mission);
    }

    public async Task<List<MissionDto>> GetMissionsAsync(bool publishedOnly = true)
    {
        var missions = await _missionRepository.GetAllAsync(publishedOnly);
        var result = new List<MissionDto>();
        foreach (var mission in missions)
        {
            result.Add(await MapToDtoAsync(mission));
        }
        return result;
    }

    public async Task<AssignmentDto> ApplyToMissionAsync(Guid missionId, ApplyMissionDto dto)
    {
        var mission = await _missionRepository.GetByIdAsync(missionId);
        if (mission == null)
        {
            throw new KeyNotFoundException($"Mission not found: {missionId}");
        }

        var volunteer = await _volunteerRepository.GetByIdAsync(dto.VolunteerId);
        if (volunteer == null)
        {
            throw new KeyNotFoundException($"Volunteer not found: {dto.VolunteerId}");
        }

        var requiredCerts = DeserializeCertifications(mission.RequiredCertifications);
        if (requiredCerts.Count > 0 && !HasRequiredCertifications(volunteer, requiredCerts))
        {
            throw new InvalidOperationException("Volunteer does not meet required certifications");
        }

        var availableSlots = await _missionRepository.GetAvailableSlotsAsync(missionId);
        if (availableSlots <= 0)
        {
            throw new InvalidOperationException("No available slots for this mission");
        }

        var hasConflict = await _assignmentValidator.HasTimeConflictAsync(
            dto.VolunteerId,
            mission.StartAt,
            mission.EndAt,
            mission.TravelBufferMinutes);

        if (hasConflict)
        {
            throw new InvalidOperationException("Volunteer has conflicting assignments");
        }

        var existingAssignments = await _assignmentRepository.GetByMissionIdAsync(missionId);
        var existing = existingAssignments.FirstOrDefault(a => a.VolunteerId == dto.VolunteerId);
        if (existing != null)
        {
            return MapAssignment(existing);
        }

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            MissionId = missionId,
            VolunteerId = dto.VolunteerId,
            Status = AssignmentStatus.Pending,
            AssignedAt = DateTime.UtcNow
        };

        await _assignmentRepository.CreateAsync(assignment);
        _logger.LogInformation("Volunteer {VolunteerId} applied to mission {MissionId}", dto.VolunteerId, missionId);

        return MapAssignment(assignment);
    }

    public async Task<List<AssignmentDto>> AssignVolunteersAsync(Guid missionId, AssignVolunteersDto dto)
    {
        var mission = await _missionRepository.GetByIdAsync(missionId);
        if (mission == null)
        {
            throw new KeyNotFoundException($"Mission not found: {missionId}");
        }

        var results = new List<AssignmentDto>();
        foreach (var volunteerId in dto.VolunteerIds)
        {
            var bufferStart = mission.StartAt.AddMinutes(-mission.TravelBufferMinutes);
            var bufferEnd = mission.EndAt.AddMinutes(mission.TravelBufferMinutes);
            var conflictingAssignments = await _assignmentRepository.GetVolunteerAssignmentsInTimeRangeAsync(
                volunteerId,
                bufferStart,
                bufferEnd);

            if (conflictingAssignments.Any(a => a.MissionId != missionId))
            {
                throw new InvalidOperationException("Volunteer has conflicting assignments");
            }

            var existingAssignments = await _assignmentRepository.GetByMissionIdAsync(missionId);
            var existing = existingAssignments.FirstOrDefault(a => a.VolunteerId == volunteerId);
            if (existing == null)
            {
                existing = new Assignment
                {
                    Id = Guid.NewGuid(),
                    MissionId = missionId,
                    VolunteerId = volunteerId,
                    Status = AssignmentStatus.Confirmed,
                    RoleDescription = dto.RoleDescription,
                    AssignedAt = DateTime.UtcNow
                };

                await _assignmentRepository.CreateAsync(existing);
            }
            else
            {
                existing.Status = AssignmentStatus.Confirmed;
                existing.RoleDescription = dto.RoleDescription ?? existing.RoleDescription;
                await _assignmentRepository.UpdateAsync(existing);
            }

            results.Add(MapAssignment(existing));
        }

        return results;
    }

    public async Task<AssignmentDto> ConfirmAssignmentAsync(Guid assignmentId, Guid volunteerId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
        {
            throw new KeyNotFoundException($"Assignment not found: {assignmentId}");
        }

        if (assignment.VolunteerId != volunteerId)
        {
            throw new InvalidOperationException("Assignment does not belong to volunteer");
        }

        assignment.Status = AssignmentStatus.Confirmed;
        await _assignmentRepository.UpdateAsync(assignment);

        return MapAssignment(assignment);
    }

    public async Task<AssignmentDto> UpdateAssignmentStatusAsync(Guid assignmentId, UpdateAssignmentStatusDto dto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
        {
            throw new KeyNotFoundException($"Assignment not found: {assignmentId}");
        }

        if (!Enum.TryParse<AssignmentStatus>(dto.Status, true, out var status))
        {
            throw new InvalidOperationException("Invalid assignment status");
        }

        assignment.Status = status;
        if (status == AssignmentStatus.Completed)
        {
            assignment.ReminderSentAt = DateTime.UtcNow;
        }

        await _assignmentRepository.UpdateAsync(assignment);
        return MapAssignment(assignment);
    }

    private static bool HasRequiredCertifications(Volunteer volunteer, List<string> required)
    {
        var certs = volunteer.Certifications
            .Where(c => c.ExpiresAt > DateTime.UtcNow && c.VerificationStatus == VerificationStatus.Approved)
            .Select(c => c.Type.ToString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return required.All(req => certs.Contains(req));
    }

    private async Task<MissionDto> MapToDtoAsync(Mission mission)
    {
        var availableSlots = await _missionRepository.GetAvailableSlotsAsync(mission.Id);
        return new MissionDto
        {
            Id = mission.Id,
            Title = mission.Title,
            Description = mission.Description,
            MissionType = mission.MissionType.ToString(),
            Location = mission.Location,
            StartAt = mission.StartAt,
            EndAt = mission.EndAt,
            RequiredCertifications = DeserializeCertifications(mission.RequiredCertifications),
            VolunteersNeeded = mission.VolunteersNeeded,
            TravelBufferMinutes = mission.TravelBufferMinutes,
            Published = mission.Published,
            CreatedAt = mission.CreatedAt,
            CreatedBy = mission.CreatedBy,
            AvailableSlots = availableSlots
        };
    }

    private static AssignmentDto MapAssignment(Assignment assignment)
    {
        return new AssignmentDto
        {
            Id = assignment.Id,
            MissionId = assignment.MissionId,
            VolunteerId = assignment.VolunteerId,
            Status = assignment.Status.ToString(),
            RoleDescription = assignment.RoleDescription,
            AssignedAt = assignment.AssignedAt,
            ReminderSentAt = assignment.ReminderSentAt
        };
    }

    private static string SerializeCertifications(List<string> certifications)
    {
        return JsonSerializer.Serialize(certifications ?? new List<string>());
    }

    private static List<string> DeserializeCertifications(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<string>();
        }

        return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
    }
}
