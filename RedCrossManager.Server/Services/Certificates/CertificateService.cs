using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Services.Certificates;

public class CertificateService : ICertificateService
{
    private readonly RedCrossDbContext _context;
    private readonly ILogger<CertificateService> _logger;

    public CertificateService(
        RedCrossDbContext context,
        ILogger<CertificateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Certification> GenerateCertificateAsync(Guid enrollmentId, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.TrainingEnrollments
            .Include(e => e.Training)
            .Include(e => e.Volunteer)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId, cancellationToken);

        if (enrollment == null)
        {
            throw new InvalidOperationException($"Training enrollment not found: {enrollmentId}");
        }

        if (enrollment.Status != "Completed")
        {
            throw new InvalidOperationException("Certificate can only be generated for completed trainings");
        }

        if (enrollment.CertificateId != null)
        {
            _logger.LogWarning($"Certificate already exists for enrollment {enrollmentId}");
            var existing = await _context.Certifications.FindAsync(enrollment.CertificateId);
            if (existing != null) return existing;
        }

        var certificationType = MapTrainingCategoryToCertificationType(enrollment.Training.Category);
        var expirationMonths = GetCertificationExpirationMonths(certificationType);

        var certification = new Certification
        {
            Id = Guid.NewGuid(),
            VolunteerId = enrollment.VolunteerId,
            Type = certificationType,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMonths(expirationMonths),
            Issuer = "Canadian Red Cross",
            VerificationStatus = VerificationStatus.Approved
        };

        _context.Certifications.Add(certification);

        enrollment.CertificateId = certification.Id;
        enrollment.CertificateIssuedAt = DateTime.UtcNow;
        enrollment.CertificateNumber = $"CRC-{certification.Type}-{certification.Id:N}".ToUpperInvariant();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            $"Generated certificate {certification.Id} for volunteer {enrollment.VolunteerId} from training {enrollment.TrainingId}");

        return certification;
    }

    public async Task<Certification> GetCertificationAsync(Guid certificationId, CancellationToken cancellationToken = default)
    {
        var certification = await _context.Certifications
            .Include(c => c.Volunteer)
            .Include(c => c.Document)
            .FirstOrDefaultAsync(c => c.Id == certificationId, cancellationToken);

        if (certification == null)
        {
            throw new KeyNotFoundException($"Certification not found: {certificationId}");
        }

        return certification;
    }

    public async Task<List<Certification>> GetVolunteerCertificationsAsync(Guid volunteerId, CancellationToken cancellationToken = default)
    {
        return await _context.Certifications
            .Where(c => c.VolunteerId == volunteerId)
            .OrderByDescending(c => c.IssuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<byte[]> GenerateCertificatePdfAsync(Guid certificationId, CancellationToken cancellationToken = default)
    {
        var certification = await GetCertificationAsync(certificationId, cancellationToken);

        // For now, generate a simple text-based certificate
        // In production, use a PDF library like QuestPDF or iTextSharp
        var certificateText = $@"
CANADIAN RED CROSS
CERTIFICATE OF COMPLETION

This certifies that

{certification.Volunteer.FirstName} {certification.Volunteer.LastName}

has successfully completed the training for

{certification.Type}

Issued: {certification.IssuedAt:MMMM dd, yyyy}
Expires: {certification.ExpiresAt:MMMM dd, yyyy}

Certificate ID: {certificationId}
Issuer: {certification.Issuer}
Status: {certification.VerificationStatus}

This certificate is valid until the expiration date shown above.
";

        _logger.LogInformation($"Generated PDF certificate for certification {certificationId}");

        // Return as UTF-8 encoded bytes (in production, use proper PDF generation)
        return System.Text.Encoding.UTF8.GetBytes(certificateText);
    }

    private static CertificationType MapTrainingCategoryToCertificationType(string category)
    {
        return category?.ToLowerInvariant() switch
        {
            "firstaid" or "first aid" => CertificationType.FirstAid,
            "cpr" => CertificationType.CPR,
            "disasterresponse" or "disaster response" => CertificationType.DisasterResponse,
            _ => CertificationType.Other
        };
    }

    private static int GetCertificationExpirationMonths(CertificationType type)
    {
        return type switch
        {
            CertificationType.FirstAid => 36, // 3 years
            CertificationType.CPR => 24, // 2 years
            CertificationType.DisasterResponse => 12, // 1 year
            CertificationType.Other => 12,
            _ => 12
        };
    }
}
