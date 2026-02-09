using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedCrossManager.Server.DTOs.Missions;
using RedCrossManager.Server.Services.Missions;

namespace RedCrossManager.Server.Controllers;

[ApiController]
[Route("api/v1/missions")]
[Authorize]
public class MissionsController : ControllerBase
{
    private readonly IMissionService _missionService;

    public MissionsController(IMissionService missionService)
    {
        _missionService = missionService;
    }

    [HttpPost]
    [Authorize(Policy = "Coordinator")]
    public async Task<ActionResult<MissionDto>> CreateMission([FromBody] CreateMissionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var mission = await _missionService.CreateMissionAsync(dto);
        return CreatedAtAction(nameof(GetMission), new { id = mission.Id }, mission);
    }

    [HttpGet]
    public async Task<ActionResult<List<MissionDto>>> GetMissions()
    {
        var missions = await _missionService.GetMissionsAsync();
        return Ok(missions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MissionDto>> GetMission(Guid id)
    {
        try
        {
            var mission = await _missionService.GetMissionAsync(id);
            return Ok(mission);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{missionId}/apply")]
    [Authorize(Policy = "Volunteer")]
    public async Task<ActionResult<AssignmentDto>> ApplyToMission(Guid missionId, [FromBody] ApplyMissionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var assignment = await _missionService.ApplyToMissionAsync(missionId, dto);
            return CreatedAtAction(nameof(GetMission), new { id = missionId }, assignment);
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

    [HttpPost("{missionId}/assign")]
    [Authorize(Policy = "Coordinator")]
    public async Task<ActionResult<List<AssignmentDto>>> AssignVolunteers(Guid missionId, [FromBody] AssignVolunteersDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var assignments = await _missionService.AssignVolunteersAsync(missionId, dto);
            return Ok(assignments);
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
}

[ApiController]
[Route("api/v1/assignments")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IMissionService _missionService;

    public AssignmentsController(IMissionService missionService)
    {
        _missionService = missionService;
    }

    [HttpPost("{assignmentId}/confirm")]
    [Authorize(Policy = "Volunteer")]
    public async Task<ActionResult<AssignmentDto>> ConfirmAssignment(Guid assignmentId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var volunteerId))
        {
            return Unauthorized();
        }

        try
        {
            var assignment = await _missionService.ConfirmAssignmentAsync(assignmentId, volunteerId);
            return Ok(assignment);
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

    [HttpPost("{assignmentId}/status")]
    [Authorize(Policy = "Coordinator")]
    public async Task<ActionResult<AssignmentDto>> UpdateAssignmentStatus(Guid assignmentId, [FromBody] UpdateAssignmentStatusDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var assignment = await _missionService.UpdateAssignmentStatusAsync(assignmentId, dto);
            return Ok(assignment);
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
}
