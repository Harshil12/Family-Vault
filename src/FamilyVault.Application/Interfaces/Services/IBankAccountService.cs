using FamilyVault.Application.DTOs.BankAccounts;

namespace FamilyVault.Application.Interfaces.Services;

/// <summary>
/// Defines contract for bank account management.
/// </summary>
public interface IBankAccountService
{
    /// <summary>
    /// Gets bank account by id.
    /// </summary>
    public Task<BankAccountDetailsDto> GetBankAccountByIdAsync(Guid bankAccountId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all bank accounts by family member id.
    /// </summary>
    public Task<IReadOnlyList<BankAccountDetailsDto>> GetBankAccountsByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates bank account.
    /// </summary>
    public Task<BankAccountDetailsDto> CreateBankAccountAsync(CreateBankAccountRequest createBankAccountRequest, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates bank account.
    /// </summary>
    public Task<BankAccountDetailsDto> UpdateBankAccountAsync(UpdateBankAccountRequest updateBankAccountRequest, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes bank account by id.
    /// </summary>
    public Task DeleteBankAccountByIdAsync(Guid bankAccountId, Guid userId, CancellationToken cancellationToken);
}
