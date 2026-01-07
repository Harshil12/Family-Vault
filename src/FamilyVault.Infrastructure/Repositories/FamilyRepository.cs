using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyRepository : IFamilyRepository
{
    private readonly AppDbContext _appDbContext;

    public FamilyRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<IReadOnlyList<Family>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _appDbContext.Families.AsNoTracking().ToListAsync(cancellationToken);
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
