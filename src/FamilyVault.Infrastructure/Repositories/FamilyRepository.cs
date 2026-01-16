using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyRepository : IFamilyRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IMemoryCache _memoryCache;

    public FamilyRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        _appDbContext = appDbContext;
        _memoryCache = memoryCache;
    }

    public async Task<IReadOnlyList<Family>> GetAllWithFamilyMembersAsync(CancellationToken cancellationToken)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue("AllWithFamilyMembers", out IReadOnlyList<Family>? families) && families is not null)
        {
            return families;
        }
        var result = await _appDbContext.Families.AsNoTracking().Include(f => f.FamilyMembers).ToListAsync(cancellationToken);

        _memoryCache.Set("AllWithFamilyMembers", result, cacheOptions);

        return result;
    }

    public async Task<IReadOnlyList<Family>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue("AllFamilyMembers", out IReadOnlyList<Family>? families) && families is not null)
        {
            return families;
        }
        var result = await _appDbContext.Families.Where(f => f.UserId == userId).AsNoTracking().ToListAsync(cancellationToken);

        _memoryCache.Set("AllFamilyMembers", result, cacheOptions);

        return result;
    }

    public async Task<Family?> GetByIdAsync(Guid familyId, CancellationToken cancellationToken)
    {
        return await _appDbContext.Families.FirstOrDefaultAsync(x => x.Id == familyId, cancellationToken);
    }

    public async Task<Family> AddAsync(Family family, CancellationToken cancellationToken)
    {
        await _appDbContext.Families.AddAsync(family, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove("AllWithFamilyMembers");
        _memoryCache.Remove("AllFamilyMembers");

        return family;
    }

    public async Task<Family> UpdateAsync(Family family, CancellationToken cancellationToken)
    {
        var existingFamily = await _appDbContext.Families
            .FirstOrDefaultAsync(fm => fm.Id == family.Id, cancellationToken) ?? throw new KeyNotFoundException("Family not found");

        existingFamily.Name = family.Name;
        existingFamily.UserId = family.UserId;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove("AllWithFamilyMembers");
        _memoryCache.Remove("AllFamilyMembers");

        return family;
    }

    public async Task DeleteByIdAsync(Guid familyId, string user, CancellationToken cancellationToken)
    {
        using var tx = await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        // Soft delete the family
        await _appDbContext.Families
                .Where(fm => fm.Id == familyId)
                .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
                .SetProperty(fm => fm.UpdatedBy, user)
                .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        // soft delete the family members
        await _appDbContext.FamilyMembers
               .Where(fm => fm.FamilyId == familyId && !fm.IsDeleted)
               .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
               .SetProperty(fm => fm.UpdatedBy, user)
               .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await _appDbContext.Documents
        .Where(d =>
            _appDbContext.FamilyMembers
                .Where(m => m.FamilyId == familyId && !m.IsDeleted)
                .Select(m => m.Id)
                .Contains(d.FamilyMemberId)
            && !d.IsDeleted
        )
        .ExecuteUpdateAsync(s =>
            s.SetProperty(d => d.IsDeleted, true)
            .SetProperty(fm => fm.UpdatedBy, user)
            .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await tx.CommitAsync(cancellationToken);

        _memoryCache.Remove("AllWithFamilyMembers");
        _memoryCache.Remove("AllFamilyMembers");
    }
}
