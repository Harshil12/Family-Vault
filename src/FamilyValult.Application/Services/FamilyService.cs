using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;

namespace FamilyVault.Application.Services;

public class FamilyService : IFamilyService
{
    private readonly IFamilyRepository _familyrepository;

    public FamilyService(IFamilyRepository familyRepository)
    {
        _familyrepository = familyRepository;
    }
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
