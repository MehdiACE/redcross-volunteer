using System;

namespace RedCrossManager.Server.DTOs.Documents;

public class UploadDocumentRequestDto
{
    public string Category { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UploadDocumentResponseDto
{
    public Guid DocumentId { get; set; }
    public string UploadUrl { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
}

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid VolunteerId { get; set; }
    public string Category { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string VerificationStatus { get; set; } = null!;
    public string VirusScanStatus { get; set; } = null!;
    public string? ReviewerNotes { get; set; }
}

public class VerifyDocumentDto
{
    public string Status { get; set; } = null!;
    public string? ReviewerNotes { get; set; }
}
