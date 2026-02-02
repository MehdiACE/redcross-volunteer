using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Repositories
{
    public interface ITrainingRepository
    {
        Task<Training?> GetByIdAsync(Guid id);
        Task<List<Training>> GetAllPublishedAsync();
        Task<List<Training>> GetByFilterAsync(string? category, DateTime? startDateFrom, DateTime? startDateTo, bool? availableSpotsOnly);
        Task<Training> CreateAsync(Training training);
        Task<Training> UpdateAsync(Training training);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetEnrollmentCountAsync(Guid trainingId);
    }

    public interface ITrainingEnrollmentRepository
    {
        Task<TrainingEnrollment?> GetByIdAsync(Guid id);
        Task<List<TrainingEnrollment>> GetByTrainingIdAsync(Guid trainingId);
        Task<List<TrainingEnrollment>> GetByVolunteerIdAsync(Guid volunteerId);
        Task<TrainingEnrollment?> GetByTrainingAndVolunteerAsync(Guid trainingId, Guid volunteerId);
        Task<TrainingEnrollment> CreateAsync(TrainingEnrollment enrollment);
        Task<TrainingEnrollment> UpdateAsync(TrainingEnrollment enrollment);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetEnrolledCountAsync(Guid trainingId);
    }

    public class TrainingRepository : ITrainingRepository
    {
        private readonly RedCrossDbContext _context;

        public TrainingRepository(RedCrossDbContext context)
        {
            _context = context;
        }

        public async Task<Training?> GetByIdAsync(Guid id)
        {
            return await _context.Trainings
                .Include(t => t.Enrollments)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Training>> GetAllPublishedAsync()
        {
            return await _context.Trainings
                .Where(t => t.Status == "Published")
                .OrderBy(t => t.StartDate)
                .ToListAsync();
        }

        public async Task<List<Training>> GetByFilterAsync(string? category, DateTime? startDateFrom, DateTime? startDateTo, bool? availableSpotsOnly)
        {
            var query = _context.Trainings
                .Where(t => t.Status == "Published")
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.Category == category);
            }

            if (startDateFrom.HasValue)
            {
                query = query.Where(t => t.StartDate >= startDateFrom.Value);
            }

            if (startDateTo.HasValue)
            {
                query = query.Where(t => t.StartDate <= startDateTo.Value);
            }

            if (availableSpotsOnly.HasValue && availableSpotsOnly.Value)
            {
                query = query.Where(t => t.Enrollments.Count(e => e.Status == "Enrolled") < t.MaxEnrollment);
            }

            return await query.OrderBy(t => t.StartDate).ToListAsync();
        }

        public async Task<Training> CreateAsync(Training training)
        {
            training.CreatedAt = DateTime.UtcNow;
            _context.Trainings.Add(training);
            await _context.SaveChangesAsync();
            return training;
        }

        public async Task<Training> UpdateAsync(Training training)
        {
            training.UpdatedAt = DateTime.UtcNow;
            _context.Trainings.Update(training);
            await _context.SaveChangesAsync();
            return training;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var training = await GetByIdAsync(id);
            if (training == null) return false;

            _context.Trainings.Remove(training);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetEnrollmentCountAsync(Guid trainingId)
        {
            return await _context.TrainingEnrollments
                .Where(e => e.TrainingId == trainingId && e.Status == "Enrolled")
                .CountAsync();
        }
    }

    public class TrainingEnrollmentRepository : ITrainingEnrollmentRepository
    {
        private readonly RedCrossDbContext _context;

        public TrainingEnrollmentRepository(RedCrossDbContext context)
        {
            _context = context;
        }

        public async Task<TrainingEnrollment?> GetByIdAsync(Guid id)
        {
            return await _context.TrainingEnrollments
                .Include(e => e.Training)
                .Include(e => e.Volunteer)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<TrainingEnrollment>> GetByTrainingIdAsync(Guid trainingId)
        {
            return await _context.TrainingEnrollments
                .Where(e => e.TrainingId == trainingId)
                .Include(e => e.Volunteer)
                .OrderBy(e => e.EnrolledAt)
                .ToListAsync();
        }

        public async Task<List<TrainingEnrollment>> GetByVolunteerIdAsync(Guid volunteerId)
        {
            return await _context.TrainingEnrollments
                .Where(e => e.VolunteerId == volunteerId)
                .Include(e => e.Training)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }

        public async Task<TrainingEnrollment?> GetByTrainingAndVolunteerAsync(Guid trainingId, Guid volunteerId)
        {
            return await _context.TrainingEnrollments
                .FirstOrDefaultAsync(e => e.TrainingId == trainingId && e.VolunteerId == volunteerId);
        }

        public async Task<TrainingEnrollment> CreateAsync(TrainingEnrollment enrollment)
        {
            enrollment.EnrolledAt = DateTime.UtcNow;
            _context.TrainingEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            return enrollment;
        }

        public async Task<TrainingEnrollment> UpdateAsync(TrainingEnrollment enrollment)
        {
            _context.TrainingEnrollments.Update(enrollment);
            await _context.SaveChangesAsync();
            return enrollment;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var enrollment = await GetByIdAsync(id);
            if (enrollment == null) return false;

            _context.TrainingEnrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetEnrolledCountAsync(Guid trainingId)
        {
            return await _context.TrainingEnrollments
                .Where(e => e.TrainingId == trainingId && e.Status == "Enrolled")
                .CountAsync();
        }
    }
}
