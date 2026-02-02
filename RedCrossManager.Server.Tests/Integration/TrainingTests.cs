using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using RedCrossManager.Server.DTOs.Training;
using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Tests.Integration
{
    public class TrainingTests : IAsyncLifetime
    {
        private TestWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;
        private Guid _coordinatorId;
        private Guid _volunteerId;
        private string _adminToken = null!;
        private string _coordinatorToken = null!;
        private string _volunteerToken = null!;

        public async Task InitializeAsync()
        {
            _factory = new TestWebApplicationFactory();
            _client = _factory.CreateClient();
            await _factory.InitializeDatabaseAsync();

            // Seed users
            var seedResult = await _factory.SeedTestUsersAsync();
            _adminToken = seedResult["admin"];
            _coordinatorToken = seedResult["coordinator"];
            _volunteerToken = seedResult["volunteer"];
            
            _coordinatorId = seedResult.ContainsKey("coordinatorId") 
                ? Guid.Parse(seedResult["coordinatorId"].ToString()!)
                : Guid.NewGuid();
            _volunteerId = seedResult.ContainsKey("volunteerId")
                ? Guid.Parse(seedResult["volunteerId"].ToString()!)
                : Guid.NewGuid();
        }

        public async Task DisposeAsync()
        {
            await _factory.ResetDatabaseAsync();
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Fact]
        public async Task CreateTraining_WithValidData_ReturnsCreatedTraining()
        {
            // Arrange
            var createDto = new CreateTrainingDto
            {
                Title = "First Aid Certification",
                Description = "Learn basic first aid techniques",
                Category = "Safety",
                MaxEnrollment = 20,
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(9),
                LocationName = "Red Cross Center",
                CreatedByCoordinatorId = _coordinatorId
            };

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _coordinatorToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/trainings", createDto);
            var responseContent = await response.Content.ReadAsAsync<TrainingDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            responseContent.Should().NotBeNull();
            responseContent.Title.Should().Be("First Aid Certification");
            responseContent.Category.Should().Be("Safety");
            responseContent.MaxEnrollment.Should().Be(20);
            responseContent.Status.Should().Be("Published");
        }

        [Fact]
        public async Task GetTrainings_ReturnsAllPublishedTrainings()
        {
            // Arrange
            var training1 = new CreateTrainingDto
            {
                Title = "First Aid",
                Description = "First aid training",
                Category = "Safety",
                MaxEnrollment = 20,
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(9),
                LocationName = "Center A",
                CreatedByCoordinatorId = _coordinatorId
            };

            var training2 = new CreateTrainingDto
            {
                Title = "CPR Certification",
                Description = "CPR training",
                Category = "Medical",
                MaxEnrollment = 15,
                StartDate = DateTime.UtcNow.AddDays(14),
                EndDate = DateTime.UtcNow.AddDays(16),
                LocationName = "Center B",
                CreatedByCoordinatorId = _coordinatorId
            };

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _coordinatorToken);

            await _client.PostAsJsonAsync("/api/v1/trainings", training1);
            await _client.PostAsJsonAsync("/api/v1/trainings", training2);

            // Act
            var response = await _client.GetAsync("/api/v1/trainings");
            var trainings = await response.Content.ReadAsAsync<List<TrainingDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            trainings.Should().HaveCount(2);
            trainings.Should().Contain(t => t.Title == "First Aid");
            trainings.Should().Contain(t => t.Title == "CPR Certification");
        }

        [Fact]
        public async Task EnrollVolunteer_WithValidData_CreatesEnrollment()
        {
            // Arrange
            var createDto = new CreateTrainingDto
            {
                Title = "First Aid",
                Description = "First aid training",
                Category = "Safety",
                MaxEnrollment = 20,
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(9),
                LocationName = "Center A",
                CreatedByCoordinatorId = _coordinatorId
            };

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _coordinatorToken);

            var createResponse = await _client.PostAsJsonAsync("/api/v1/trainings", createDto);
            var training = await createResponse.Content.ReadAsAsync<TrainingDto>();

            var enrollDto = new EnrollTrainingDto
            {
                VolunteerId = _volunteerId,
                Status = "Enrolled"
            };

            // Act
            var enrollResponse = await _client.PostAsJsonAsync(
                $"/api/v1/trainings/{training.Id}/enroll", 
                enrollDto);

            // Assert
            enrollResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var enrollment = await enrollResponse.Content.ReadAsAsync<TrainingEnrollmentDto>();
            enrollment.VolunteerId.Should().Be(_volunteerId);
            enrollment.TrainingId.Should().Be(training.Id);
            enrollment.Status.Should().Be("Enrolled");
        }

        [Fact]
        public async Task EnrollVolunteer_WhenFullyEnrolled_CreatesWaitlistEntry()
        {
            // Arrange
            var createDto = new CreateTrainingDto
            {
                Title = "Limited Training",
                Description = "Limited enrollment",
                Category = "Safety",
                MaxEnrollment = 1,
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(9),
                LocationName = "Center A",
                CreatedByCoordinatorId = _coordinatorId
            };

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _coordinatorToken);

            var createResponse = await _client.PostAsJsonAsync("/api/v1/trainings", createDto);
            var training = await createResponse.Content.ReadAsAsync<TrainingDto>();

            // Enroll first volunteer
            var enroll1 = new EnrollTrainingDto
            {
                VolunteerId = _volunteerId,
                Status = "Enrolled"
            };

            await _client.PostAsJsonAsync($"/api/v1/trainings/{training.Id}/enroll", enroll1);

            // Enroll second volunteer (should go to waitlist)
            var secondVolunteerId = Guid.NewGuid();
            var enroll2 = new EnrollTrainingDto
            {
                VolunteerId = secondVolunteerId,
                Status = "Enrolled"
            };

            // Act
            var enrollResponse = await _client.PostAsJsonAsync(
                $"/api/v1/trainings/{training.Id}/enroll", 
                enroll2);

            // Assert
            enrollResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var enrollment = await enrollResponse.Content.ReadAsAsync<TrainingEnrollmentDto>();
            enrollment.Status.Should().Be("Waitlisted");
        }

        [Fact]
        public async Task MarkAttendance_WithValidData_GeneratesCertificate()
        {
            // Arrange
            var createDto = new CreateTrainingDto
            {
                Title = "Certification Training",
                Description = "Training with certificate",
                Category = "Safety",
                MaxEnrollment = 20,
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(9),
                LocationName = "Center A",
                CreatedByCoordinatorId = _coordinatorId
            };

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _coordinatorToken);

            var createResponse = await _client.PostAsJsonAsync("/api/v1/trainings", createDto);
            var training = await createResponse.Content.ReadAsAsync<TrainingDto>();

            // Enroll volunteer
            var enrollDto = new EnrollTrainingDto
            {
                VolunteerId = _volunteerId,
                Status = "Enrolled"
            };

            var enrollResponse = await _client.PostAsJsonAsync(
                $"/api/v1/trainings/{training.Id}/enroll", 
                enrollDto);
            var enrollment = await enrollResponse.Content.ReadAsAsync<TrainingEnrollmentDto>();

            // Mark attendance
            var attendanceDto = new MarkAttendanceDto
            {
                EnrollmentId = enrollment.Id,
                Attended = true,
                CertificateNumber = "CERT-001-2026"
            };

            // Act
            var attendanceResponse = await _client.PostAsJsonAsync(
                $"/api/v1/trainings/{training.Id}/mark-attendance",
                attendanceDto);

            // Assert
            attendanceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await attendanceResponse.Content.ReadAsAsync<TrainingEnrollmentDto>();
            result.Status.Should().Be("Completed");
            result.CertificateNumber.Should().Be("CERT-001-2026");
        }

        [Fact]
        public async Task GetTrainingDetails_WithEnrollments_ReturnsSummary()
        {
            // Arrange
            var createDto = new CreateTrainingDto
            {
                Title = "Detailed Training",
                Description = "With details",
                Category = "Safety",
                MaxEnrollment = 20,
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(9),
                LocationName = "Center A",
                CreatedByCoordinatorId = _coordinatorId
            };

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _coordinatorToken);

            var createResponse = await _client.PostAsJsonAsync("/api/v1/trainings", createDto);
            var training = await createResponse.Content.ReadAsAsync<TrainingDto>();

            // Enroll multiple volunteers
            for (int i = 0; i < 3; i++)
            {
                var enrollDto = new EnrollTrainingDto
                {
                    VolunteerId = Guid.NewGuid(),
                    Status = "Enrolled"
                };
                await _client.PostAsJsonAsync($"/api/v1/trainings/{training.Id}/enroll", enrollDto);
            }

            // Act
            var response = await _client.GetAsync($"/api/v1/trainings/{training.Id}");
            var details = await response.Content.ReadAsAsync<TrainingDetailDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            details.Should().NotBeNull();
            details.EnrollmentCount.Should().Be(3);
            details.AvailableSpots.Should().Be(17);
        }
    }
}
