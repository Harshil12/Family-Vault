using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface IMediclaimRepository
{
    Task<MediclaimPolicyDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<MediclaimPolicyDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken);
    Task<MediclaimPolicyDetails> AddAsync(MediclaimPolicyDetails item, CancellationToken cancellationToken);
    Task<MediclaimPolicyDetails> UpdateAsync(MediclaimPolicyDetails item, CancellationToken cancellationToken);
    Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken);
}
