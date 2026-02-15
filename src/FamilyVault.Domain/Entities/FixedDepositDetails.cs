using FamilyVault.Domain.Enums;

namespace FamilyVault.Domain.Entities;

/// <summary>
/// Represents fixed deposit details for a family member.
/// </summary>
public class FixedDepositDetails : BaseEntity
{
    public string InstitutionName { get; set; } = null!;
    public string DepositNumber { get; set; } = null!;
    public string? DepositNumberLast4 { get; set; }
    public FixedDepositType DepositType { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly MaturityDate { get; set; }
    public decimal? MaturityAmount { get; set; }
    public bool IsAutoRenewal { get; set; }
    public string? NomineeName { get; set; }
    public Guid FamilyMemberId { get; set; }
    public FamilyMember FamilyMember { get; set; } = null!;
}
