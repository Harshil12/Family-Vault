using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyMemberRepository(AppDbContext appContext) : IFamilyMemberRepository
{
    private readonly AppDbContext _appDbContext = appContext;


    public async Task<FamilyMember> AddAsync(FamilyMember familyMember)
    {
        _appDbContext.FamilyMembers.Add(familyMember);
        await _appDbContext.SaveChangesAsync();
        return familyMember;
    }

    public async Task<FamilyMember?> GetByIdAsync(Guid familyMemberId)
    {
        return await _appDbContext.FamilyMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(fm => fm.Id == familyMemberId);
    }

    public async Task<IReadOnlyList<FamilyMember>> GetAllAsync()
    {
        return await _appDbContext.FamilyMembers.AsNoTracking().ToListAsync();
    }

    public async Task<FamilyMember> UpdateAsync(FamilyMember familyMember)
    {
        var existingFamilyMember = await _appDbContext.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.Id == familyMember.Id) ?? throw new KeyNotFoundException("Family member not found");

        existingFamilyMember.FirstName = familyMember.FirstName;
        existingFamilyMember.LastName = familyMember.LastName;
        existingFamilyMember.CountryCode = familyMember.CountryCode;
        existingFamilyMember.Mobile = familyMember.Mobile;
        existingFamilyMember.RelationshipType = familyMember.RelationshipType;
        existingFamilyMember.DateOfBirth = familyMember.DateOfBirth;
        existingFamilyMember.BloodGroup = familyMember.BloodGroup;
        existingFamilyMember.Email = familyMember.Email;
        existingFamilyMember.PAN = familyMember.PAN;
        existingFamilyMember.Aadhar = familyMember.Aadhar;
        existingFamilyMember.FamilyId = familyMember.FamilyId;

        await _appDbContext.SaveChangesAsync();
        return familyMember;
    }

    public async Task DeleteByIdAsync(Guid familyMemberId, string user)
    {
        var existingFamilyMember = await _appDbContext.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.Id == familyMemberId) ?? throw new KeyNotFoundException("Family member not found");

        existingFamilyMember.IsDeleted = true;
        existingFamilyMember.UpdatedAt = DateTimeOffset.UtcNow;
        existingFamilyMember.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync();
    }
}
