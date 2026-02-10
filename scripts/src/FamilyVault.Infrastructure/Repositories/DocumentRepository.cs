using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class DocumentRepository : GenericRepository<DocumentDetails>, IDocumentRepository
{
    public DocumentRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
        : base(appDbContext, memoryCache)
    {
    }

    public async Task<IReadOnlyList<DocumentDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var suffix = $"ByFamilyMember:{familyMemberId}";
        return await GetOrCreateCachedAsync(suffix, async () =>
        {
            return await _appDbContext.Documents
                .Where(d => d.FamilyMemberId == familyMemberId && !d.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public override async Task<DocumentDetails> AddAsync(DocumentDetails documentDetails, CancellationToken cancellationToken)
    {
        await _appDbContext.Documents.AddAsync(documentDetails, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return documentDetails;
    }

    public override async Task<DocumentDetails> UpdateAsync(DocumentDetails documentDetails, CancellationToken cancellationToken)
    {
        var existingDocument = await _appDbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == documentDetails.Id, cancellationToken) ?? throw new KeyNotFoundException("Document not found");

        _appDbContext.Entry(existingDocument).CurrentValues.SetValues(documentDetails);

        existingDocument.UpdatedAt = DateTimeOffset.UtcNow;
        existingDocument.UpdatedBy = documentDetails.UpdatedBy ?? existingDocument.UpdatedBy;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return existingDocument;
    }

    public override async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        var existingDocument = await _appDbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken) ?? throw new KeyNotFoundException("Document not found");

        existingDocument.IsDeleted = true;
        existingDocument.UpdatedAt = DateTimeOffset.UtcNow;
        existingDocument.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
    }
}
