using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

public class MediclaimRepository : IMediclaimRepository
{
    private readonly AppDbContext _appDbContext;

    public MediclaimRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<MediclaimPolicyDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _appDbContext.MediclaimPolicies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MediclaimPolicyDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        return await _appDbContext.MediclaimPolicies
            .Where(x => x.FamilyMemberId == familyMemberId && !x.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<MediclaimPolicyDetails> AddAsync(MediclaimPolicyDetails item, CancellationToken cancellationToken)
    {
        await _appDbContext.MediclaimPolicies.AddAsync(item, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<MediclaimPolicyDetails> UpdateAsync(MediclaimPolicyDetails item, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.MediclaimPolicies.FirstOrDefaultAsync(x => x.Id == item.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Mediclaim policy not found");

        _appDbContext.Entry(existing).CurrentValues.SetValues(item);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.MediclaimPolicies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Mediclaim policy not found");

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.UpdatedBy = user;
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
}
