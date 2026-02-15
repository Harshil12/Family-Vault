using AutoMapper;
using FamilyVault.Application.DTOs.Mediclaim;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Services;

public class MediclaimService : IMediclaimService
{
    private readonly IMediclaimRepository _repository;
    private readonly ICryptoService _cryptoService;
    private readonly IMapper _mapper;

    public MediclaimService(IMediclaimRepository repository, ICryptoService cryptoService, IMapper mapper)
    {
        _repository = repository;
        _cryptoService = cryptoService;
        _mapper = mapper;
    }

    public async Task<MediclaimPolicyDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _repository.GetByIdAsync(id, cancellationToken);
        if (result != null)
        {
            result.PolicyNumber = _cryptoService.DecryptData(result.PolicyNumber);
        }
        return _mapper.Map<MediclaimPolicyDetailsDto>(result);
    }

    public async Task<IReadOnlyList<MediclaimPolicyDetailsDto>> GetByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var result = await _repository.GetAllByFamilyMemberIdAsync(familyMemberId, cancellationToken);
        foreach (var item in result)
        {
            item.PolicyNumber = _cryptoService.DecryptData(item.PolicyNumber);
        }
        return _mapper.Map<List<MediclaimPolicyDetailsDto>>(result);
    }

    public async Task<MediclaimPolicyDetailsDto> CreateAsync(CreateMediclaimPolicyRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<MediclaimPolicyDetails>(request);
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.CreatedBy = userId.ToString();
        entity.PolicyNumberLast4 = GetLastFourDigits(entity.PolicyNumber);
        entity.PolicyNumber = _cryptoService.EncryptData(entity.PolicyNumber);

        var result = await _repository.AddAsync(entity, cancellationToken);
        result.PolicyNumber = request.PolicyNumber;
        return _mapper.Map<MediclaimPolicyDetailsDto>(result);
    }

    public async Task<MediclaimPolicyDetailsDto> UpdateAsync(UpdateMediclaimPolicyRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<MediclaimPolicyDetails>(request);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.PolicyNumberLast4 = GetLastFourDigits(entity.PolicyNumber);
        entity.PolicyNumber = _cryptoService.EncryptData(entity.PolicyNumber);

        var result = await _repository.UpdateAsync(entity, cancellationToken);
        result.PolicyNumber = request.PolicyNumber;
        return _mapper.Map<MediclaimPolicyDetailsDto>(result);
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
