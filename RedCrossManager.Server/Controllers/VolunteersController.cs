using Microsoft.AspNetCore.Mvc;
using RedCrossManager.Server.DTOs.Volunteers;
using RedCrossManager.Server.Services.Volunteers;

namespace RedCrossManager.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class VolunteersController : ControllerBase
{
    private readonly IVolunteerService _volunteerService;
    private readonly ILogger<VolunteersController> _logger;

    public VolunteersController(
        IVolunteerService volunteerService,
        ILogger<VolunteersController> logger)
    {
        _volunteerService = volunteerService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(VolunteerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VolunteerDto>> Register(
        [FromBody] RegisterVolunteerDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _volunteerService.RegisterAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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
}
