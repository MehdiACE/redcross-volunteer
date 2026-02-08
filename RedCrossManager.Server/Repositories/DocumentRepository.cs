using Microsoft.EntityFrameworkCore;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly RedCrossDbContext _context;

    public DocumentRepository(RedCrossDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Volunteer)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<List<Document>> GetByVolunteerIdAsync(Guid volunteerId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Where(d => d.VolunteerId == volunteerId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync(cancellationToken);
        return document;
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
