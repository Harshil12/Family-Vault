using AutoMapper;
using FamilyVault.Application.DTOs.DematAccounts;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Services;

public class DematAccountService : IDematAccountService
{
    private readonly IDematAccountRepository _repository;
    private readonly ICryptoService _cryptoService;
    private readonly IMapper _mapper;

    public DematAccountService(IDematAccountRepository repository, ICryptoService cryptoService, IMapper mapper)
    {
        _repository = repository;
        _cryptoService = cryptoService;
        _mapper = mapper;
    }

    public async Task<DematAccountDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _repository.GetByIdAsync(id, cancellationToken);
        if (result != null)
        {
            result.ClientId = _cryptoService.DecryptData(result.ClientId);
            if (!string.IsNullOrWhiteSpace(result.BOId))
            {
                result.BOId = _cryptoService.DecryptData(result.BOId);
            }
        }
        return _mapper.Map<DematAccountDetailsDto>(result);
    }

    public async Task<IReadOnlyList<DematAccountDetailsDto>> GetByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var result = await _repository.GetAllByFamilyMemberIdAsync(familyMemberId, cancellationToken);
        foreach (var item in result)
        {
            item.ClientId = _cryptoService.DecryptData(item.ClientId);
            if (!string.IsNullOrWhiteSpace(item.BOId))
            {
                item.BOId = _cryptoService.DecryptData(item.BOId);
            }
        }
        return _mapper.Map<List<DematAccountDetailsDto>>(result);
    }

    public async Task<DematAccountDetailsDto> CreateAsync(CreateDematAccountRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<DematAccountDetails>(request);
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.CreatedBy = userId.ToString();
        entity.ClientIdLast4 = GetLastFourDigits(entity.ClientId);
        entity.ClientId = _cryptoService.EncryptData(entity.ClientId);
        entity.BOIdLast4 = GetLastFourDigits(entity.BOId);
        if (!string.IsNullOrWhiteSpace(entity.BOId))
        {
            entity.BOId = _cryptoService.EncryptData(entity.BOId);
        }

        var result = await _repository.AddAsync(entity, cancellationToken);
        result.ClientId = request.ClientId;
        result.BOId = request.BOId;
        return _mapper.Map<DematAccountDetailsDto>(result);
    }

    public async Task<DematAccountDetailsDto> UpdateAsync(UpdateDematAccountRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<DematAccountDetails>(request);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.ClientIdLast4 = GetLastFourDigits(entity.ClientId);
        entity.ClientId = _cryptoService.EncryptData(entity.ClientId);
        entity.BOIdLast4 = GetLastFourDigits(entity.BOId);
        if (!string.IsNullOrWhiteSpace(entity.BOId))
        {
            entity.BOId = _cryptoService.EncryptData(entity.BOId);
        }

        var result = await _repository.UpdateAsync(entity, cancellationToken);
        result.ClientId = request.ClientId;
        result.BOId = request.BOId;
        return _mapper.Map<DematAccountDetailsDto>(result);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await _repository.DeleteByIdAsync(id, userId.ToString(), cancellationToken);
    }

    private static string? GetLastFourDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        return value.Length <= 4 ? value : value[^4..];
    }
}
