using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyMemberRepository(AppDbContext appContext) : IFamilyMemberRepository
{
    private readonly AppDbContext _appDbContext = appContext;


    public async Task<FamilyMember> AddAsync(FamilyMember familyMember, CancellationToken cancellationToken)
    {
        await _appDbContext.FamilyMembers.AddAsync(familyMember, cancellationToken);
        await _appDbContext.SaveChangesAsync();

        return familyMember;
    }

    public async Task<IReadOnlyList<FamilyMember>> GetAllWithDocumentsAsync(CancellationToken cancellationToken)
    {
        return await _appDbContext.FamilyMembers
            .AsNoTracking()
            .Include(fm => fm.DocumentDetails)
            .ToListAsync(cancellationToken);
    }

    public async Task<FamilyMember?> GetByIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        return await _appDbContext.FamilyMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(fm => fm.Id == familyMemberId, cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyMember>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _appDbContext.FamilyMembers.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<FamilyMember> UpdateAsync(FamilyMember familyMember, CancellationToken cancellationToken)
    {
        var existingFamilyMember = await _appDbContext.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.Id == familyMember.Id, cancellationToken) ?? throw new KeyNotFoundException("Family member not found");

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

        await _appDbContext.SaveChangesAsync(cancellationToken);

        return familyMember;
    }

    public async Task DeleteByIdAsync(Guid familyMemberId, string user, CancellationToken cancellationToken)
    {
        var existingFamilyMember = await _appDbContext.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.Id == familyMemberId, cancellationToken) ?? throw new KeyNotFoundException("Family member not found");

        existingFamilyMember.IsDeleted = true;
        existingFamilyMember.UpdatedAt = DateTimeOffset.UtcNow;
        existingFamilyMember.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
}
