using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface IDematAccountRepository
{
    Task<DematAccountDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<DematAccountDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken);
    Task<DematAccountDetails> AddAsync(DematAccountDetails item, CancellationToken cancellationToken);
    Task<DematAccountDetails> UpdateAsync(DematAccountDetails item, CancellationToken cancellationToken);
    Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken);
}
