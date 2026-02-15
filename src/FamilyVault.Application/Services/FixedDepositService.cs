using AutoMapper;
using FamilyVault.Application.DTOs.FixedDeposits;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Services;

public class FixedDepositService : IFixedDepositService
{
    private readonly IFixedDepositRepository _repository;
    private readonly ICryptoService _cryptoService;
    private readonly IMapper _mapper;

    public FixedDepositService(IFixedDepositRepository repository, ICryptoService cryptoService, IMapper mapper)
    {
        _repository = repository;
        _cryptoService = cryptoService;
        _mapper = mapper;
    }

    public async Task<FixedDepositDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _repository.GetByIdAsync(id, cancellationToken);
        if (result != null)
        {
            result.DepositNumber = _cryptoService.DecryptData(result.DepositNumber);
        }
        return _mapper.Map<FixedDepositDetailsDto>(result);
    }

    public async Task<IReadOnlyList<FixedDepositDetailsDto>> GetByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var result = await _repository.GetAllByFamilyMemberIdAsync(familyMemberId, cancellationToken);
        foreach (var item in result)
        {
            item.DepositNumber = _cryptoService.DecryptData(item.DepositNumber);
        }
        return _mapper.Map<List<FixedDepositDetailsDto>>(result);
    }

    public async Task<FixedDepositDetailsDto> CreateAsync(CreateFixedDepositRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<FixedDepositDetails>(request);
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.CreatedBy = userId.ToString();
        entity.DepositNumberLast4 = GetLastFourDigits(entity.DepositNumber);
        entity.DepositNumber = _cryptoService.EncryptData(entity.DepositNumber);

        var result = await _repository.AddAsync(entity, cancellationToken);
        result.DepositNumber = request.DepositNumber;
        return _mapper.Map<FixedDepositDetailsDto>(result);
    }

    public async Task<FixedDepositDetailsDto> UpdateAsync(UpdateFixedDepositRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<FixedDepositDetails>(request);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.DepositNumberLast4 = GetLastFourDigits(entity.DepositNumber);
        entity.DepositNumber = _cryptoService.EncryptData(entity.DepositNumber);

        var result = await _repository.UpdateAsync(entity, cancellationToken);
        result.DepositNumber = request.DepositNumber;
        return _mapper.Map<FixedDepositDetailsDto>(result);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await _repository.DeleteByIdAsync(id, userId.ToString(), cancellationToken);
    }

    private static string? GetLastFourDigits(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        return value.Length <= 4 ? value : value[^4..];
    }
}
