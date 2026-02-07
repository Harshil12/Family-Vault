using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyRepository : GenericRepository<Family>, IFamilyRepository
{
    public FamilyRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
        : base(appDbContext, memoryCache)
    {
    }

    public async Task<IReadOnlyList<Family>> GetAllWithFamilyMembersAsync(CancellationToken cancellationToken)
    {
        return await GetOrCreateCachedAsync(\"WithMembers\", async () =>
        {
            return await _appDbContext.Families
                .Where(f => !f.IsDeleted)
                .AsNoTracking()
                .Include(f => f.FamilyMembers.Where(m => !m.IsDeleted))
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<Family>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var suffix = $\"ByUser:{userId}\";
        return await GetOrCreateCachedAsync(suffix, async () =>
        {
            return await _appDbContext.Families
                .Where(f => f.UserId == userId && !f.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public override async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        using var tx = await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        // Soft delete the family
        await _appDbContext.Families
                .Where(fm => fm.Id == id)
                .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
                .SetProperty(fm => fm.UpdatedBy, user)
                .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        // soft delete the family members
        await _appDbContext.FamilyMembers
               .Where(fm => fm.FamilyId == id)
               .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
               .SetProperty(fm => fm.UpdatedBy, user)
               .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        // soft delete documents for members of this family (match by family member id regardless of IsDeleted)
        await _appDbContext.Documents
        .Where(d =>
            _appDbContext.FamilyMembers
                .Where(m => m.FamilyId == id)
                .Select(m => m.Id)
                .Contains(d.FamilyMemberId)
            && !d.IsDeleted
        )
        .ExecuteUpdateAsync(s =>
            s.SetProperty(d => d.IsDeleted, true)
            .SetProperty(d => d.UpdatedBy, user)
            .SetProperty(d => d.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await tx.CommitAsync(cancellationToken);

        // Invalidate family caches (base InvalidateCache handles generic keys; call it to expire family-specific caches)
        InvalidateCache();
    }
}
