param(
    [string]$Branch = "generic-repository"
)

Write-Host "Applying generic repository & service refactor on branch $Branch"
git fetch origin
git checkout $Branch

function Write-File($path, $content) {
    $dir = Split-Path $path -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    $content | Out-File -FilePath $path -Encoding UTF8 -Force
    Write-Host "Wrote $path"
}

# IGenericRepository
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

# GenericRepository with centralized caching & eviction token
Write-File "src/FamilyVault.Infrastructure/Repositories/GenericRepository.cs" @"
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

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

    protected virtual MemoryCacheEntryOptions DefaultCacheOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        SlidingExpiration = TimeSpan.FromMinutes(2),
        Priority = CacheItemPriority.High
    };

    protected string BuildCacheKey(string suffix) => $""{_cacheKeyPrefix}:{suffix}"";

    private string CacheEvictionTokenKey => $""{_cacheKeyPrefix}:_EvictionToken"";

    protected CancellationTokenSource GetOrCreateEvictionToken()
    {
        if (!_memoryCache.TryGetValue(CacheEvictionTokenKey, out CancellationTokenSource? cts) || cts is null)
        {
            cts = new CancellationTokenSource();
            _memoryCache.Set(CacheEvictionTokenKey, cts);
        }
        return cts;
    }

    protected void InvalidateCache()
    {
        if (_memoryCache.TryGetValue(CacheEvictionTokenKey, out CancellationTokenSource? oldCts) && oldCts is not null)
        {
            try { oldCts.Cancel(); } catch { }
            try { oldCts.Dispose(); } catch { }
        }

        var newCts = new CancellationTokenSource();
        _memoryCache.Set(CacheEvictionTokenKey, newCts);
    }

    protected async Task<TItem> GetOrCreateCachedAsync<TItem>(string keySuffix, Func<Task<TItem>> factory, CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey(keySuffix);
        var evictionToken = GetOrCreateEvictionToken();

        return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetOptions(DefaultCacheOptions);
            entry.AddExpirationToken(new CancellationChangeToken(evictionToken.Token));
            return await factory();
        });
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _appDbContext.Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await GetOrCreateCachedAsync(""All"", async () =>
        {
            return await _appDbContext.Set<T>().AsNoTracking().ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken)
    {
        await _appDbContext.Set<T>().AddAsync(entity, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken)
            ?? throw new KeyNotFoundException($""{typeof(T).Name} not found"");

        _appDbContext.Entry(existing).CurrentValues.SetValues(entity);

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return entity;
    }

    public virtual async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($""{typeof(T).Name} not found"");

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
    }
}
"@

# UserRepository (use generic caching helper)
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
        return await GetOrCreateCachedAsync(""WithFamilies"", async () =>
        {
            return await _appDbContext.Users.AsNoTracking().Include(f => f.Families).ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _appDbContext.Users
           .AsNoTracking()
           .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
"@

# FamilyRepository (specialized caching, override Delete for cascading soft-delete)
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
        return await GetOrCreateCachedAsync(""WithMembers"", async () =>
        {
            return await _appDbContext.Families.AsNoTracking().Include(f => f.FamilyMembers).ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<Family>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var suffix = $""ByUser:{userId}"";
        return await GetOrCreateCachedAsync(suffix, async () =>
        {
            return await _appDbContext.Families.Where(f => f.UserId == userId).AsNoTracking().ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public override async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        using var tx = await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        await _appDbContext.Families
                .Where(fm => fm.Id == id)
                .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
                .SetProperty(fm => fm.UpdatedBy, user)
                .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

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

        // Invalidate cache for Family entity (GenericRepository.DeleteByIdAsync would do this for other entities).
        InvalidateCache();
    }
}
"@

# FamilyMemberRepository
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

        InvalidateCache();
        return familyMember;
    }

    public async Task<IReadOnlyList<FamilyMember>> GetAllWithDocumentsAsync(CancellationToken cancellationToken)
    {
        return await GetOrCreateCachedAsync(""WithDocuments"", async () =>
        {
            return await _appDbContext.FamilyMembers
                .AsNoTracking()
                .Include(fm => fm.DocumentDetails)
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyMember>> GetAllByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var suffix = $""ByFamily:{familyId}"";
        return await GetOrCreateCachedAsync(suffix, async () =>
        {
            return await _appDbContext.FamilyMembers
                .Where(fm => fm.FamilyId == familyId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public override async Task<FamilyMember> UpdateAsync(FamilyMember familyMember, CancellationToken cancellationToken)
    {
        var existingFamilyMember = await _appDbContext.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.Id == familyMember.Id, cancellationToken) ?? throw new KeyNotFoundException(""Family member not found"");

        _appDbContext.Entry(existingFamilyMember).CurrentValues.SetValues(familyMember);

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
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

        InvalidateCache();
    }
}
"@

# DocumentRepository
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
        var suffix = $""ByFamilyMember:{familyMemberId}"";
        return await GetOrCreateCachedAsync(suffix, async () =>
        {
            return await _appDbContext.Documents
                .Where(d => d.FamilyMemberId == familyMemberId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public override async Task<DocumentDetails> UpdateAsync(DocumentDetails documentDetails, CancellationToken cancellationToken)
    {
        var existingDocument = await _appDbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == documentDetails.Id, cancellationToken) ?? throw new KeyNotFoundException(""Document not found"");

        _appDbContext.Entry(existingDocument).CurrentValues.SetValues(documentDetails);

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return documentDetails;
    }

    public override async Task<DocumentDetails> AddAsync(DocumentDetails documentDetails, CancellationToken cancellationToken)
    {
        await _appDbContext.Documents.AddAsync(documentDetails, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return documentDetails;
    }

    public override async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        var existingDocument = await _appDbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken) ?? throw new KeyNotFoundException(""Document not found"");

        existingDocument.IsDeleted = true;
        existingDocument.UpdatedAt = DateTimeOffset.UtcNow;
        existingDocument.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
    }
}
"@

# GenericService and concrete services
Write-File "src/FamilyVault.Application/Services/GenericService.cs" @"
using AutoMapper;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public abstract class GenericService<TDto, TEntity>
    where TEntity : BaseEntity
{
    protected readonly IGenericRepository<TEntity> _repository;
    protected readonly IMapper _mapper;
    protected readonly ILogger _logger;

    protected GenericService(IGenericRepository<TEntity> repository, IMapper mapper, ILogger logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    protected virtual async Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TDto>>(entities);
    }

    protected virtual async Task<TDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<TDto>(entity);
    }

    protected virtual async Task<TDto> CreateAsync(TEntity entity, Guid userId, CancellationToken cancellationToken)
    {
        entity.CreatedAt = DateTimeOffset.Now;
        entity.CreatedBy = userId.ToString();

        var result = await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<TDto>(result);
    }

    protected virtual async Task<TDto> UpdateAsync(TEntity entity, Guid userId, CancellationToken cancellationToken)
    {
        entity.UpdatedAt = DateTimeOffset.Now;
        entity.UpdatedBy = userId.ToString();

        var result = await _repository.UpdateAsync(entity, cancellationToken);
        return _mapper.Map<TDto>(result);
    }

    protected virtual async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await _repository.DeleteByIdAsync(id, userId.ToString(), cancellationToken);
    }
}
"@

Write-File "src/FamilyVault.Application/Services/FamilyService.cs" @"
using AutoMapper;
using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class FamilyService : GenericService<FamilyDto, Family>, IFamilyService
{
    private readonly IFamilyRepository _familyrepository;
    private readonly ILogger<FamilyService> _typedLogger;

    public FamilyService(IFamilyRepository familyRepository, IMapper mapper, ILogger<FamilyService> logger)
        : base(familyRepository, mapper, logger)
    {
        _familyrepository = familyRepository;
        _typedLogger = logger;
    }

    public async Task<IReadOnlyList<FamilyDto>> GetFamilyByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _familyrepository.GetAllByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<IReadOnlyList<FamilyDto>>(result);
    }

    public Task<FamilyDto> GetFamilyByIdAsync(Guid familyId, CancellationToken cancellationToken)
        => GetByIdAsync(familyId, cancellationToken);

    public Task<FamilyDto> CreateFamilyAsync(CreateFamilyRequest createFamilyRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(""Creating a new family with name: {Name}"", createFamilyRequest.FamilyName);
        var familyToCreate = _mapper.Map<Family>(createFamilyRequest);
        return CreateAsync(familyToCreate, userId, cancellationToken);
    }

    public Task DeleteFamilyByIdAsync(Guid familyId, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(""Deleting family with ID: {FamilyId}"", familyId);
        return DeleteAsync(familyId, userId, cancellationToken);
    }

    public Task<FamilyDto> UpdateFamilyAsync(UpdateFamlyRequest updateFamlyRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(""Updating family with ID: {FamilyId}"", updateFamlyRequest.Id);
        var familyToUpdate = _mapper.Map<Family>(updateFamlyRequest);
        return UpdateAsync(familyToUpdate, userId, cancellationToken);
    }
}
"@

Write-File "src/FamilyVault.Application/Services/Userservice.cs" @"
using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class Userservice : GenericService<UserDto, User>, IUserService
{
    private readonly ICryptoService _cryptoService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<Userservice> _typedLogger;

    public Userservice(IUserRepository userRepository, ICryptoService cryptoService, IMapper mapper, ILogger<Userservice> logger)
        : base(userRepository, mapper, logger)
    {
        _cryptoService = cryptoService;
        _userRepository = userRepository;
        _typedLogger = logger;
    }

    public async Task<IReadOnlyList<UserDto>> GetUserAsync(CancellationToken cancellationToken)
        => await GetAllAsync(cancellationToken);

    public Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        => GetByIdAsync(userId, cancellationToken);

    public async Task<UserDto> CreateUserAsync(CreateUserRequest createUserRequest, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(""Creating a new user with username: {Username}"", createUserRequest.Username);

        var userToCreate = _mapper.Map<User>(createUserRequest);
        userToCreate.Password = _cryptoService.HashPassword(createUserRequest.Password);

        return await CreateAsync(userToCreate, createdByUserId, cancellationToken);
    }

    public Task DeleteUserByIdAsync(Guid userId, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(""Deleting user with ID: {UserId}"", userId);
        return DeleteAsync(userId, createdByUserId, cancellationToken);
    }

    public async Task<UserDto> UpdateuUerAsync(UpdateUserRequest updateUserRequest, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(""Updating user with ID: {UserId}"", updateUserRequest.Id);

        var userToUpdate = _mapper.Map<User>(updateUserRequest);
        userToUpdate.Password = _cryptoService.HashPassword(userToUpdate.Password);

        return await UpdateAsync(userToUpdate, createdByUserId, cancellationToken);
    }
}
"@

Write-File "src/FamilyVault.Application/Services/DocumentService.cs" @"
using AutoMapper;
using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class DocumentService : GenericService<DocumentDetailsDto, DocumentDetails>, IDocumentService
{
    private readonly ICryptoService _cryptoService;
    private readonly IDocumentRepository _documentReppository;
    private readonly ILogger<DocumentService> _typedLogger;

    public DocumentService(IDocumentRepository documentRepository, ICryptoService cryptoService, IMapper mapper, ILogger<DocumentService> logger)
        : base(documentRepository, mapper, logger)
    {
        _cryptoService = cryptoService;
        _documentReppository = documentRepository;
        _typedLogger = logger;
    }

    public async Task<IReadOnlyList<DocumentDetailsDto>> GetDocumentsDetailsByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var result = await _documentReppository.GetAllByFamilyMemberIdAsync(familyMemberId, cancellationToken);

        foreach (var doc in result)
        {
            doc.DocumentNumber = _cryptoService.DecryptData(doc.DocumentNumber);
        }

        return _mapper.Map<List<DocumentDetailsDto>>(result);
    }

    public async Task<DocumentDetailsDto> GetDocumentDetailsByIdAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documentReppository.GetByIdAsync(documentId, cancellationToken);

        if (result != null)
            result.DocumentNumber = _cryptoService.DecryptData(result.DocumentNumber);

        return _mapper.Map<DocumentDetailsDto>(result);
    }

    public async Task<DocumentDetailsDto> CreateDocumentDetailsAsync(CreateDocumentRequest createDocumentRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(""Creating a new document for FamilyMemberId: {FamilyMemberId}"", createDocumentRequest.FamilyMemberId);

        var documentToCreate = _mapper.Map<DocumentDetails>(createDocumentRequest);
        documentToCreate.DocumentNumber = _cryptoService.EncryptData(documentToCreate.DocumentNumber);

        return await CreateAsync(documentToCreate, userId, cancellationToken);
    }

    public Task DeleteDocumentDetailsByIdAsync(Guid documentId, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(""Deleting document with Id: {DocumentId}"", documentId);
        return DeleteAsync(documentId, userId, cancellationToken);
    }

    public async Task<DocumentDetailsDto> UpdateDocumentDetailsAsync(UpdateDocumentRequest updateDocumentRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(""Updating document with Id: {DocumentId}"", updateDocumentRequest.Id);

        var documentToUpdate = _mapper.Map<DocumentDetails>(updateDocumentRequest);
        documentToUpdate.DocumentNumber = _cryptoService.EncryptData(documentToUpdate.DocumentNumber);

        return await UpdateAsync(documentToUpdate, userId, cancellationToken);
    }
}
"@

# Stage & commit
git add .
git commit -m "refactor: introduce GenericRepository with eviction token and GenericService; update concrete repos/services"

# Build
Write-Host "Running dotnet build (showing output)..."
dotnet build

Write-Host "Script finished. Review changes in your IDE's Git Changes window."
