using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface ILifeInsuranceRepository
{
    Task<LifeInsurancePolicyDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<LifeInsurancePolicyDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken);
    Task<LifeInsurancePolicyDetails> AddAsync(LifeInsurancePolicyDetails item, CancellationToken cancellationToken);
    Task<LifeInsurancePolicyDetails> UpdateAsync(LifeInsurancePolicyDetails item, CancellationToken cancellationToken);
    Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken);
}
