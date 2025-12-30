using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyMemberRepository(AppDbContext appContext) : IFamilyMemberRepository
{
    private readonly AppDbContext _appDbContext = appContext;


    public async Task<FamilyMember> CreateFamilyMemberAsync(FamilyMember familyMember)
    {
        _appDbContext.FamilyMembers.Add(familyMember);
        await _appDbContext.SaveChangesAsync();
        return familyMember;
    }

    public async Task DeleteFamilyMemberByIdAsync(Guid familyMemberId)
    {
        _appDbContext.FamilyMembers.Remove(await _appDbContext.
                        FamilyMembers.FirstOrDefaultAsync(fm => fm.Id == familyMemberId)
                        ?? throw new Exception("No family member found"));
    }

    public async Task<FamilyMember?> GetFamilyMemberByIdAsync(Guid familyMemberId)
    {
        return await _appDbContext.FamilyMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(fm => fm.Id == familyMemberId);

    }

    public async Task<IReadOnlyList<FamilyMember>> GetFamilyMembersAsync()
    {
       return await _appDbContext.FamilyMembers.AsNoTracking().ToListAsync();
    }

    public async Task<FamilyMember> UpdateFamilyMemberAsync(FamilyMember familyMember)
    {
        _appDbContext.FamilyMembers.Update(familyMember);
        await _appDbContext.SaveChangesAsync();
        return familyMember;
    }
}
