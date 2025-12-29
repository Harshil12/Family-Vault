using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Services;

namespace FamilyVault.Application.Services;

public class FamilyMemberService : IFamilymemeberService
{
    public Task<FamilyMemberDto> CreateFamilyMemberAsync(UpdateFamilyMememberRequest updateFamilyMememberRequest)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFamilyMemberByIdAsync(Guid familyMemberId)
    {
        throw new NotImplementedException();
    }

    public Task<FamilyMemberDto> GetFamilyMemberByIdAsync(Guid familyMemberId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<FamilyMemberDto>> GetFamilyMembersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<FamilyMemberDto> UpdateFamilyMemberAsync(CreateFamilyMememberRequest createFamilyMememberRequest)
    {
        throw new NotImplementedException();
    }
}
