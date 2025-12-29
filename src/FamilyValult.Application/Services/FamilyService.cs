using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.Interfaces.Services;

namespace FamilyVault.Application.Services;

public class FamilyService : IFamilyService
{
    public Task<FamilyDto> CreateFamilyAsync(UpdateFamlyRequest updateFamlyRequest)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFamilyByIdAsync(Guid familyId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<FamilyDto>> GetFamilyAsync()
    {
        throw new NotImplementedException();
    }

    public Task<FamilyDto> GetFamilyByIdAsync(Guid familyId)
    {
        throw new NotImplementedException();
    }

    public Task<FamilyDto> UpdateFamilyAsync(CreateFamlyRequest createFamlyRequest)
    {
        throw new NotImplementedException();
    }
}
