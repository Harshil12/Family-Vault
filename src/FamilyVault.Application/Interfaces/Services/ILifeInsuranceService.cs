using FamilyVault.Application.DTOs.LifeInsurance;

namespace FamilyVault.Application.Interfaces.Services;

public interface ILifeInsuranceService
{
    Task<LifeInsurancePolicyDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<LifeInsurancePolicyDetailsDto>> GetByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken);
    Task<LifeInsurancePolicyDetailsDto> CreateAsync(CreateLifeInsurancePolicyRequest request, Guid userId, CancellationToken cancellationToken);
    Task<LifeInsurancePolicyDetailsDto> UpdateAsync(UpdateLifeInsurancePolicyRequest request, Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
