using AutoMapper;
using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Services;

public class FamilyService : IFamilyService
{
    private readonly IFamilyRepository _familyrepository;
    private readonly IMapper _mapper;

    public FamilyService(IFamilyRepository familyRepository, IMapper mapper)
    {
        _familyrepository = familyRepository;
        _mapper = mapper;
    }
    public async Task<FamilyDto> CreateFamilyAsync(CreateFamilyRequest createFamilyRequest)
    {
        var result = await _familyrepository.AddAsync(_mapper.Map<Family>(createFamilyRequest));
        return _mapper.Map<FamilyDto>(result);
    }

    public async Task DeleteFamilyByIdAsync(Guid familyId)
    {
        await _familyrepository.DeleteByIdAsync(familyId, "Harshil");
    }

    public async Task<IReadOnlyList<FamilyDto>> GetFamilyAsync()
    {
        var result = await _familyrepository.GetAllAsync();
        return _mapper.Map<IReadOnlyList<FamilyDto>>(result);
    }

    public async Task<FamilyDto> GetFamilyByIdAsync(Guid familyId)
    {
        var result = await _familyrepository.GetByIdAsync(familyId);
        return _mapper.Map<FamilyDto>(result);
    }

    public async Task<FamilyDto> UpdateFamilyAsync(UpdateFamlyRequest updateFamlyRequest)
    {
        var family = await _familyrepository.UpdateAsync(_mapper.Map<Family>(updateFamlyRequest));
        return _mapper.Map<FamilyDto>(family);
    }
}
