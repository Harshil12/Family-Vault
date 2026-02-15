using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

public class LifeInsuranceRepository : ILifeInsuranceRepository
{
    private readonly AppDbContext _appDbContext;

    public LifeInsuranceRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<LifeInsurancePolicyDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _appDbContext.LifeInsurancePolicies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<LifeInsurancePolicyDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        return await _appDbContext.LifeInsurancePolicies
            .Where(x => x.FamilyMemberId == familyMemberId && !x.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<LifeInsurancePolicyDetails> AddAsync(LifeInsurancePolicyDetails item, CancellationToken cancellationToken)
    {
        await _appDbContext.LifeInsurancePolicies.AddAsync(item, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<LifeInsurancePolicyDetails> UpdateAsync(LifeInsurancePolicyDetails item, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.LifeInsurancePolicies.FirstOrDefaultAsync(x => x.Id == item.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Life insurance policy not found");

        _appDbContext.Entry(existing).CurrentValues.SetValues(item);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.LifeInsurancePolicies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Life insurance policy not found");

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.UpdatedBy = user;
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
}
