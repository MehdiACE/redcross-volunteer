using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedCrossManager.Server.DTOs.Dashboard;
using RedCrossManager.Server.Services.Dashboard;

namespace RedCrossManager.Server.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("me")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(VolunteerDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VolunteerDashboardDto>> GetMyDashboard(CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            var dashboard = await _dashboardService.GetDashboardByUserIdAsync(userId, cancellationToken);
            return dashboard == null ? NotFound() : Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving volunteer dashboard");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}
