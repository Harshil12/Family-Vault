using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

/// <summary>
/// Represents FamilyRepository.
/// </summary>
public class FamilyRepository : IFamilyRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of FamilyRepository.
    /// </summary>
    public FamilyRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        _appDbContext = appDbContext;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Performs the GetAllWithFamilyMembersAsync operation.
    /// </summary>
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
        var result = await _appDbContext.Families
            .Where(f => !f.IsDeleted)
            .AsNoTracking()
            .Include(f => f.FamilyMembers!.Where(m => !m.IsDeleted))
            .ToListAsync(cancellationToken);

        _memoryCache.Set("AllWithFamilyMembers", result, cacheOptions);

        return result;
    }

    /// <summary>
    /// Performs the GetAllByUserIdAsync operation.
    /// </summary>
    public async Task<IReadOnlyList<Family>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        var cacheKey = $"AllFamilyMembers:{userId}";
        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<Family>? families) && families is not null)
        {
            return families;
        }
        var result = await _appDbContext.Families
            .Where(f => f.UserId == userId && !f.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        _memoryCache.Set(cacheKey, result, cacheOptions);

        return result;
    }

    /// <summary>
    /// Performs the GetByIdAsync operation.
    /// </summary>
    public async Task<Family?> GetByIdAsync(Guid familyId, CancellationToken cancellationToken)
    {
        return await _appDbContext.Families
            .Where(f => !f.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == familyId, cancellationToken);
    }

    /// <summary>
    /// Performs the AddAsync operation.
    /// </summary>
    public async Task<Family> AddAsync(Family family, CancellationToken cancellationToken)
    {
        await _appDbContext.Families.AddAsync(family, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove("AllWithFamilyMembers");
        _memoryCache.Remove($"AllFamilyMembers:{family.UserId}");

        return family;
    }

    /// <summary>
    /// Performs the UpdateAsync operation.
    /// </summary>
    public async Task<Family> UpdateAsync(Family family, CancellationToken cancellationToken)
    {
        var existingFamily = await _appDbContext.Families
            .FirstOrDefaultAsync(fm => fm.Id == family.Id, cancellationToken) ?? throw new KeyNotFoundException("Family not found");

        existingFamily.Name = family.Name;
        existingFamily.UserId = family.UserId;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove("AllWithFamilyMembers");
        _memoryCache.Remove($"AllFamilyMembers:{family.UserId}");

        return family;
    }

    /// <summary>
    /// Performs the DeleteByIdAsync operation.
    /// </summary>
    public async Task DeleteByIdAsync(Guid familyId, string user, CancellationToken cancellationToken)
    {
        using var tx = await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        var userId = await _appDbContext.Families
            .Where(f => f.Id == familyId)
            .Select(f => f.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        // Soft delete the family
        await _appDbContext.Families
                .Where(fm => fm.Id == familyId)
                .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
                .SetProperty(fm => fm.UpdatedBy, user)
                .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        // soft delete the family members
        await _appDbContext.FamilyMembers
               .Where(fm => fm.FamilyId == familyId)
               .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
               .SetProperty(fm => fm.UpdatedBy, user)
               .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await _appDbContext.Documents
        .Where(d =>
            _appDbContext.FamilyMembers
                .Where(m => m.FamilyId == familyId)
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
        if (userId != Guid.Empty)
        {
            _memoryCache.Remove($"AllFamilyMembers:{userId}");
        }
    }
}
