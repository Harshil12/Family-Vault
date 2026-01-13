using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

internal class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IMemoryCache _memoryCache;

    public DocumentRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        _appDbContext = appDbContext;
        _memoryCache = memoryCache; 
    }

    public async Task<IReadOnlyList<DocumentDetails>> GetAllAsync(CancellationToken cancellationToken)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue("AllDocument", out IReadOnlyList<DocumentDetails>? cachedDocuments) && cachedDocuments is not null)
        {
            return cachedDocuments;
        }

        var result = await _appDbContext.Documents.AsNoTracking().ToListAsync(cancellationToken);
        _memoryCache.Set("AllDocument", result, cacheOptions);
        return result;

    }

    public async Task<DocumentDetails?> GetAsyncbyId(Guid documentId, CancellationToken cancellationToken)
    {
        return await _appDbContext.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
    }

    public async Task<DocumentDetails> AddAsync(DocumentDetails documentDetails, CancellationToken cancellationToken)
    {
        await _appDbContext.Documents.AddAsync(documentDetails, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove("AllDocument");
  
        return documentDetails;
    }

    public async Task<DocumentDetails> UpdateAsync(DocumentDetails documentDetails, CancellationToken cancellationToken)
    {
        var existingDocument = await _appDbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == documentDetails.Id, cancellationToken) ?? throw new KeyNotFoundException("Document not found");

        existingDocument.DocumentType = documentDetails.DocumentType;
        existingDocument.DocumentNumber = documentDetails.DocumentNumber;
        existingDocument.IssueDate = documentDetails.IssueDate;
        existingDocument.ExpiryDate = documentDetails.ExpiryDate;
        existingDocument.SavedLocation = documentDetails.SavedLocation;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove("AllDocument");

        return documentDetails;
    }

    public async Task DeleteByIdAsync(Guid documentId, string user, CancellationToken cancellationToken)
    {
        var existingDocument = await _appDbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken) ?? throw new KeyNotFoundException("Document not found");

        existingDocument.IsDeleted = true;
        existingDocument.UpdatedAt = DateTimeOffset.UtcNow;
        existingDocument.UpdatedBy = user;

        _memoryCache.Remove("AllDocument");

        await _appDbContext.SaveChangesAsync(cancellationToken);
    }

}
