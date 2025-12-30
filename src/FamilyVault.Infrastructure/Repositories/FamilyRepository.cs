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

    public async Task<IReadOnlyList<Family>> GetFamilyAsync()
    {
        return await _appDbContext.Families.AsNoTracking().ToListAsync();
    }

    public async Task<Family?> GetFamilyByIdAsync(Guid familyId)
    {
        return await _appDbContext.Families.FirstOrDefaultAsync(x=>x.Id == familyId);
    }

    public async Task<Family> CreateFamilyAsync(Family family)
    {
        _appDbContext.Families.Add(family);
        await _appDbContext.SaveChangesAsync();
        return family;
    }    

    public async Task<Family> UpdateFamilyAsync(Family family)
    {
        _appDbContext.Families.Update(family);
        await _appDbContext.SaveChangesAsync();
        return family;
    }

    public async Task DeleteFamilyByIdAsync(Guid familyId)
    {
        _appDbContext.Families.Remove(
            await _appDbContext.Families
                .FirstOrDefaultAsync(f => f.Id == familyId) ?? 
            throw new InvalidOperationException("Family not found"));   
    }
}
