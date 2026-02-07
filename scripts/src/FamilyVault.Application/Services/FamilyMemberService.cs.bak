using AutoMapper;
using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class FamilyMemberService : GenericService<FamilyMemberDto, FamilyMember>, IFamilymemeberService
{
    private readonly IFamilyMemberRepository _familyMemberRepository;
    private readonly ILogger<FamilyMemberService> _typedLogger;

    public FamilyMemberService(IFamilyMemberRepository familyMemberRepository, IMapper mapper, ILogger<FamilyMemberService> logger)
        : base(familyMemberRepository, mapper, logger)
    {
        _familyMemberRepository = familyMemberRepository;
        _typedLogger = logger;
    }

    public Task<FamilyMemberDto> GetFamilyMemberByIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
        => GetByIdAsync(familyMemberId, cancellationToken);

    public async Task<IReadOnlyList<FamilyMemberDto>> GetFamilyMembersByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var result = await _familyMemberRepository.GetAllByFamilyIdAsync(familyId, cancellationToken);
        return _mapper.Map<IReadOnlyList<FamilyMemberDto>>(result);
    }

    public Task<FamilyMemberDto> CreateFamilyMemberAsync(CreateFamilyMememberRequest createFamilyMememberRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Creating a new family member: {FirstName} {LastName}\", createFamilyMememberRequest.FirstName, createFamilyMememberRequest.LastName);
        var familMemberToCreate = _mapper.Map<FamilyMember>(createFamilyMememberRequest);
        return CreateAsync(familMemberToCreate, userId, cancellationToken);
    }

    public Task DeleteFamilyMemberByIdAsync(Guid familyMemberId, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Deleting family member with ID: {FamilyMemberId}\", familyMemberId);
        return DeleteAsync(familyMemberId, userId, cancellationToken);
    }

    public Task<FamilyMemberDto> UpdateFamilyMemberAsync(UpdateFamilyMememberRequest updateFamilyMememberRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Updating family member with ID: {FamilyMemberId}\", updateFamilyMememberRequest.Id);
        var familMemberToUpdate = _mapper.Map<FamilyMember>(updateFamilyMememberRequest);
        return UpdateAsync(familMemberToUpdate, userId, cancellationToken);
    }
}
