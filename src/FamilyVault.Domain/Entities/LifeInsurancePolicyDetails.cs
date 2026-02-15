using FamilyVault.Domain.Enums;

namespace FamilyVault.Domain.Entities;

/// <summary>
/// Represents life insurance policy details for a family member.
/// </summary>
public class LifeInsurancePolicyDetails : BaseEntity
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
    public FamilyMember FamilyMember { get; set; } = null!;
}
