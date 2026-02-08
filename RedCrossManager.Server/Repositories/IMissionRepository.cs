using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Repositories;

public interface IMissionRepository
{
    Task<Mission?> GetByIdAsync(Guid id);
    Task<IEnumerable<Mission>> GetAllAsync(bool publishedOnly = true);
    Task<IEnumerable<Mission>> GetUpcomingMissionsAsync(DateTime? fromDate = null);
    Task<IEnumerable<Mission>> GetMissionsByTypeAsync(MissionType missionType);
    Task<Mission> CreateAsync(Mission mission);
    Task UpdateAsync(Mission mission);
    Task DeleteAsync(Guid id);
    Task<int> GetAvailableSlotsAsync(Guid missionId);
    Task<IEnumerable<Mission>> GetQualifiedMissionsForVolunteerAsync(Guid volunteerId);
}
