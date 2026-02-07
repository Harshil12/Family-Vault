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
        var cacheKey = $""Documents_FamilyMember_{familyMemberId}"";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<DocumentDetails>? cachedDocuments) && cachedDocuments is not null)
        {
            return cachedDocuments;
        }

        var result = await _appDbContext.Documents
            .Where(d => d.FamilyMemberId == familyMemberId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        _memoryCache.Set(cacheKey, result, cacheOptions);
        return result;
    }
}
