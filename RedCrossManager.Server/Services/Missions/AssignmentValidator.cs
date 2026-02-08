using System;
using System.Linq;
using System.Threading.Tasks;
using RedCrossManager.Server.Repositories;

namespace RedCrossManager.Server.Services.Missions;

public interface IAssignmentValidator
{
    Task<bool> HasTimeConflictAsync(Guid volunteerId, DateTime missionStart, DateTime missionEnd, int travelBufferMinutes);
}

public class AssignmentValidator : IAssignmentValidator
{
    private readonly IAssignmentRepository _assignmentRepository;

    public AssignmentValidator(IAssignmentRepository assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    public async Task<bool> HasTimeConflictAsync(Guid volunteerId, DateTime missionStart, DateTime missionEnd, int travelBufferMinutes)
    {
        var bufferStart = missionStart.AddMinutes(-travelBufferMinutes);
        var bufferEnd = missionEnd.AddMinutes(travelBufferMinutes);

        var assignments = await _assignmentRepository.GetVolunteerAssignmentsInTimeRangeAsync(
            volunteerId,
            bufferStart,
            bufferEnd);

        return assignments.Any();
    }
}
