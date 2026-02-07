using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyMemberRepository : GenericRepository<FamilyMember>, IFamilyMemberRepository
{
    public FamilyMemberRepository(AppDbContext appContext, IMemoryCache memoryCache)
        : base(appContext, memoryCache)
    {
    }

    public override async Task<FamilyMember> AddAsync(FamilyMember familyMember, CancellationToken cancellationToken)
    {
        await _appDbContext.FamilyMembers.AddAsync(familyMember, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return familyMember;
    }

    public async Task<IReadOnlyList<FamilyMember>> GetAllWithDocumentsAsync(CancellationToken cancellationToken)
    {
        return await GetOrCreateCachedAsync(""WithDocuments"", async () =>
        {
            return await _appDbContext.FamilyMembers
                .AsNoTracking()
                .Include(fm => fm.DocumentDetails)
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyMember>> GetAllByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var suffix = $""ByFamily:{familyId}"";
        return await GetOrCreateCachedAsync(suffix, async () =>
        {
            return await _appDbContext.FamilyMembers
                .Where(fm => fm.FamilyId == familyId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public override async Task<FamilyMember> UpdateAsync(FamilyMember familyMember, CancellationToken cancellationToken)
    {
        var existingFamilyMember = await _appDbContext.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.Id == familyMember.Id, cancellationToken) ?? throw new KeyNotFoundException(""Family member not found"");

        _appDbContext.Entry(existingFamilyMember).CurrentValues.SetValues(familyMember);

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return familyMember;
    }

    public override async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        using var tx = await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        await _appDbContext.FamilyMembers
                .Where(fm => fm.Id == id)
                .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
                .SetProperty(fm => fm.UpdatedBy, user)
                .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await _appDbContext.Documents
               .Where(fm => fm.FamilyMemberId == id)
               .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
               .SetProperty(fm => fm.UpdatedBy, user)
               .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await tx.CommitAsync(cancellationToken);

        InvalidateCache();
    }
}
