using FamilyVault.Domain.Enums;

namespace FamilyVault.Domain.Entities;

/// <summary>
/// Represents BankAccountDetails.
/// </summary>
public class BankAccountDetails : BaseEntity
{
    /// <summary>
    /// Name of the bank.
    /// </summary>
    public string BankName { get; set; } = null!;

    /// <summary>
    /// Encrypted bank account number.
    /// </summary>
    public string AccountNumber { get; set; } = null!;

    /// <summary>
    /// Optional last 4 digits for masked display.
    /// </summary>
    public string? AccountNumberLast4 { get; set; }

    /// <summary>
    /// Type of bank account.
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
    /// Optional branch.
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// Identifier linking the account to a family member.
    /// </summary>
    public Guid FamilyMemberId { get; set; }

    /// <summary>
    /// Navigation property to family member.
    /// </summary>
    public FamilyMember FamilyMember { get; set; } = null!;
}
