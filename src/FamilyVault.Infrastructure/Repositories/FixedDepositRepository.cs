using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

public class FixedDepositRepository : IFixedDepositRepository
{
    private readonly AppDbContext _appDbContext;

    public FixedDepositRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<FixedDepositDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _appDbContext.FixedDeposits.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FixedDepositDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        return await _appDbContext.FixedDeposits
            .Where(x => x.FamilyMemberId == familyMemberId && !x.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<FixedDepositDetails> AddAsync(FixedDepositDetails item, CancellationToken cancellationToken)
    {
        await _appDbContext.FixedDeposits.AddAsync(item, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<FixedDepositDetails> UpdateAsync(FixedDepositDetails item, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.FixedDeposits.FirstOrDefaultAsync(x => x.Id == item.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Fixed deposit not found");

        _appDbContext.Entry(existing).CurrentValues.SetValues(item);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.FixedDeposits.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Fixed deposit not found");

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.UpdatedBy = user;
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
}
