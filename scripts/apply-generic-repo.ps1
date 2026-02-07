param(
    [string]$Branch = "generic-repository"
)

Write-Host "Running apply-generic-repo.ps1 on branch $Branch"
git fetch origin
git checkout -b $Branch

# Helpers
function Write-File($path, $content) {
    $dir = Split-Path $path -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    $content | Out-File -FilePath $path -Encoding UTF8 -Force
    Write-Host "Wrote $path"
}

# Files to create/update
Write-File "src/FamilyVault.Application/Interfaces/Repositories/IGenericRepository.cs" @"
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

/// <summary>
/// Generic repository contract that provides common CRUD operations for entities.
/// </summary>
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken);
    Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken);
}
"@

Write-File "src/FamilyVault.Application/Interfaces/Repositories/IUserRepository.cs" @"
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<IReadOnlyList<User>> GetAllWithFamilyDetailsAsync(CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}
"@

Write-File "src/FamilyVault.Application/Interfaces/Repositories/IFamilyRepository.cs" @"
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface IFamilyRepository : IGenericRepository<Family>
{
    Task<IReadOnlyList<Family>> GetAllWithFamilyMembersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Family>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    new Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken);
}
"@

# Note: preserve original filename
Write-File "src/FamilyVault.Application/Interfaces/Repositories/IFamilymemeberRepository.cs" @"
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface IFamilyMemberRepository : IGenericRepository<FamilyMember>
{
    Task<FamilyMember?> GetByIdAsync(Guid familyMemberId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FamilyMember>> GetAllWithDocumentsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<FamilyMember>> GetAllByFamilyIdAsync(Guid FamilyId, CancellationToken cancellationToken);
    new Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken);
}
"@

Write-File "src/FamilyVault.Application/Interfaces/Repositories/IDocumentRepository.cs" @"
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface IDocumentRepository : IGenericRepository<DocumentDetails>
{
    Task<DocumentDetails?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentDetails>> GetAllByFamilyMemberIdAsync(Guid FamilyMemberId, CancellationToken cancellationToken);
}
"@

Write-File "src/FamilyVault.Infrastructure/Repositories/GenericRepository.cs" @"
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _appDbContext;
    protected readonly IMemoryCache _memoryCache;
    private readonly string _cacheKeyPrefix;

    public GenericRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        _appDbContext = appDbContext;
        _memoryCache = memoryCache;
        _cacheKeyPrefix = typeof(T).Name;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _appDbContext.Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        var cacheKey = $"{_cacheKeyPrefix}_All";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<T>? cached) && cached is not null)
        {
            return cached;
        }

        var result = await _appDbContext.Set<T>().AsNoTracking().ToListAsync(cancellationToken);
        _memoryCache.Set(cacheKey, result, cacheOptions);
        return result;
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken)
    {
        await _appDbContext.Set<T>().AddAsync(entity, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove($"{_cacheKeyPrefix}_All");
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"{typeof(T).Name} not found");

        _appDbContext.Entry(existing).CurrentValues.SetValues(entity);

        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove($"{_cacheKeyPrefix}_All");
        return entity;
    }

    public virtual async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"{typeof(T).Name} not found");

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove($"{_cacheKeyPrefix}_All");
    }
}
"@

Write-File "src/FamilyVault.Infrastructure/Repositories/UserRepository.cs" @"
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
        : base(appDbContext, memoryCache)
    {
    }

    public async Task<IReadOnlyList<User>> GetAllWithFamilyDetailsAsync(CancellationToken cancellationToken)
    {
        var cacheKey = ""UsersWithFamilies"";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<User>? cachedUsers) && cachedUsers is not null)
        {
            return cachedUsers;
        }

        var result = await _appDbContext.Users.AsNoTracking().Include(f => f.Families).ToListAsync(cancellationToken);
        _memoryCache.Set(cacheKey, result, cacheOptions);
        return result;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _appDbContext.Users
           .AsNoTracking()
           .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
"@

Write-File "src/FamilyVault.Infrastructure/Repositories/FamilyRepository.cs" @"
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyRepository : GenericRepository<Family>, IFamilyRepository
{
    public FamilyRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
        : base(appDbContext, memoryCache)
    {
    }

    public async Task<IReadOnlyList<Family>> GetAllWithFamilyMembersAsync(CancellationToken cancellationToken)
    {
        var cacheKey = ""AllWithFamilyMembers"";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<Family>? families) && families is not null)
        {
            return families;
        }

        var result = await _appDbContext.Families.AsNoTracking().Include(f => f.FamilyMembers).ToListAsync(cancellationToken);
        _memoryCache.Set(cacheKey, result, cacheOptions);
        return result;
    }

    public async Task<IReadOnlyList<Family>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cacheKey = ""AllFamilyMembers"";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<Family>? families) && families is not null)
        {
            return families;
        }

        var result = await _appDbContext.Families.Where(f => f.UserId == userId).AsNoTracking().ToListAsync(cancellationToken);
        _memoryCache.Set(cacheKey, result, cacheOptions);
        return result;
    }

    public override async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        using var tx = await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        // Soft delete the family
        await _appDbContext.Families
                .Where(fm => fm.Id == id)
                .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
                .SetProperty(fm => fm.UpdatedBy, user)
                .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        // soft delete the family members
        await _appDbContext.FamilyMembers
               .Where(fm => fm.FamilyId == id && !fm.IsDeleted)
               .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
               .SetProperty(fm => fm.UpdatedBy, user)
               .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await _appDbContext.Documents
        .Where(d =>
            _appDbContext.FamilyMembers
                .Where(m => m.FamilyId == id && !m.IsDeleted)
                .Select(m => m.Id)
                .Contains(d.FamilyMemberId)
            && !d.IsDeleted
        )
        .ExecuteUpdateAsync(s =>
            s.SetProperty(d => d.IsDeleted, true)
            .SetProperty(fm => fm.UpdatedBy, user)
            .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await tx.CommitAsync(cancellationToken);

        _memoryCache.Remove(""AllWithFamilyMembers"");
        _memoryCache.Remove(""AllFamilyMembers"");
    }
}
"@

Write-File "src/FamilyVault.Infrastructure/Repositories/FamilyMemberRepository.cs" @"
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class FamilyMemberRepository : GenericRepository<FamilyMember>, IFamilyMemberRepository
{
    public FamilyMemberRepository(AppDbContext appContext, IMemoryCache memoryCache)
        : base(appContext, memoryCache)
    {
    }

    public override async Task<FamilyMember> AddAsync(FamilyMember familyMember, CancellationToken cancellationToken)
    {
        await _appDbContext.FamilyMembers.AddAsync(familyMember, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove(""AllFamiliesWithDocuments"");
        _memoryCache.Remove(""AllFamilies"");

        return familyMember;
    }

    public async Task<IReadOnlyList<FamilyMember>> GetAllWithDocumentsAsync(CancellationToken cancellationToken)
    {
        var cacheKey = ""AllFamiliesWithDocuments"";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<FamilyMember>? familyMembers) && familyMembers is not null)
        {
            return familyMembers;
        }

        var result = await _appDbContext.FamilyMembers
            .AsNoTracking()
            .Include(fm => fm.DocumentDetails)
            .ToListAsync(cancellationToken);

        _memoryCache.Set(cacheKey, result, cacheOptions);
        return result;
    }

    public async Task<IReadOnlyList<FamilyMember>> GetAllByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var cacheKey = ""AllFamilies"";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<FamilyMember>? familyMembers) && familyMembers is not null)
        {
            return familyMembers;
        }

        var result = await _appDbContext.FamilyMembers
            .Where(fm => fm.FamilyId == familyId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        _memoryCache.Set(cacheKey, result, cacheOptions);
        return result;
    }

    public override async Task<FamilyMember> UpdateAsync(FamilyMember familyMember, CancellationToken cancellationToken)
    {
        var existingFamilyMember = await _appDbContext.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.Id == familyMember.Id, cancellationToken) ?? throw new KeyNotFoundException(""Family member not found"");

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

        _memoryCache.Remove(""AllFamiliesWithDocuments"");
        _memoryCache.Remove(""AllFamilies"");

        return familyMember;
    }

    public override async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        using var tx = await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        await _appDbContext.FamilyMembers
                .Where(fm => fm.Id == id)
                .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
                .SetProperty(fm => fm.UpdatedBy, user)
                .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await _appDbContext.Documents
               .Where(fm => fm.FamilyMemberId == id)
               .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
               .SetProperty(fm => fm.UpdatedBy, user)
               .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await tx.CommitAsync(cancellationToken);

        _memoryCache.Remove(""AllFamiliesWithDocuments"");
        _memoryCache.Remove(""AllFamilies"");
    }
}
"@

Write-File "src/FamilyVault.Infrastructure/Repositories/DocumentRepository.cs" @"
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class DocumentRepository : GenericRepository<DocumentDetails>, IDocumentRepository
{
    public DocumentRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
        : base(appDbContext, memoryCache)
    {
    }

    public async Task<IReadOnlyList<DocumentDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var cacheKey = $""Documents_FamilyMember_{familyMemberId}"";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<DocumentDetails>? cachedDocuments) && cachedDocuments is not null)
        {
            return cachedDocuments;
        }

        var result = await _appDbContext.Documents
            .Where(d => d.FamilyMemberId == familyMemberId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        _memoryCache.Set(cacheKey, result, cacheOptions);
        return result;
    }
}
"@

# Stage & commit
git add .
git commit -m "refactor: introduce GenericRepository<T> and refactor concrete repositories"

# Build
Write-Host "Running dotnet build (showing output)..."
dotnet build

Write-Host "Script finished. Review changes in your IDE's Git Changes window."
