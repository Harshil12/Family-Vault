using AutoMapper;
using FamilyVault.Application.DTOs.MutualFunds;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Services;

public class MutualFundService : IMutualFundService
{
    private readonly IMutualFundRepository _repository;
    private readonly ICryptoService _cryptoService;
    private readonly IMapper _mapper;

    public MutualFundService(IMutualFundRepository repository, ICryptoService cryptoService, IMapper mapper)
    {
        _repository = repository;
        _cryptoService = cryptoService;
        _mapper = mapper;
    }

    public async Task<MutualFundHoldingDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _repository.GetByIdAsync(id, cancellationToken);
        if (result != null)
        {
            result.FolioNumber = _cryptoService.DecryptData(result.FolioNumber);
        }
        return _mapper.Map<MutualFundHoldingDetailsDto>(result);
    }

    public async Task<IReadOnlyList<MutualFundHoldingDetailsDto>> GetByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var result = await _repository.GetAllByFamilyMemberIdAsync(familyMemberId, cancellationToken);
        foreach (var item in result)
        {
            item.FolioNumber = _cryptoService.DecryptData(item.FolioNumber);
        }
        return _mapper.Map<List<MutualFundHoldingDetailsDto>>(result);
    }

    public async Task<MutualFundHoldingDetailsDto> CreateAsync(CreateMutualFundHoldingRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<MutualFundHoldingDetails>(request);
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.CreatedBy = userId.ToString();
        entity.FolioNumberLast4 = GetLastFourDigits(entity.FolioNumber);
        entity.FolioNumber = _cryptoService.EncryptData(entity.FolioNumber);

        var result = await _repository.AddAsync(entity, cancellationToken);
        result.FolioNumber = request.FolioNumber;
        return _mapper.Map<MutualFundHoldingDetailsDto>(result);
    }

    public async Task<MutualFundHoldingDetailsDto> UpdateAsync(UpdateMutualFundHoldingRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<MutualFundHoldingDetails>(request);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.FolioNumberLast4 = GetLastFourDigits(entity.FolioNumber);
        entity.FolioNumber = _cryptoService.EncryptData(entity.FolioNumber);

        var result = await _repository.UpdateAsync(entity, cancellationToken);
        result.FolioNumber = request.FolioNumber;
        return _mapper.Map<MutualFundHoldingDetailsDto>(result);
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
