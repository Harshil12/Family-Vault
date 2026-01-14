using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyMemberRepository : IFamilyMemberRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IMemoryCache _memoryCache;

    public FamilyMemberRepository(AppDbContext appContext, IMemoryCache memoryCache)
    {
        _appDbContext = appContext;
        _memoryCache = memoryCache;
    }
    public async Task<FamilyMember> AddAsync(FamilyMember familyMember, CancellationToken cancellationToken)
    {
        await _appDbContext.FamilyMembers.AddAsync(familyMember, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove("AllFamiliesWithDocuments");
        _memoryCache.Remove("AllFamilies");

        return familyMember;
    }

    public async Task<IReadOnlyList<FamilyMember>> GetAllWithDocumentsAsync(CancellationToken cancellationToken)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue("AllFamiliesWithDocuments", out IReadOnlyList<FamilyMember>? familyMembers) && familyMembers is not null)
        {
            return familyMembers;
        }
        var result = await _appDbContext.FamilyMembers
            .AsNoTracking()
            .Include(fm => fm.DocumentDetails)
            .ToListAsync(cancellationToken);

        _memoryCache.Set("AllFamiliesWithDocuments", result, cacheOptions);
        return result;
    }

    public async Task<FamilyMember?> GetByIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        return await _appDbContext.FamilyMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(fm => fm.Id == familyMemberId, cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyMember>> GetAllByFamilyIdAsync(Guid FamilyId, CancellationToken cancellationToken)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue("AllFamilies", out IReadOnlyList<FamilyMember>? familyMembers) && familyMembers is not null)
        {
            return familyMembers;
        }
        var result = await _appDbContext.FamilyMembers
            .Where(fm => fm.FamilyId == FamilyId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        _memoryCache.Set("AllFamilies", result, cacheOptions);
        return result;
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

        _memoryCache.Remove("AllFamiliesWithDocuments");
        _memoryCache.Remove("AllFamilies");

        return familyMember;
    }

    public async Task DeleteByIdAsync(Guid familyMemberId, string user, CancellationToken cancellationToken)
    {
        using var tx = await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        await _appDbContext.FamilyMembers
                .Where(fm => fm.Id == familyMemberId)
                .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
                .SetProperty(fm => fm.UpdatedBy, user)
                .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await _appDbContext.Documents
               .Where(fm => fm.FamilyMemberId == familyMemberId)
               .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
               .SetProperty(fm => fm.UpdatedBy, user)
               .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await tx.CommitAsync(cancellationToken);

        _memoryCache.Remove("AllFamiliesWithDocuments");
        _memoryCache.Remove("AllFamilies");
    }
}
