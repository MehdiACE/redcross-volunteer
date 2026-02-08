using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Document>> GetByVolunteerIdAsync(Guid volunteerId, CancellationToken cancellationToken = default);
    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
}
