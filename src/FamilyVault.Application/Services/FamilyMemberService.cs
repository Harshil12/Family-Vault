using AutoMapper;
using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

/// <summary>
/// Represents FamilyMemberService.
/// </summary>
public class FamilyMemberService : IFamilyMemberService
{
    private readonly IFamilyMemberRepository _familyMemberRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FamilyMemberService> _logger;

    /// <summary>
    /// Initializes a new instance of FamilyMemberService.
    /// </summary>
    public FamilyMemberService(IFamilyMemberRepository familyMemberRepository, IMapper mapper, ILogger<FamilyMemberService> logger)
    {
        _familyMemberRepository = familyMemberRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Performs the GetFamilyMemberByIdAsync operation.
    /// </summary>
    public async Task<FamilyMemberDto> GetFamilyMemberByIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var result = await _familyMemberRepository.GetByIdAsync(familyMemberId, cancellationToken);

        return _mapper.Map<FamilyMemberDto>(result);
    }

    /// <summary>
    /// Performs the GetFamilyMembersByFamilyIdAsync operation.
    /// </summary>
    public async Task<IReadOnlyList<FamilyMemberDto>> GetFamilyMembersByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var result = await _familyMemberRepository.GetAllByFamilyIdAsync(familyId, cancellationToken);

        return _mapper.Map<IReadOnlyList<FamilyMemberDto>>(result);
    }

    /// <summary>
    /// Performs the CreateFamilyMemberAsync operation.
    /// </summary>
    public async Task<FamilyMemberDto> CreateFamilyMemberAsync(CreateFamilyMemberRequest createFamilyMemberRequest, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating a new family member: {FirstName} {LastName}", createFamilyMemberRequest.FirstName, createFamilyMemberRequest.LastName);

        var familMemberToCreate = _mapper.Map<FamilyMember>(createFamilyMemberRequest);
        familMemberToCreate.CreatedAt = DateTimeOffset.UtcNow;
        familMemberToCreate.CreatedBy = userId.ToString();

        var result = await _familyMemberRepository.AddAsync(familMemberToCreate, cancellationToken);

        _logger.LogInformation("Successfully created family member with ID: {FamilyMemberId}", result.Id);

        return _mapper.Map<FamilyMemberDto>(result);
    }

    /// <summary>
    /// Performs the DeleteFamilyMemberByIdAsync operation.
    /// </summary>
    public async Task DeleteFamilyMemberByIdAsync(Guid familyMemberId, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting family member with ID: {FamilyMemberId}", familyMemberId);

        await _familyMemberRepository.DeleteByIdAsync(familyMemberId, userId.ToString(), cancellationToken);

        _logger.LogInformation("Successfully deleted family member with ID: {FamilyMemberId}", familyMemberId);
    }

    /// <summary>
    /// Performs the UpdateFamilyMemberAsync operation.
    /// </summary>
    public async Task<FamilyMemberDto> UpdateFamilyMemberAsync(UpdateFamilyMemberRequest updateFamilyMemberRequest, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating family member with ID: {FamilyMemberId}", updateFamilyMemberRequest.Id);

        var familMemberToUpdate = _mapper.Map<FamilyMember>(updateFamilyMemberRequest);
        familMemberToUpdate.UpdatedAt = DateTimeOffset.UtcNow;
        familMemberToUpdate.UpdatedBy = userId.ToString();

        var result = await _familyMemberRepository.UpdateAsync(familMemberToUpdate, cancellationToken);

        _logger.LogInformation("Successfully updated family member with ID: {FamilyMemberId}", result.Id);

        return _mapper.Map<FamilyMemberDto>(result);
    }
}

