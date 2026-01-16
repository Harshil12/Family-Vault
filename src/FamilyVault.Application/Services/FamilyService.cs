using AutoMapper;
using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class FamilyService : IFamilyService
{
    private readonly IFamilyRepository _familyrepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FamilyService> _logger;

    public FamilyService(IFamilyRepository familyRepository, IMapper mapper, ILogger<FamilyService> logger)
    {
        _familyrepository = familyRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FamilyDto>> GetFamilyByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _familyrepository.GetAllByUserIdAsync(userId, cancellationToken);

        return _mapper.Map<IReadOnlyList<FamilyDto>>(result);
    }

    public async Task<FamilyDto> GetFamilyByIdAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var result = await _familyrepository.GetByIdAsync(familyId, cancellationToken);

        return _mapper.Map<FamilyDto>(result);
    }

    public async Task<FamilyDto> CreateFamilyAsync(CreateFamilyRequest createFamilyRequest, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating a new family with name: {FamilyName}", createFamilyRequest.FamilyName);

        var familyToCreate = _mapper.Map<Family>(createFamilyRequest);
        familyToCreate.CreatedAt = DateTimeOffset.Now;
        familyToCreate.CreatedBy = userId.ToString();

        var result = await _familyrepository.AddAsync(familyToCreate, cancellationToken);

        _logger.LogInformation("Successfully created family with ID: {FamilyId}", result.Id);

        return _mapper.Map<FamilyDto>(result);
    }

    public async Task DeleteFamilyByIdAsync(Guid familyId, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting family with ID: {FamilyId}", familyId);

        await _familyrepository.DeleteByIdAsync(familyId, userId.ToString(), cancellationToken);

        _logger.LogInformation("Successfully deleted family with ID: {FamilyId}", familyId);
    }

    public async Task<FamilyDto> UpdateFamilyAsync(UpdateFamlyRequest updateFamlyRequest, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating family with ID: {FamilyId}", updateFamlyRequest.Id);

        var familyToUpdate = _mapper.Map<Family>(updateFamlyRequest);
        familyToUpdate.UpdatedAt = DateTimeOffset.Now;
        familyToUpdate.UpdatedBy = userId.ToString();

        var family = await _familyrepository.UpdateAsync(familyToUpdate, cancellationToken);

        _logger.LogInformation("Successfully updated family with ID: {FamilyId}", family.Id);

        return _mapper.Map<FamilyDto>(family);
    }
}
