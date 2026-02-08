using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RedCrossManager.Server.DTOs.Documents;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Repositories;
using RedCrossManager.Server.Services.Documents;

namespace RedCrossManager.Server.Controllers;

[ApiController]
[Route("api/v1/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IDocumentRepository _documentRepository;
    private readonly IVolunteerRepository _volunteerRepository;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public DocumentsController(
        IDocumentService documentService,
        IDocumentRepository documentRepository,
        IVolunteerRepository volunteerRepository,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _documentService = documentService;
        _documentRepository = documentRepository;
        _volunteerRepository = volunteerRepository;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpPost("upload-url")]
    [Authorize(Policy = "Volunteer")]
    public async Task<ActionResult<UploadDocumentResponseDto>> GetUploadUrl([FromBody] UploadDocumentRequestDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            var volunteer = await _volunteerRepository.GetByUserIdAsync(userId, cancellationToken);
            if (volunteer == null)
            {
                return NotFound(new { error = "Volunteer not found" });
            }

            var result = await _documentService.CreateUploadUrlAsync(volunteer.Id, dto, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{documentId:guid}/upload")]
    [Authorize(Policy = "Volunteer")]
    public async Task<IActionResult> UploadDocument(Guid documentId, IFormFile file, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new { error = "Invalid or missing user ID in token" });
        }

        var volunteer = await _volunteerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (volunteer == null)
        {
            return NotFound(new { error = "Volunteer not found" });
        }

        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null || document.VolunteerId != volunteer.Id)
        {
            return NotFound(new { error = "Document not found" });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "File is required" });
        }

        var webRoot = string.IsNullOrEmpty(_environment.WebRootPath)
            ? Path.Combine(AppContext.BaseDirectory, "wwwroot")
            : _environment.WebRootPath;
        var uploadsRoot = Path.Combine(webRoot, "uploads", documentId.ToString());
        Directory.CreateDirectory(uploadsRoot);
        var filePath = Path.Combine(uploadsRoot, file.FileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var baseUrl = _configuration["AppBaseUrl"] ?? "http://localhost:5000";
        document.FileName = file.FileName;
        document.ContentType = file.ContentType;
        document.SizeBytes = file.Length;
        document.FileUrl = $"{baseUrl.TrimEnd('/')}/uploads/{documentId}/{file.FileName}";
        document.VirusScanStatus = VirusScanStatus.Clean;

        await _documentRepository.UpdateAsync(document, cancellationToken);

        return Ok(new { documentId = document.Id, document.FileUrl });
    }

    [HttpGet("{volunteerId:guid}")]
    public async Task<ActionResult<List<DocumentDto>>> GetDocuments(Guid volunteerId, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Coordinator") && !User.IsInRole("Admin"))
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            var volunteer = await _volunteerRepository.GetByUserIdAsync(userId, cancellationToken);
            if (volunteer == null || volunteer.Id != volunteerId)
            {
                return Forbid();
            }
        }

        var documents = await _documentService.GetVolunteerDocumentsAsync(volunteerId, cancellationToken);
        return Ok(documents);
    }

    [HttpPatch("{documentId:guid}/verify")]
    [Authorize(Policy = "Coordinator")]
    public async Task<ActionResult<DocumentDto>> VerifyDocument(Guid documentId, [FromBody] VerifyDocumentDto dto, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdValue) || !Guid.TryParse(userIdValue, out var reviewerId))
        {
            return Unauthorized(new { error = "Invalid or missing user ID in token" });
        }

        try
        {
            var updated = await _documentService.VerifyDocumentAsync(documentId, dto, reviewerId, cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
