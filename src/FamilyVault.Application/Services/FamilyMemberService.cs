using AutoMapper;
using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class FamilyMemberService : IFamilymemeberService
{
    private readonly IFamilyMemberRepository _familyMemberRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FamilyMemberService> _logger;

    public FamilyMemberService(IFamilyMemberRepository familyMemberRepository, IMapper mapper, ILogger<FamilyMemberService> logger)
    {
        _familyMemberRepository = familyMemberRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FamilyMemberDto> GetFamilyMemberByIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var result = await _familyMemberRepository.GetByIdAsync(familyMemberId, cancellationToken);

        return _mapper.Map<FamilyMemberDto>(result);
    }

    public async Task<IReadOnlyList<FamilyMemberDto>> GetFamilyMembersByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var result = await _familyMemberRepository.GetAllByFamilyIdAsync(familyId, cancellationToken);

        return _mapper.Map<IReadOnlyList<FamilyMemberDto>>(result);
    }

    public async Task<FamilyMemberDto> CreateFamilyMemberAsync(CreateFamilyMememberRequest createFamilyMememberRequest, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating a new family member: {FirstName} {LastName}", createFamilyMememberRequest.FirstName, createFamilyMememberRequest.LastName);

        var familMemberToCreate = _mapper.Map<FamilyMember>(createFamilyMememberRequest);
        familMemberToCreate.CreatedAt = DateTimeOffset.Now;
        familMemberToCreate.CreatedBy = userId.ToString();

        var result = await _familyMemberRepository.AddAsync(familMemberToCreate, cancellationToken);

        _logger.LogInformation("Successfully created family member with ID: {FamilyMemberId}", result.Id);

        return _mapper.Map<FamilyMemberDto>(result);
    }

    public async Task DeleteFamilyMemberByIdAsync(Guid familyMemberId, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting family member with ID: {FamilyMemberId}", familyMemberId);

        await _familyMemberRepository.DeleteByIdAsync(familyMemberId, userId.ToString(), cancellationToken);

        _logger.LogInformation("Successfully deleted family member with ID: {FamilyMemberId}", familyMemberId);
    }

    public async Task<FamilyMemberDto> UpdateFamilyMemberAsync(UpdateFamilyMememberRequest updateFamilyMememberRequest, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating family member with ID: {FamilyMemberId}", updateFamilyMememberRequest.Id);

        var familMemberToUpdate = _mapper.Map<FamilyMember>(updateFamilyMememberRequest);
        familMemberToUpdate.UpdatedAt = DateTimeOffset.Now;
        familMemberToUpdate.UpdatedBy = userId.ToString();

        var result = await _familyMemberRepository.UpdateAsync(familMemberToUpdate, cancellationToken);

        _logger.LogInformation("Successfully updated family member with ID: {FamilyMemberId}", result.Id);

        return _mapper.Map<FamilyMemberDto>(result);
    }
}
