using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.BankAccounts;

/// <summary>
/// Represents BankAccountDetailsDto.
/// </summary>
public class BankAccountDetailsDto : BaseDto
{
    /// <summary>
    /// Name of bank.
    /// </summary>
    public string BankName { get; set; } = null!;

    /// <summary>
    /// Decrypted account number.
    /// </summary>
    public string AccountNumber { get; set; } = null!;

    /// <summary>
    /// Optional last 4 digits.
    /// </summary>
    public string? AccountNumberLast4 { get; set; }

    /// <summary>
    /// Account type.
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
    /// Optional nominee name.
    /// </summary>
    public string? NomineeName { get; set; }

    /// <summary>
    /// Family member id.
    /// </summary>
    public Guid FamilyMemberId { get; set; }
}
