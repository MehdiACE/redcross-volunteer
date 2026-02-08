using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Repositories;

public class MissionRepository : IMissionRepository
{
    private readonly RedCrossDbContext _context;

    public MissionRepository(RedCrossDbContext context)
    {
        _context = context;
    }

    public async Task<Mission?> GetByIdAsync(Guid id)
    {
        return await _context.Missions
            .Include(m => m.Assignments)
                .ThenInclude(a => a.Volunteer)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Mission>> GetAllAsync(bool publishedOnly = true)
    {
        var query = _context.Missions.Include(m => m.Assignments).AsQueryable();
        
        if (publishedOnly)
        {
            query = query.Where(m => m.Published);
        }
        
        return await query.OrderBy(m => m.StartAt).ToListAsync();
    }

    public async Task<IEnumerable<Mission>> GetUpcomingMissionsAsync(DateTime? fromDate = null)
    {
        var startDate = fromDate ?? DateTime.UtcNow;
        
        return await _context.Missions
            .Include(m => m.Assignments)
            .Where(m => m.Published && m.StartAt >= startDate)
            .OrderBy(m => m.StartAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mission>> GetMissionsByTypeAsync(MissionType missionType)
    {
        return await _context.Missions
            .Include(m => m.Assignments)
            .Where(m => m.Published && m.MissionType == missionType)
            .OrderBy(m => m.StartAt)
            .ToListAsync();
    }

    public async Task<Mission> CreateAsync(Mission mission)
    {
        _context.Missions.Add(mission);
        await _context.SaveChangesAsync();
        return mission;
    }

    public async Task UpdateAsync(Mission mission)
    {
        _context.Missions.Update(mission);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var mission = await _context.Missions.FindAsync(id);
        if (mission != null)
        {
            _context.Missions.Remove(mission);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetAvailableSlotsAsync(Guid missionId)
    {
        var mission = await _context.Missions
            .Include(m => m.Assignments)
            .FirstOrDefaultAsync(m => m.Id == missionId);
        
        if (mission == null) return 0;
        
        var assignedCount = mission.Assignments.Count(a => 
            a.Status == AssignmentStatus.Confirmed || 
            a.Status == AssignmentStatus.Pending);
        
        return Math.Max(0, mission.VolunteersNeeded - assignedCount);
    }

    public async Task<IEnumerable<Mission>> GetQualifiedMissionsForVolunteerAsync(Guid volunteerId)
    {
        var volunteer = await _context.Volunteers
            .Include(v => v.Certifications)
            .FirstOrDefaultAsync(v => v.Id == volunteerId);
        
        if (volunteer == null) return Enumerable.Empty<Mission>();
        
        var volunteerCertTypes = volunteer.Certifications
            .Where(c => c.ExpiresAt > DateTime.UtcNow && c.VerificationStatus == VerificationStatus.Approved)
            .Select(c => c.Type.ToString())
            .ToHashSet();
        
        var missions = await _context.Missions
            .Include(m => m.Assignments)
            .Where(m => m.Published && m.StartAt > DateTime.UtcNow)
            .ToListAsync();
        
        // Filter missions where volunteer has all required certifications
        return missions.Where(m =>
        {
            if (string.IsNullOrEmpty(m.RequiredCertifications))
                return true;
            
            var required = System.Text.Json.JsonSerializer
                .Deserialize<List<string>>(m.RequiredCertifications) ?? new List<string>();
            
            return required.All(req => volunteerCertTypes.Contains(req));
        }).OrderBy(m => m.StartAt);
    }
}
