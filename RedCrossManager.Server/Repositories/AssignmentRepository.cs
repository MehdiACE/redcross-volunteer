using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Repositories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly RedCrossDbContext _context;

    public AssignmentRepository(RedCrossDbContext context)
    {
        _context = context;
    }

    public async Task<Assignment?> GetByIdAsync(Guid id)
    {
        return await _context.Assignments
            .Include(a => a.Mission)
            .Include(a => a.Volunteer)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Assignment>> GetByMissionIdAsync(Guid missionId)
    {
        return await _context.Assignments
            .Include(a => a.Volunteer)
            .Where(a => a.MissionId == missionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Assignment>> GetByVolunteerIdAsync(Guid volunteerId)
    {
        return await _context.Assignments
            .Include(a => a.Mission)
            .Where(a => a.VolunteerId == volunteerId)
            .OrderByDescending(a => a.Mission.StartAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Assignment>> GetVolunteerAssignmentsInTimeRangeAsync(
        Guid volunteerId, 
        DateTime start, 
        DateTime end)
    {
        return await _context.Assignments
            .Include(a => a.Mission)
            .Where(a => 
                a.VolunteerId == volunteerId &&
                a.Status != AssignmentStatus.Cancelled &&
                a.Status != AssignmentStatus.NoShow &&
                ((a.Mission.StartAt >= start && a.Mission.StartAt < end) ||
                 (a.Mission.EndAt > start && a.Mission.EndAt <= end) ||
                 (a.Mission.StartAt <= start && a.Mission.EndAt >= end)))
            .ToListAsync();
    }

    public async Task<Assignment> CreateAsync(Assignment assignment)
    {
        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task UpdateAsync(Assignment assignment)
    {
        _context.Assignments.Update(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment != null)
        {
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasConflictingAssignmentAsync(
        Guid volunteerId, 
        DateTime missionStart, 
        DateTime missionEnd, 
        int travelBufferMinutes)
    {
        // Add travel buffer to the time range
        var bufferStart = missionStart.AddMinutes(-travelBufferMinutes);
        var bufferEnd = missionEnd.AddMinutes(travelBufferMinutes);

        var conflictingAssignments = await GetVolunteerAssignmentsInTimeRangeAsync(
            volunteerId, 
            bufferStart, 
            bufferEnd);

        return conflictingAssignments.Any();
    }
}
