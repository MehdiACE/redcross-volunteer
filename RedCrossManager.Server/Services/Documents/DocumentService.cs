using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Documents;
using RedCrossManager.Server.Repositories;

namespace RedCrossManager.Server.Services.Documents;

public interface IDocumentService
{
    Task<UploadDocumentResponseDto> CreateUploadUrlAsync(Guid volunteerId, UploadDocumentRequestDto dto, CancellationToken cancellationToken = default);
    Task<List<DocumentDto>> GetVolunteerDocumentsAsync(Guid volunteerId, CancellationToken cancellationToken = default);
    Task<DocumentDto> VerifyDocumentAsync(Guid documentId, VerifyDocumentDto dto, Guid reviewerId, CancellationToken cancellationToken = default);
    Task<DocumentDto?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}

public class DocumentService : IDocumentService
{
    private const long MaxUploadBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png"
    };

    private readonly IDocumentRepository _documentRepository;
    private readonly IVolunteerRepository _volunteerRepository;
    private readonly string _appBaseUrl;

    public DocumentService(
        IDocumentRepository documentRepository,
        IVolunteerRepository volunteerRepository,
        IConfiguration configuration)
    {
        _documentRepository = documentRepository;
        _volunteerRepository = volunteerRepository;
        _appBaseUrl = configuration["AppBaseUrl"] ?? "http://localhost:5000";
    }

    public async Task<UploadDocumentResponseDto> CreateUploadUrlAsync(Guid volunteerId, UploadDocumentRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.SizeBytes <= 0 || dto.SizeBytes > MaxUploadBytes)
        {
            throw new InvalidOperationException("Invalid file size");
        }

        if (!AllowedContentTypes.Contains(dto.ContentType))
        {
            throw new InvalidOperationException("Unsupported content type");
        }

        if (!Enum.TryParse<DocumentCategory>(dto.Category, true, out var category))
        {
            throw new InvalidOperationException("Invalid document category");
        }

        var volunteer = await _volunteerRepository.GetByIdAsync(volunteerId, cancellationToken);
        if (volunteer == null)
        {
            throw new KeyNotFoundException("Volunteer not found");
        }

        var documentId = Guid.NewGuid();
        var fileUrl = $"{_appBaseUrl.TrimEnd('/')}/uploads/{documentId}/{dto.FileName}";
        var uploadUrl = $"{_appBaseUrl.TrimEnd('/')}/api/v1/documents/{documentId}/upload";

        var document = new Document
        {
            Id = documentId,
            VolunteerId = volunteerId,
            Category = category,
            FileName = dto.FileName,
            FileUrl = fileUrl,
            ContentType = dto.ContentType,
            SizeBytes = dto.SizeBytes,
            ExpiresAt = dto.ExpiresAt,
            VirusScanStatus = VirusScanStatus.Pending,
            VerificationStatus = VerificationStatus.Pending
        };

        await _documentRepository.AddAsync(document, cancellationToken);

        return new UploadDocumentResponseDto
        {
            DocumentId = documentId,
            UploadUrl = uploadUrl,
            FileUrl = fileUrl
        };
    }

    public async Task<List<DocumentDto>> GetVolunteerDocumentsAsync(Guid volunteerId, CancellationToken cancellationToken = default)
    {
        var docs = await _documentRepository.GetByVolunteerIdAsync(volunteerId, cancellationToken);
        return docs.Select(Map).ToList();
    }

    public async Task<DocumentDto> VerifyDocumentAsync(Guid documentId, VerifyDocumentDto dto, Guid reviewerId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null)
        {
            throw new KeyNotFoundException("Document not found");
        }

        if (!Enum.TryParse<VerificationStatus>(dto.Status, true, out var status))
        {
            throw new InvalidOperationException("Invalid verification status");
        }

        document.VerificationStatus = status;
        document.ReviewerId = reviewerId;
        document.ReviewerNotes = dto.ReviewerNotes;

        await _documentRepository.UpdateAsync(document, cancellationToken);

        return Map(document);
    }

    public async Task<DocumentDto?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        return document == null ? null : Map(document);
    }

    private static DocumentDto Map(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            VolunteerId = document.VolunteerId,
            Category = document.Category.ToString(),
            FileName = document.FileName,
            FileUrl = document.FileUrl,
            ContentType = document.ContentType,
            SizeBytes = document.SizeBytes,
            UploadedAt = document.UploadedAt,
            ExpiresAt = document.ExpiresAt,
            VerificationStatus = document.VerificationStatus.ToString(),
            VirusScanStatus = document.VirusScanStatus.ToString(),
            ReviewerNotes = document.ReviewerNotes
        };
    }
}
