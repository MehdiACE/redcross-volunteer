using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Repositories;

public interface IAssignmentRepository
{
    Task<Assignment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Assignment>> GetByMissionIdAsync(Guid missionId);
    Task<IEnumerable<Assignment>> GetByVolunteerIdAsync(Guid volunteerId);
    Task<IEnumerable<Assignment>> GetVolunteerAssignmentsInTimeRangeAsync(Guid volunteerId, DateTime start, DateTime end);
    Task<Assignment> CreateAsync(Assignment assignment);
    Task UpdateAsync(Assignment assignment);
    Task DeleteAsync(Guid id);
    Task<bool> HasConflictingAssignmentAsync(Guid volunteerId, DateTime missionStart, DateTime missionEnd, int travelBufferMinutes);
}
