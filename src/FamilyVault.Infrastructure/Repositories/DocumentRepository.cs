using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

/// <summary>
/// Represents DocumentRepository.
/// </summary>
public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of DocumentRepository.
    /// </summary>
    public DocumentRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        _appDbContext = appDbContext;
        _memoryCache = memoryCache; 
    }

    /// <summary>
    /// Performs the GetAllByFamilymemberIdAsync operation.
    /// </summary>
    public async Task<IReadOnlyList<DocumentDetails>> GetAllByFamilymemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
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

        var result = await _appDbContext.Documents
            .Where(d => d.FamilyMemberId == familyMemberId && !d.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        _memoryCache.Set("AllDocument", result, cacheOptions);
        return result;
    }

    /// <summary>
    /// Performs the GetAsyncbyId operation.
    /// </summary>
    public async Task<DocumentDetails?> GetAsyncbyId(Guid documentId, CancellationToken cancellationToken)
    {
        return await _appDbContext.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
    }

    /// <summary>
    /// Performs the AddAsync operation.
    /// </summary>
    public async Task<DocumentDetails> AddAsync(DocumentDetails documentDetails, CancellationToken cancellationToken)
    {
        await _appDbContext.Documents.AddAsync(documentDetails, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove("AllDocument");
  
        return documentDetails;
    }

    /// <summary>
    /// Performs the UpdateAsync operation.
    /// </summary>
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

    /// <summary>
    /// Performs the DeleteByIdAsync operation.
    /// </summary>
    public async Task DeleteByIdAsync(Guid documentId, string user, CancellationToken cancellationToken)
    {
        var existingDocument = await _appDbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken) ?? throw new KeyNotFoundException("Document not found");

        existingDocument.IsDeleted = true;
        existingDocument.UpdatedAt = DateTimeOffset.UtcNow;
        existingDocument.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove("AllDocument");
    }

}
