using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.LifeInsurance;

public class LifeInsurancePolicyDetailsDto : BaseDto
{
    public string InsurerName { get; set; } = null!;
    public string PolicyNumber { get; set; } = null!;
    public string? PolicyNumberLast4 { get; set; }
    public LifeInsurancePolicyType PolicyType { get; set; }
    public string? PlanName { get; set; }
    public decimal CoverAmount { get; set; }
    public decimal PremiumAmount { get; set; }
    public PremiumFrequency PremiumFrequency { get; set; }
    public DateOnly PolicyStartDate { get; set; }
    public DateOnly? PolicyEndDate { get; set; }
    public DateOnly? MaturityDate { get; set; }
    public string? NomineeName { get; set; }
    public string? AgentName { get; set; }
    public PolicyStatus Status { get; set; }
    public Guid FamilyMemberId { get; set; }
}

public class CreateLifeInsurancePolicyRequest
{
    public string InsurerName { get; set; } = null!;
    public string PolicyNumber { get; set; } = null!;
    public LifeInsurancePolicyType PolicyType { get; set; }
    public string? PlanName { get; set; }
    public decimal CoverAmount { get; set; }
    public decimal PremiumAmount { get; set; }
    public PremiumFrequency PremiumFrequency { get; set; }
    public DateOnly PolicyStartDate { get; set; }
    public DateOnly? PolicyEndDate { get; set; }
    public DateOnly? MaturityDate { get; set; }
    public string? NomineeName { get; set; }
    public string? AgentName { get; set; }
    public PolicyStatus Status { get; set; }
    public Guid FamilyMemberId { get; set; }
}

public class UpdateLifeInsurancePolicyRequest : CreateLifeInsurancePolicyRequest
{
    public Guid Id { get; set; }
}
