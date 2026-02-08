using System;
using System.Collections.Generic;

namespace RedCrossManager.Server.DTOs.Missions;

public class CreateMissionDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string MissionType { get; set; } = null!;
    public string Location { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public List<string> RequiredCertifications { get; set; } = new();
    public int VolunteersNeeded { get; set; }
    public int TravelBufferMinutes { get; set; } = 120;
    public bool Published { get; set; } = true;
    public Guid CreatedByCoordinatorId { get; set; }
}

public class MissionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string MissionType { get; set; } = null!;
    public string Location { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public List<string> RequiredCertifications { get; set; } = new();
    public int VolunteersNeeded { get; set; }
    public int TravelBufferMinutes { get; set; }
    public bool Published { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public int AvailableSlots { get; set; }
}

public class ApplyMissionDto
{
    public Guid VolunteerId { get; set; }
}

public class AssignVolunteersDto
{
    public List<Guid> VolunteerIds { get; set; } = new();
    public string? RoleDescription { get; set; }
}

public class AssignmentDto
{
    public Guid Id { get; set; }
    public Guid MissionId { get; set; }
    public Guid VolunteerId { get; set; }
    public string Status { get; set; } = null!;
    public string? RoleDescription { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ReminderSentAt { get; set; }
}

public class UpdateAssignmentStatusDto
{
    public string Status { get; set; } = null!;
}
