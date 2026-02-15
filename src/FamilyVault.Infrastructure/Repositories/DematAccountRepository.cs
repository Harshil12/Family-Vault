using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

public class DematAccountRepository : IDematAccountRepository
{
    private readonly AppDbContext _appDbContext;

    public DematAccountRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<DematAccountDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _appDbContext.DematAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<DematAccountDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        return await _appDbContext.DematAccounts
            .Where(x => x.FamilyMemberId == familyMemberId && !x.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<DematAccountDetails> AddAsync(DematAccountDetails item, CancellationToken cancellationToken)
    {
        await _appDbContext.DematAccounts.AddAsync(item, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<DematAccountDetails> UpdateAsync(DematAccountDetails item, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.DematAccounts.FirstOrDefaultAsync(x => x.Id == item.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Demat account not found");

        _appDbContext.Entry(existing).CurrentValues.SetValues(item);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.DematAccounts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Demat account not found");

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.UpdatedBy = user;
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
}
