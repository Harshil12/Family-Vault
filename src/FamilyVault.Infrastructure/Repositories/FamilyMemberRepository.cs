using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyMemberRepository : IFamilyMemberRepository
{
    public Task<FamilyMember> CreateFamilyMemberAsync(FamilyMember familyMember)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFamilyMemberByIdAsync(Guid familyMemberId)
    {
        throw new NotImplementedException();
    }

    public Task<FamilyMember> GetFamilyMemberByIdAsync(Guid familyMemberId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<FamilyMember>> GetFamilyMembersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<FamilyMember> UpdateFamilyMemberAsync(FamilyMember familyMember)
    {
        throw new NotImplementedException();
    }
}
