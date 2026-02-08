using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Repositories;
using RedCrossManager.Server.Services.Missions;
using Xunit;

namespace RedCrossManager.Server.Tests.Unit;

public class AssignmentValidatorTests
{
    [Fact]
    public async Task HasTimeConflictAsync_ReturnsTrue_WhenAssignmentOverlapsWithBuffer()
    {
        var volunteerId = Guid.NewGuid();
        var missionStart = DateTime.UtcNow.AddDays(1);
        var missionEnd = missionStart.AddHours(4);

        var existingMission = new Mission
        {
            Id = Guid.NewGuid(),
            Title = "Existing",
            Description = "Existing mission",
            MissionType = MissionType.Other,
            Location = "Center",
            StartAt = missionStart.AddHours(-1),
            EndAt = missionStart.AddHours(1),
            VolunteersNeeded = 2,
            CreatedBy = Guid.NewGuid()
        };

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            MissionId = existingMission.Id,
            VolunteerId = volunteerId,
            Mission = existingMission
        };

        var repository = new FakeAssignmentRepository(new List<Assignment> { assignment });
        var validator = new AssignmentValidator(repository);

        var hasConflict = await validator.HasTimeConflictAsync(volunteerId, missionStart, missionEnd, 120);

        Assert.True(hasConflict);
    }

    [Fact]
    public async Task HasTimeConflictAsync_ReturnsFalse_WhenNoOverlap()
    {
        var volunteerId = Guid.NewGuid();
        var missionStart = DateTime.UtcNow.AddDays(1);
        var missionEnd = missionStart.AddHours(2);

        var existingMission = new Mission
        {
            Id = Guid.NewGuid(),
            Title = "Existing",
            Description = "Existing mission",
            MissionType = MissionType.Other,
            Location = "Center",
            StartAt = missionStart.AddDays(-2),
            EndAt = missionStart.AddDays(-2).AddHours(2),
            VolunteersNeeded = 2,
            CreatedBy = Guid.NewGuid()
        };

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            MissionId = existingMission.Id,
            VolunteerId = volunteerId,
            Mission = existingMission
        };

        var repository = new FakeAssignmentRepository(new List<Assignment> { assignment });
        var validator = new AssignmentValidator(repository);

        var hasConflict = await validator.HasTimeConflictAsync(volunteerId, missionStart, missionEnd, 120);

        Assert.False(hasConflict);
    }

    private sealed class FakeAssignmentRepository : IAssignmentRepository
    {
        private readonly List<Assignment> _assignments;

        public FakeAssignmentRepository(List<Assignment> assignments)
        {
            _assignments = assignments;
        }

        public Task<IEnumerable<Assignment>> GetVolunteerAssignmentsInTimeRangeAsync(Guid volunteerId, DateTime start, DateTime end)
        {
            var results = _assignments
                .Where(a => a.VolunteerId == volunteerId)
                .Where(a =>
                    a.Mission.StartAt < end &&
                    a.Mission.EndAt > start)
                .ToList();

            return Task.FromResult<IEnumerable<Assignment>>(results);
        }

        public Task<Assignment?> GetByIdAsync(Guid id) => Task.FromResult<Assignment?>(null);
        public Task<IEnumerable<Assignment>> GetByMissionIdAsync(Guid missionId) => Task.FromResult<IEnumerable<Assignment>>(Array.Empty<Assignment>());
        public Task<IEnumerable<Assignment>> GetByVolunteerIdAsync(Guid volunteerId) => Task.FromResult<IEnumerable<Assignment>>(Array.Empty<Assignment>());
        public Task<Assignment> CreateAsync(Assignment assignment) => Task.FromResult(assignment);
        public Task UpdateAsync(Assignment assignment) => Task.CompletedTask;
        public Task DeleteAsync(Guid id) => Task.CompletedTask;
        public Task<bool> HasConflictingAssignmentAsync(Guid volunteerId, DateTime missionStart, DateTime missionEnd, int travelBufferMinutes)
            => Task.FromResult(false);
    }
}
