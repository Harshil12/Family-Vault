using AutoMapper;
using FamilyVault.Application.DTOs.Documents;
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
    public async Task<FamilyMemberDto> CreateFamilyMemberAsync(CreateFamilyMememberRequest createFamilyMememberRequest)
    {
        _logger.LogInformation("Creating a new family member: {FirstName} {LastName}", createFamilyMememberRequest.FirstName, createFamilyMememberRequest.LastName);

        var entity = _mapper.Map<FamilyMember>(createFamilyMememberRequest);
        entity.CreatedAt = DateTimeOffset.Now;
        entity.CreatedBy = "Harshil";

        var result = await _familyMemberRepository.AddAsync(entity);

        _logger.LogInformation("Successfully created family member with ID: {FamilyMemberId}", result.Id);
        
        return _mapper.Map<FamilyMemberDto>(result);
    }

    public async Task DeleteFamilyMemberByIdAsync(Guid familyMemberId)
    {
        _logger.LogInformation("Deleting family member with ID: {FamilyMemberId}", familyMemberId);

        await _familyMemberRepository.DeleteByIdAsync(familyMemberId, "Harshil");

        _logger.LogInformation("Successfully deleted family member with ID: {FamilyMemberId}", familyMemberId); 
    }

    public async Task<FamilyMemberDto> GetFamilyMemberByIdAsync(Guid familyMemberId)
    {
        var result = await _familyMemberRepository.GetByIdAsync(familyMemberId);
        return _mapper.Map<FamilyMemberDto>(result);
    }

    public async Task<IReadOnlyList<FamilyMemberDto>> GetFamilyMembersAsync()
    {
        var result = await _familyMemberRepository.GetAllAsync();
        return _mapper.Map<IReadOnlyList<FamilyMemberDto>>(result);
    }

    public async Task<FamilyMemberDto> UpdateFamilyMemberAsync(UpdateFamilyMememberRequest updateFamilyMememberRequest)
    {
        _logger.LogInformation("Updating family member with ID: {FamilyMemberId}", updateFamilyMememberRequest.Id);

        var entity = _mapper.Map<FamilyMember>(updateFamilyMememberRequest);
        entity.UpdatedAt = DateTimeOffset.Now;
        entity.UpdatedBy = "Harshil";

        var result = await _familyMemberRepository.UpdateAsync(entity);
        
        _logger.LogInformation("Successfully updated family member with ID: {FamilyMemberId}", result.Id);

        return _mapper.Map<FamilyMemberDto>(result);
    }
}
