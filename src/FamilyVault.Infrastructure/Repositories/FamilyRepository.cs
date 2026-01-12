using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyRepository : IFamilyRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IMemoryCache _memoryCache;

    public FamilyRepository(AppDbContext appDbContext, IMemoryCache memoryCache )
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
        var result = await _appDbContext.Families.AsNoTracking().Include(f=>f.FamilyMembers).ToListAsync(cancellationToken);
        
        _memoryCache.Set("AllWithFamilyMembers", result, cacheOptions);

        return result;
    }

    public async Task<IReadOnlyList<Family>> GetAllAsync(CancellationToken cancellationToken)
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
        var result = await _appDbContext.Families.AsNoTracking().ToListAsync(cancellationToken);

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

        return family;
    }

    public async Task<Family> UpdateAsync(Family family, CancellationToken cancellationToken)
    {
        var existingFamily = await _appDbContext.Families
            .FirstOrDefaultAsync(fm => fm.Id == family.Id, cancellationToken) ?? throw new KeyNotFoundException("Family not found");

        existingFamily.Name = family.Name;
        existingFamily.UserId = family.UserId;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        return family;
    }

    public async Task DeleteByIdAsync(Guid familyId, string user, CancellationToken cancellationToken)
    {
        var existingFamily = await _appDbContext.Families
            .FirstOrDefaultAsync(fm => fm.Id == familyId, cancellationToken) ?? throw new KeyNotFoundException("Family not found");

        existingFamily.IsDeleted = true;
        existingFamily.UpdatedAt = DateTimeOffset.UtcNow;
        existingFamily.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
}
