param(
    [string]$Branch = "generic-repository"
)

Write-Host "Applying generic service refactor on branch $Branch"
git fetch origin
git checkout $Branch

function Write-File($path, $content) {
    $dir = Split-Path $path -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    $content | Out-File -FilePath $path -Encoding UTF8 -Force
    Write-Host "Wrote $path"
}

# Generic service
Write-File "src/FamilyVault.Application/Services/GenericService.cs" @"
using AutoMapper;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

/// <summary>
/// Generic service with common CRUD helpers used by concrete service implementations.
/// Concrete services can inherit and call the protected helpers to reduce duplication.
/// </summary>
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

# Overwrite concrete services
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
        _typedLogger.LogInformation(\"Creating a new family with name: {Name}\", createFamilyRequest.FamilyName);
        var familyToCreate = _mapper.Map<Family>(createFamilyRequest);
        return CreateAsync(familyToCreate, userId, cancellationToken);
    }

    public Task DeleteFamilyByIdAsync(Guid familyId, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Deleting family with ID: {FamilyId}\", familyId);
        return DeleteAsync(familyId, userId, cancellationToken);
    }

    public Task<FamilyDto> UpdateFamilyAsync(UpdateFamlyRequest updateFamlyRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Updating family with ID: {FamilyId}\", updateFamlyRequest.Id);
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
using FamilyVault.Application.Services;
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
        _typedLogger.LogInformation(\"Creating a new user with username: {Username}\", createUserRequest.Username);

        var userToCreate = _mapper.Map<User>(createUserRequest);
        userToCreate.Password = _cryptoService.HashPassword(createUserRequest.Password);

        return await CreateAsync(userToCreate, createdByUserId, cancellationToken);
    }

    public Task DeleteUserByIdAsync(Guid userId, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Deleting user with ID: {UserId}\", userId);
        return DeleteAsync(userId, createdByUserId, cancellationToken);
    }

    public async Task<UserDto> UpdateuUerAsync(UpdateUserRequest updateUserRequest, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Updating user with ID: {UserId}\", updateUserRequest.Id);

        var userToUpdate = _mapper.Map<User>(updateUserRequest);
        userToUpdate.Password = _cryptoService.HashPassword(userToUpdate.Password);

        return await UpdateAsync(userToUpdate, createdByUserId, cancellationToken);
    }
}
"@

Write-File "src/FamilyVault.Application/Services/DocumentService.cs" @"
using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using AutoMapper;
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
            doc.DocumentNumber = _crypto_service.DecryptData(doc.DocumentNumber);
        }

        return _mapper.Map<List<DocumentDetailsDto>>(result);
    }

    public async Task<DocumentDetailsDto> GetDocumentDetailsByIdAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documentReppository.GetByIdAsync(documentId, cancellationToken);

        if (result != null)
            result.DocumentNumber = _crypto_service.DecryptData(result.DocumentNumber);

        return _mapper.Map<DocumentDetailsDto>(result);
    }

    public async Task<DocumentDetailsDto> CreateDocumentDetailsAsync(CreateDocumentRequest createDocumentRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Creating a new document for FamilyMemberId: {FamilyMemberId}\", createDocumentRequest.FamilyMemberId);

        var documentToCreate = _mapper.Map<DocumentDetails>(createDocumentRequest);
        documentToCreate.DocumentNumber = _crypto_service.EncryptData(documentToCreate.DocumentNumber);

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
        documentToUpdate.DocumentNumber = _crypto_service.EncryptData(documentToUpdate.DocumentNumber);

        return await UpdateAsync(documentToUpdate, userId, cancellationToken);
    }
}
"@

Write-File "src/FamilyVault.Application/Services/FamilyMemberService.cs" @"
using AutoMapper;
using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class FamilyMemberService : GenericService<FamilyMemberDto, FamilyMember>, IFamilymemeberService
{
    private readonly IFamilyMemberRepository _familyMemberRepository;
    private readonly ILogger<FamilyMemberService> _typedLogger;

    public FamilyMemberService(IFamilyMemberRepository familyMemberRepository, IMapper mapper, ILogger<FamilyMemberService> logger)
        : base(familyMemberRepository, mapper, logger)
    {
        _familyMemberRepository = familyMemberRepository;
        _typedLogger = logger;
    }

    public Task<FamilyMemberDto> GetFamilyMemberByIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
        => GetByIdAsync(familyMemberId, cancellationToken);

    public async Task<IReadOnlyList<FamilyMemberDto>> GetFamilyMembersByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var result = await _familyMemberRepository.GetAllByFamilyIdAsync(familyId, cancellationToken);
        return _mapper.Map<IReadOnlyList<FamilyMemberDto>>(result);
    }

    public Task<FamilyMemberDto> CreateFamilyMemberAsync(CreateFamilyMememberRequest createFamilyMememberRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Creating a new family member: {FirstName} {LastName}\", createFamilyMememberRequest.FirstName, createFamilyMememberRequest.LastName);
        var familMemberToCreate = _mapper.Map<FamilyMember>(createFamilyMememberRequest);
        return CreateAsync(familMemberToCreate, userId, cancellationToken);
    }

    public Task DeleteFamilyMemberByIdAsync(Guid familyMemberId, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Deleting family member with ID: {FamilyMemberId}\", familyMemberId);
        return DeleteAsync(familyMemberId, userId, cancellationToken);
    }

    public Task<FamilyMemberDto> UpdateFamilyMemberAsync(UpdateFamilyMememberRequest updateFamilyMememberRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Updating family member with ID: {FamilyMemberId}\", updateFamilyMememberRequest.Id);
        var familMemberToUpdate = _mapper.Map<FamilyMember>(updateFamilyMememberRequest);
        return UpdateAsync(familMemberToUpdate, userId, cancellationToken);
    }
}
"@
