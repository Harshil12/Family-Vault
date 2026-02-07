# Run from repo root. Assumes branch generic-repository is checked out.
param()

Write-Host "Applying critical fixes - files will be overwritten. Review changes in Git Changes."

function Write-File($path, $content) {
    $dir = Split-Path $path -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    $content | Out-File -FilePath $path -Encoding UTF8 -Force
    Write-Host "WROTE: $path"
}

# GenericRepository.cs (centralized caching + eviction token + soft-delete filtering + InvalidateCache hook)
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
        _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _cacheKeyPrefix = typeof(T).Name;
    }

    protected virtual MemoryCacheEntryOptions DefaultCacheOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        SlidingExpiration = TimeSpan.FromMinutes(2),
        Priority = CacheItemPriority.High
    };

    protected string BuildCacheKey(string suffix) => $\"{_cacheKeyPrefix}:{suffix}\";
    private string CacheEvictionTokenKey => $\"{_cacheKeyPrefix}:_EvictionToken\";

    // Create or retrieve a CancellationTokenSource used to expire related cache entries.
    protected CancellationTokenSource GetOrCreateEvictionToken()
    {
        if (!_memoryCache.TryGetValue(CacheEvictionTokenKey, out CancellationTokenSource? cts) || cts is null)
        {
            cts = new CancellationTokenSource();
            _memoryCache.Set(CacheEvictionTokenKey, cts);
        }
        return cts;
    }

    // Default invalidation: cancel token so all entries that referenced it expire.
    // Derived classes can override to clear additional explicit keys.
    protected virtual void InvalidateCache()
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
        // Exclude soft-deleted by default
        return await _appDbContext.Set<T>()
            .Where(e => !e.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await GetOrCreateCachedAsync(\"All\", async () =>
        {
            return await _appDbContext.Set<T>().Where(e => !e.IsDeleted).AsNoTracking().ToListAsync(cancellationToken);
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
            ?? throw new KeyNotFoundException($\"{typeof(T).Name} not found\");

        _appDbContext.Entry(existing).CurrentValues.SetValues(entity);

        // ensure audit fields are updated if incoming entity set them; keep UtcNow fallback
        existing.UpdatedAt ??= DateTimeOffset.UtcNow;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return existing;
    }

    public virtual async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($\"{typeof(T).Name} not found\");

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
    }
}
"@

# UserRepository: ensure proper soft-delete filtering and correct cache usage
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
        // cache suffix: WithFamilies
        return await GetOrCreateCachedAsync(\"WithFamilies\", async () =>
        {
            return await _appDbContext.Users
                .Where(u => !u.IsDeleted)
                .AsNoTracking()
                .Include(u => u.Families.Where(f => !f.IsDeleted))
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _appDbContext.Users
           .AsNoTracking()
           .Where(u => !u.IsDeleted)
           .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
"@

# FamilyRepository: per-user cache key; cascading delete fix (remove !m.IsDeleted in doc subquery)
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
        return await GetOrCreateCachedAsync(\"WithMembers\", async () =>
        {
            return await _appDbContext.Families
                .Where(f => !f.IsDeleted)
                .AsNoTracking()
                .Include(f => f.FamilyMembers.Where(m => !m.IsDeleted))
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<Family>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var suffix = $\"ByUser:{userId}\";
        return await GetOrCreateCachedAsync(suffix, async () =>
        {
            return await _appDbContext.Families
                .Where(f => f.UserId == userId && !f.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }, cancellationToken);
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
               .Where(fm => fm.FamilyId == id)
               .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
               .SetProperty(fm => fm.UpdatedBy, user)
               .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        // soft delete documents for members of this family (match by family member id regardless of IsDeleted)
        await _appDbContext.Documents
        .Where(d =>
            _appDbContext.FamilyMembers
                .Where(m => m.FamilyId == id)
                .Select(m => m.Id)
                .Contains(d.FamilyMemberId)
            && !d.IsDeleted
        )
        .ExecuteUpdateAsync(s =>
            s.SetProperty(d => d.IsDeleted, true)
            .SetProperty(d => d.UpdatedBy, user)
            .SetProperty(d => d.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await tx.CommitAsync(cancellationToken);

        // Invalidate family caches (base InvalidateCache handles generic keys; call it to expire family-specific caches)
        InvalidateCache();
    }
}
"@

# FamilyMemberRepository: per-family cache keys, filter IsDeleted, set audit fields on update
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
        return await GetOrCreateCachedAsync(\"WithDocuments\", async () =>
        {
            return await _appDbContext.FamilyMembers
                .Where(fm => !fm.IsDeleted)
                .AsNoTracking()
                .Include(fm => fm.DocumentDetails.Where(d => !d.IsDeleted))
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyMember>> GetAllByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var suffix = $\"ByFamily:{familyId}\";
        return await GetOrCreateCachedAsync(suffix, async () =>
        {
            return await _appDbContext.FamilyMembers
                .Where(fm => fm.FamilyId == familyId && !fm.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public override async Task<FamilyMember> UpdateAsync(FamilyMember familyMember, CancellationToken cancellationToken)
    {
        var existingFamilyMember = await _appDbContext.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.Id == familyMember.Id, cancellationToken) ?? throw new KeyNotFoundException(\"Family member not found\");

        _appDbContext.Entry(existingFamilyMember).CurrentValues.SetValues(familyMember);

        // update audit fields if not already set
        existingFamilyMember.UpdatedAt = DateTimeOffset.UtcNow;
        existingFamilyMember.UpdatedBy = familyMember.UpdatedBy ?? existingFamilyMember.UpdatedBy;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return existingFamilyMember;
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
               .Where(d => d.FamilyMemberId == id)
               .ExecuteUpdateAsync(setter => setter.SetProperty(d => d.IsDeleted, true)
               .SetProperty(d => d.UpdatedBy, user)
               .SetProperty(d => d.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await tx.CommitAsync(cancellationToken);

        InvalidateCache();
    }
}
"@

# DocumentRepository: per-family-member cache key and exclude soft-deleted documents
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
        var suffix = $\"ByFamilyMember:{familyMemberId}\";
        return await GetOrCreateCachedAsync(suffix, async () =>
        {
            return await _appDbContext.Documents
                .Where(d => d.FamilyMemberId == familyMemberId && !d.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public override async Task<DocumentDetails> AddAsync(DocumentDetails documentDetails, CancellationToken cancellationToken)
    {
        await _appDbContext.Documents.AddAsync(documentDetails, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return documentDetails;
    }

    public override async Task<DocumentDetails> UpdateAsync(DocumentDetails documentDetails, CancellationToken cancellationToken)
    {
        var existingDocument = await _appDbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == documentDetails.Id, cancellationToken) ?? throw new KeyNotFoundException(\"Document not found\");

        _appDbContext.Entry(existingDocument).CurrentValues.SetValues(documentDetails);

        existingDocument.UpdatedAt = DateTimeOffset.UtcNow;
        existingDocument.UpdatedBy = documentDetails.UpdatedBy ?? existingDocument.UpdatedBy;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return existingDocument;
    }

    public override async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        var existingDocument = await _appDbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken) ?? throw new KeyNotFoundException(\"Document not found\");

        existingDocument.IsDeleted = true;
        existingDocument.UpdatedAt = DateTimeOffset.UtcNow;
        existingDocument.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
    }
}
"@

# DocumentService: map to DTO first then decrypt (avoid mutating cached entities)
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
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<DocumentService> _typedLogger;

    public DocumentService(IDocumentRepository documentRepository, ICryptoService cryptoService, IMapper mapper, ILogger<DocumentService> logger)
        : base(documentRepository, mapper, logger)
    {
        _cryptoService = cryptoService;
        _documentRepository = documentRepository;
        _typedLogger = logger;
    }

    public async Task<IReadOnlyList<DocumentDetailsDto>> GetDocumentsDetailsByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var result = await _documentRepository.GetAllByFamilyMemberIdAsync(familyMemberId, cancellationToken);

        // map to DTOs (do not mutate entities)
        var dtos = _mapper.Map<List<DocumentDetailsDto>>(result);
        foreach (var dto in dtos)
        {
            dto.DocumentNumber = _cryptoService.DecryptData(dto.DocumentNumber);
        }

        return dtos;
    }

    public async Task<DocumentDetailsDto> GetDocumentDetailsByIdAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (result != null)
        {
            var dto = _mapper.Map<DocumentDetailsDto>(result);
            dto.DocumentNumber = _cryptoService.DecryptData(dto.DocumentNumber);
            return dto;
        }

        return _mapper.Map<DocumentDetailsDto>(result);
    }

    public async Task<DocumentDetailsDto> CreateDocumentDetailsAsync(CreateDocumentRequest createDocumentRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Creating a new document for FamilyMemberId: {FamilyMemberId}\", createDocumentRequest.FamilyMemberId);

        var documentToCreate = _mapper.Map<DocumentDetails>(createDocumentRequest);
        documentToCreate.DocumentNumber = _cryptoService.EncryptData(documentToCreate.DocumentNumber);

        return await CreateAsync(documentToCreate, userId, cancellationToken);
    }

    public Task DeleteDocumentDetailsByIdAsync(Guid documentId, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Deleting document with Id: {DocumentId}\", documentId);
        return DeleteAsync(documentId, userId, cancellationToken);
    }

    public async Task<DocumentDetailsDto> UpdateDocumentDetailsAsync(UpdateDocumentRequest updateDocumentRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Updating document with Id: {DocumentId}\", updateDocumentRequest.Id);

        var documentToUpdate = _mapper.Map<DocumentDetails>(updateDocumentRequest);
        documentToUpdate.DocumentNumber = _cryptoService.EncryptData(documentToUpdate.DocumentNumber);

        return await UpdateAsync(documentToUpdate, userId, cancellationToken);
    }
}
"@

# User service: rename UpdateuUerAsync -> UpdateUserAsync, avoid logging PII and only hash provided password
Write-File "src/FamilyVault.Application/Services/Userservice.cs" @"
using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class UserService : GenericService<UserDto, User>, IUserService
{
    private readonly ICryptoService _cryptoService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _typedLogger;

    public UserService(IUserRepository userRepository, ICryptoService cryptoService, IMapper mapper, ILogger<UserService> logger)
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
        _typedLogger.LogInformation(\"Creating a new user\");

        var userToCreate = _mapper.Map<User>(createUserRequest);
        userToCreate.Password = _cryptoService.HashPassword(createUserRequest.Password);

        return await CreateAsync(userToCreate, createdByUserId, cancellationToken);
    }

    public Task DeleteUserByIdAsync(Guid userId, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Deleting user with ID: {UserId}\", userId);
        return DeleteAsync(userId, createdByUserId, cancellationToken);
    }

    public async Task<UserDto> UpdateUserAsync(UpdateUserRequest updateUserRequest, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Updating user with ID: {UserId}\", updateUserRequest.Id);

        var userToUpdate = _mapper.Map<User>(updateUserRequest);

        // only hash and update password if provided in request
        if (!string.IsNullOrEmpty(updateUserRequest.Password))
        {
            userToUpdate.Password = _cryptoService.HashPassword(updateUserRequest.Password);
        }

        return await UpdateAsync(userToUpdate, createdByUserId, cancellationToken);
    }
}
"@

# IUserService interface: rename UpdateuUerAsync -> UpdateUserAsync
Write-File "src/FamilyVault.Application/Interfaces/Services/IUserService.cs" @"
using FamilyVault.Application.DTOs.User;

namespace FamilyVault.Application.Interfaces.Services;

public interface IUserService
{
    public Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    public Task<IReadOnlyList<UserDto>> GetUserAsync(CancellationToken cancellationToken);
    public Task<UserDto> UpdateUserAsync(UpdateUserRequest updateUserRequest, Guid createdByUserId, CancellationToken cancellationToken);
    public Task<UserDto> CreateUserAsync(CreateUserRequest createUserRequest, Guid createdByUserId, CancellationToken cancellationToken);
    public Task DeleteUserByIdAsync(Guid userId, Guid createdByUserId, CancellationToken cancellationToken);
}
"@

# Fix CryptoService tests: setup IDataProtectionProvider.CreateProtector mock
Write-File "src/FamilyVault.Test/FamilyValut.Tests/Application/Services/Others/CryptoServiceTest.cs" @"
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace FamilyValut.Tests.Application.Services.Others;

public class CryptoServiceTests
{
    private readonly CryptoService _sut;
    private readonly Mock<IDataProtectionProvider> _dataProtectorMock;
    private readonly Mock<IDataProtector> _protectorMock;

    public CryptoServiceTests()
    {
        _dataProtectorMock = new Mock<IDataProtectionProvider>();
        _protectorMock = new Mock<IDataProtector>();

        // Setup provider to return a protector that does round-trip Protect/Unprotect.
        _dataProtectorMock
            .Setup(p => p.CreateProtector(It.IsAny<string>()))
            .Returns(_protectorMock.Object);

        _protectorMock.Setup(p => p.Protect(It.IsAny<byte[]>()))
            .Returns((byte[] input) => input); // identity for bytes
        _protectorMock.Setup(p => p.Unprotect(It.IsAny<byte[]>()))
            .Returns((byte[] input) => input);

        _sut = new CryptoService(_dataProtectorMock.Object);
    }

    #region Encrypt / Decrypt

    [Fact]
    public void EncryptData_ShouldThrow_WhenKeyIsNotValidBase64()
    {
        // Arrange
        var plainText = "hello world";

        // Act
        Action act = () => _sut.EncryptData(plainText);

        // Assert
        act.Should().Throw<FormatException>()
           .WithMessage("*base-64*");
    }

    [Fact]
    public void DecryptData_ShouldThrow_WhenKeyIsNotValidBase64()
    {
        // Arrange
        var encryptedText = Convert.ToBase64String(Encoding.UTF8.GetBytes("dummy"));

        // Act
        Action act = () => _sut.DecryptData(encryptedText);

        // Assert
        act.Should().Throw<FormatException>()
           .WithMessage("*base-64*");
    }

    [Fact]
    public void DecryptData_ShouldThrow_WhenEncryptedDataIsNotBase64()
    {
        // Arrange
        var invalidEncryptedData = "not-base64-data";

        // Act
        Action act = () => _sut.DecryptData(invalidEncryptedData);

        // Assert
        act.Should().Throw<FormatException>();
    }

    #endregion

    #region HashPassword

    [Fact]
    public void HashPassword_ShouldReturn_NonEmptyHash()
    {
        var password = "MyStrongPassword@123";
        var hash = _sut.HashPassword(password);
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashes_ForSamePassword()
    {
        var password = "SamePassword";
        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void HashPassword_ShouldThrow_WhenPasswordIsNull()
    {
        Action act = () => _sut.HashPassword(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region VerifyPassword

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        var password = "CorrectPassword!";
        var hash = _sut.HashPassword(password);
        var result = _sut.VerifyPassword(hash, password);
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        var password = "CorrectPassword!";
        var wrongPassword = "WrongPassword!";
        var hash = _sut.HashPassword(password);
        var result = _sut.VerifyPassword(hash, wrongPassword);
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashIsInvalid()
    {
        var invalidHash = "invalid-hash";
        var password = "AnyPassword";
        var result = _sut.VerifyPassword(invalidHash, password);
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldThrow_WhenPasswordIsNull()
    {
        var hash = _sut.HashPassword("test");
        Action act = () => _sut.VerifyPassword(hash, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
"@
