using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

/// <summary>
/// Defines a contract for managing bank accounts in repository.
/// </summary>
public interface IBankAccountRepository
{
    /// <summary>
    /// Gets bank account by id.
    /// </summary>
    public Task<BankAccountDetails?> GetByIdAsync(Guid bankAccountId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all bank accounts for a family member.
    /// </summary>
    public Task<IReadOnlyList<BankAccountDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds bank account.
    /// </summary>
    public Task<BankAccountDetails> AddAsync(BankAccountDetails bankAccount, CancellationToken cancellationToken);

    /// <summary>
    /// Updates bank account.
    /// </summary>
    public Task<BankAccountDetails> UpdateAsync(BankAccountDetails bankAccount, CancellationToken cancellationToken);

    /// <summary>
    /// Soft deletes bank account by id.
    /// </summary>
    public Task DeleteByIdAsync(Guid bankAccountId, string user, CancellationToken cancellationToken);
}
