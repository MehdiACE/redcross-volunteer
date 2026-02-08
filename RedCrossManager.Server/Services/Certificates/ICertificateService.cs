using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Training;

namespace RedCrossManager.Server.Services.Certificates;

public interface ICertificateService
{
    Task<Certification> GenerateCertificateAsync(Guid enrollmentId, CancellationToken cancellationToken = default);
    Task<Certification> GetCertificationAsync(Guid certificationId, CancellationToken cancellationToken = default);
    Task<List<Certification>> GetVolunteerCertificationsAsync(Guid volunteerId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateCertificatePdfAsync(Guid certificationId, CancellationToken cancellationToken = default);
}
