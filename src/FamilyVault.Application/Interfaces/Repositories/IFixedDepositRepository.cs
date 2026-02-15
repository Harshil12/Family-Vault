using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface IFixedDepositRepository
{
    Task<FixedDepositDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<FixedDepositDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken);
    Task<FixedDepositDetails> AddAsync(FixedDepositDetails item, CancellationToken cancellationToken);
    Task<FixedDepositDetails> UpdateAsync(FixedDepositDetails item, CancellationToken cancellationToken);
    Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken);
}
