using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.FixedDeposits;

public class FixedDepositDetailsDto : BaseDto
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
}

public class CreateFixedDepositRequest
{
    public string InstitutionName { get; set; } = null!;
    public string DepositNumber { get; set; } = null!;
    public FixedDepositType DepositType { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly MaturityDate { get; set; }
    public decimal? MaturityAmount { get; set; }
    public bool IsAutoRenewal { get; set; }
    public string? NomineeName { get; set; }
    public Guid FamilyMemberId { get; set; }
}

public class UpdateFixedDepositRequest : CreateFixedDepositRequest
{
    public Guid Id { get; set; }
}
