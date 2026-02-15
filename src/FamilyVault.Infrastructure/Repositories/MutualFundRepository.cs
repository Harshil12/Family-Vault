using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

public class MutualFundRepository : IMutualFundRepository
{
    private readonly AppDbContext _appDbContext;

    public MutualFundRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<MutualFundHoldingDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _appDbContext.MutualFundHoldings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MutualFundHoldingDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        return await _appDbContext.MutualFundHoldings
            .Where(x => x.FamilyMemberId == familyMemberId && !x.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<MutualFundHoldingDetails> AddAsync(MutualFundHoldingDetails item, CancellationToken cancellationToken)
    {
        await _appDbContext.MutualFundHoldings.AddAsync(item, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<MutualFundHoldingDetails> UpdateAsync(MutualFundHoldingDetails item, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.MutualFundHoldings.FirstOrDefaultAsync(x => x.Id == item.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Mutual fund holding not found");

        _appDbContext.Entry(existing).CurrentValues.SetValues(item);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.MutualFundHoldings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Mutual fund holding not found");

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.UpdatedBy = user;
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
}
