using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedCrossManager.Server.DTOs.Auth;
using RedCrossManager.Server.DTOs.Dashboard;
using RedCrossManager.Server.DTOs.Volunteers;
using RedCrossManager.Server.Services.Dashboard;
using RedCrossManager.Server.Services.Volunteers;

namespace RedCrossManager.Server.Controllers;

[ApiController]
[Route("api/v1/volunteers")]
public class VolunteersController : ControllerBase
{
    private readonly IVolunteerService _volunteerService;
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<VolunteersController> _logger;

    public VolunteersController(
        IVolunteerService volunteerService,
        IDashboardService dashboardService,
        ILogger<VolunteersController> logger)
    {
        _volunteerService = volunteerService;
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoginResponseDto>> Register(
        [FromBody] RegisterVolunteerDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _volunteerService.RegisterAsync(dto, cancellationToken);
            return Created(string.Empty, result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VolunteerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VolunteerDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var volunteer = await _volunteerService.GetByIdAsync(id, cancellationToken);
        return volunteer == null ? NotFound() : Ok(volunteer);
    }

    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(VolunteerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VolunteerDto>> GetByEmail(
        string email,
        CancellationToken cancellationToken)
    {
        var volunteer = await _volunteerService.GetByEmailAsync(email, cancellationToken);
        return volunteer == null ? NotFound() : Ok(volunteer);
    }

    [HttpGet("me")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(VolunteerDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VolunteerDashboardDto>> GetCurrentProfile(
        CancellationToken cancellationToken)
    {
        try
        {
            // Extract user ID from JWT claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            var dashboard = await _dashboardService.GetDashboardByUserIdAsync(id, cancellationToken);
            return dashboard == null ? NotFound() : Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current volunteer profile");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "Coordinator")]
    [ProducesResponseType(typeof(VolunteerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VolunteerDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateStatusDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _volunteerService.UpdateStatusAsync(id, dto.Status, cancellationToken);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("me/sms-opt-in")]
    [Authorize(Policy = "Volunteer")]
    [ProducesResponseType(typeof(VolunteerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VolunteerDto>> UpdateSmsOptIn(
        [FromBody] SmsOptInDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            // Extract volunteer ID from JWT claims
            var volunteerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(volunteerId) || !Guid.TryParse(volunteerId, out var id))
            {
                return BadRequest(new { error = "Invalid or missing volunteer ID in token" });
            }

            var result = await _volunteerService.UpdateSmsOptInAsync(id, dto.SmsOptIn, cancellationToken);
            return result == null ? NotFound() : Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SMS opt-in for volunteer");
            return BadRequest(new { error = ex.Message });
        }
    }
}
