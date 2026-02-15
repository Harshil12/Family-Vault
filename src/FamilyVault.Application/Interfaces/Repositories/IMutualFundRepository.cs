using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface IMutualFundRepository
{
    Task<MutualFundHoldingDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<MutualFundHoldingDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken);
    Task<MutualFundHoldingDetails> AddAsync(MutualFundHoldingDetails item, CancellationToken cancellationToken);
    Task<MutualFundHoldingDetails> UpdateAsync(MutualFundHoldingDetails item, CancellationToken cancellationToken);
    Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken);
}
