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

    public async Task<IReadOnlyList<Family>> GetAllAsync()
    {
        return await _appDbContext.Families.AsNoTracking().ToListAsync();
    }

    public async Task<Family?> GetByIdAsync(Guid familyId)
    {
        return await _appDbContext.Families.FirstOrDefaultAsync(x => x.Id == familyId);
    }

    public async Task<Family> AddAsync(Family family)
    {
        _appDbContext.Families.Add(family);
        await _appDbContext.SaveChangesAsync();
        return family;
    }

    public async Task<Family> UpdateAsync(Family family)
    {
        var existingFamily = await _appDbContext.Families
            .FirstOrDefaultAsync(fm => fm.Id == family.Id) ?? throw new KeyNotFoundException("Family not found");
        existingFamily.Name = family.Name;
        existingFamily.UserId = family.UserId;

        await _appDbContext.SaveChangesAsync();

        return family;
    }

    public async Task DeleteByIdAsync(Guid familyId, string user)
    {
        var existingFamily = await _appDbContext.Families
            .FirstOrDefaultAsync(fm => fm.Id == familyId) ?? throw new KeyNotFoundException("Family not found");

        existingFamily.IsDeleted = true;
        existingFamily.UpdatedAt = DateTimeOffset.UtcNow;
        existingFamily.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync();
    }
}
