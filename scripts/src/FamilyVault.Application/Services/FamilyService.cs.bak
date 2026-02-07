using AutoMapper;
using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class FamilyService : GenericService<FamilyDto, Family>, IFamilyService
{
    private readonly IFamilyRepository _familyrepository;
    private readonly ILogger<FamilyService> _typedLogger;

    public FamilyService(IFamilyRepository familyRepository, IMapper mapper, ILogger<FamilyService> logger)
        : base(familyRepository, mapper, logger)
    {
        _familyrepository = familyRepository;
        _typedLogger = logger;
    }

    public async Task<IReadOnlyList<FamilyDto>> GetFamilyByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _familyrepository.GetAllByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<IReadOnlyList<FamilyDto>>(result);
    }

    public Task<FamilyDto> GetFamilyByIdAsync(Guid familyId, CancellationToken cancellationToken)
        => GetByIdAsync(familyId, cancellationToken);

    public Task<FamilyDto> CreateFamilyAsync(CreateFamilyRequest createFamilyRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(""Creating a new family with name: {Name}"", createFamilyRequest.FamilyName);
        var familyToCreate = _mapper.Map<Family>(createFamilyRequest);
        return CreateAsync(familyToCreate, userId, cancellationToken);
    }

    public Task DeleteFamilyByIdAsync(Guid familyId, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(""Deleting family with ID: {FamilyId}"", familyId);
        return DeleteAsync(familyId, userId, cancellationToken);
    }

    public Task<FamilyDto> UpdateFamilyAsync(UpdateFamlyRequest updateFamlyRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(""Updating family with ID: {FamilyId}"", updateFamlyRequest.Id);
        var familyToUpdate = _mapper.Map<Family>(updateFamlyRequest);
        return UpdateAsync(familyToUpdate, userId, cancellationToken);
    }
}
