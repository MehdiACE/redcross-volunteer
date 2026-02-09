using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedCrossManager.Server.DTOs.Training;
using RedCrossManager.Server.Services.Trainings;
using RedCrossManager.Server.Services.Certificates;

namespace RedCrossManager.Server.Controllers
{
    [ApiController]
    [Route("api/v1/trainings")]
    [Authorize]
    public class TrainingsController : ControllerBase
    {
        private readonly ITrainingService _trainingService;
        private readonly ICertificateService _certificateService;

        public TrainingsController(
            ITrainingService trainingService,
            ICertificateService certificateService)
        {
            _trainingService = trainingService;
            _certificateService = certificateService;
        }

        /// <summary>
        /// Create a new training (Coordinator only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Coordinator")]
        public async Task<ActionResult<TrainingDto>> CreateTraining([FromBody] CreateTrainingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var training = await _trainingService.CreateTrainingAsync(dto);
            return CreatedAtAction(nameof(GetTraining), new { id = training.Id }, training);
        }

        /// <summary>
        /// Get all published trainings
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<TrainingDto>>> GetTrainings()
        {
            var trainings = await _trainingService.GetAllPublishedTrainingsAsync();
            return Ok(trainings);
        }

        /// <summary>
        /// Get filtered trainings with pagination
        /// </summary>
        [HttpGet("filtered")]
        [AllowAnonymous]
        public async Task<ActionResult<List<TrainingDto>>> GetFilteredTrainings([FromQuery] TrainingFilterDto filter)
        {
            var trainings = await _trainingService.GetFilteredTrainingsAsync(filter);
            return Ok(trainings);
        }

        /// <summary>
        /// Get training details by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<TrainingDetailDto>> GetTraining(Guid id)
        {
            try
            {
                var training = await _trainingService.GetTrainingDetailAsync(id);
                return Ok(training);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Enroll a volunteer in a training
        /// </summary>
        [HttpPost("{trainingId}/enroll")]
        [Authorize(Policy = "Volunteer")]
        public async Task<ActionResult<TrainingEnrollmentDto>> EnrollVolunteer(Guid trainingId, [FromBody] EnrollTrainingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var enrollment = await _trainingService.EnrollVolunteerAsync(trainingId, dto);
                return CreatedAtAction(nameof(GetTrainingEnrollments), new { trainingId }, enrollment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mark attendance and issue certificate (Coordinator only)
        /// </summary>
        [HttpPost("{trainingId}/mark-attendance")]
        [Authorize(Policy = "Coordinator")]
        public async Task<ActionResult<TrainingEnrollmentDto>> MarkAttendance(Guid trainingId, [FromBody] MarkAttendanceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var enrollment = await _trainingService.MarkAttendanceAsync(trainingId, dto);
                return Ok(enrollment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Get all enrollments for a training
        /// </summary>
        [HttpGet("{trainingId}/enrollments")]
        [Authorize(Policy = "Coordinator")]
        public async Task<ActionResult<List<TrainingEnrollmentDto>>> GetTrainingEnrollments(Guid trainingId)
        {
            try
            {
                var enrollments = await _trainingService.GetTrainingEnrollmentsAsync(trainingId);
                return Ok(enrollments);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Get trainings for current volunteer
        /// </summary>
        [HttpGet("volunteer/my-trainings")]
        [Authorize(Policy = "Volunteer")]
        public async Task<ActionResult<List<TrainingEnrollmentDto>>> GetMyTrainings()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var volunteerId))
                return Unauthorized();

            var trainings = await _trainingService.GetVolunteerTrainingsAsync(volunteerId);
            return Ok(trainings);
        }

        /// <summary>
        /// Generate certificate for completed training enrollment
        /// </summary>
        [HttpPost("enrollments/{enrollmentId}/generate-certificate")]
        [Authorize(Policy = "Coordinator")]
        public async Task<IActionResult> GenerateCertificate(Guid enrollmentId)
        {
            try
            {
                var certificate = await _certificateService.GenerateCertificateAsync(enrollmentId);
                return Ok(new { certificateId = certificate.Id, message = "Certificate generated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Download certificate as PDF
        /// </summary>
        [HttpGet("certificates/{certificationId}/pdf")]
        public async Task<IActionResult> DownloadCertificate(Guid certificationId)
        {
            try
            {
                var pdfBytes = await _certificateService.GenerateCertificatePdfAsync(certificationId);
                return File(pdfBytes, "application/pdf", $"certificate-{certificationId}.pdf");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
