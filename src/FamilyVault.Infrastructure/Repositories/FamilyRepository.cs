using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyRepository : IFamilyRepository
{
    public Task<Family> CreateFamilyAsync(Family family)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFamilyByIdAsync(Guid familyId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Family>> GetFamilyAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Family> GetFamilyByIdAsync(Guid familyId)
    {
        throw new NotImplementedException();
    }

    public Task<Family> UpdateFamilyAsync(Family family)
    {
        throw new NotImplementedException();
    }
}
