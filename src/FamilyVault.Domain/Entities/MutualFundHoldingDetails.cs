using FamilyVault.Domain.Enums;

namespace FamilyVault.Domain.Entities;

/// <summary>
/// Represents mutual fund holdings for a family member.
/// </summary>
public class MutualFundHoldingDetails : BaseEntity
{
    public string AMCName { get; set; } = null!;
    public string FolioNumber { get; set; } = null!;
    public string? FolioNumberLast4 { get; set; }
    public string SchemeName { get; set; } = null!;
    public MutualFundSchemeType SchemeType { get; set; }
    public MutualFundPlanType PlanType { get; set; }
    public MutualFundOptionType OptionType { get; set; }
    public InvestmentModeType InvestmentMode { get; set; }
    public decimal? Units { get; set; }
    public decimal? InvestedAmount { get; set; }
    public decimal? CurrentValue { get; set; }
    public DateOnly? StartDate { get; set; }
    public string? NomineeName { get; set; }
    public Guid FamilyMemberId { get; set; }
    public FamilyMember FamilyMember { get; set; } = null!;
}
