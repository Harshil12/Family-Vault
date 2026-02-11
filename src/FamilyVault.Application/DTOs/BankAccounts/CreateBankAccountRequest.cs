using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.BankAccounts;

/// <summary>
/// Represents CreateBankAccountRequest.
/// </summary>
public class CreateBankAccountRequest
{
    /// <summary>
    /// Name of the bank.
    /// </summary>
    public string BankName { get; set; } = null!;

    /// <summary>
    /// Account number in plain text (stored encrypted in DB).
    /// </summary>
    public string AccountNumber { get; set; } = null!;

    /// <summary>
    /// Type of account.
    /// </summary>
    public BankAccountType AccountType { get; set; }

    /// <summary>
    /// Optional account holder name.
    /// </summary>
    public string? AccountHolderName { get; set; }

    /// <summary>
    /// Optional IFSC code.
    /// </summary>
    public string? IFSC { get; set; }

    /// <summary>
    /// Optional branch name.
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// Family member to whom the account belongs.
    /// </summary>
    public Guid FamilyMemberId { get; set; }
}
