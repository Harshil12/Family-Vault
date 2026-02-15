using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.MutualFunds;

public class MutualFundHoldingDetailsDto : BaseDto
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
}

public class CreateMutualFundHoldingRequest
{
    public string AMCName { get; set; } = null!;
    public string FolioNumber { get; set; } = null!;
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
}

public class UpdateMutualFundHoldingRequest : CreateMutualFundHoldingRequest
{
    public Guid Id { get; set; }
}
