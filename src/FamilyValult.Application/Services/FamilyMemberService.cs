using AutoMapper;
using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Services;

public class FamilyMemberService : IFamilymemeberService
{
    private readonly IFamilyMemberRepository _familyMemberRepository;
    private readonly IMapper _mapper;

    public FamilyMemberService(IFamilyMemberRepository familyMemberRepository, IMapper mapper)
    {
        _familyMemberRepository = familyMemberRepository;
        _mapper = mapper;
    }
    public async Task<FamilyMemberDto> CreateFamilyMemberAsync(CreateFamilyMememberRequest createFamilyMememberRequest)
    {
        var result = await _familyMemberRepository.AddAsync(_mapper.Map<FamilyMember>(createFamilyMememberRequest));
        return _mapper.Map<FamilyMemberDto>(result);
    }

    public async Task DeleteFamilyMemberByIdAsync(Guid familyMemberId)
    {
        await _familyMemberRepository.DeleteByIdAsync(familyMemberId, "Harshil");
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
        var result = await _familyMemberRepository.UpdateAsync(_mapper.Map<FamilyMember>(updateFamilyMememberRequest));
        return _mapper.Map<FamilyMemberDto>(result);
    }
}
