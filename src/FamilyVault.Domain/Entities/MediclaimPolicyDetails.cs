using FamilyVault.Domain.Enums;

namespace FamilyVault.Domain.Entities;

/// <summary>
/// Represents mediclaim policy details owned by a family member.
/// </summary>
public class MediclaimPolicyDetails : BaseEntity
{
    public string InsurerName { get; set; } = null!;
    public string PolicyNumber { get; set; } = null!;
    public string? PolicyNumberLast4 { get; set; }
    public MediclaimPolicyType PolicyType { get; set; }
    public string? PlanName { get; set; }
    public decimal SumInsured { get; set; }
    public decimal PremiumAmount { get; set; }
    public DateOnly PolicyStartDate { get; set; }
    public DateOnly PolicyEndDate { get; set; }
    public string? TPAName { get; set; }
    public string? HospitalNetworkUrl { get; set; }
    public PolicyStatus Status { get; set; }
    public Guid FamilyMemberId { get; set; }
    public FamilyMember FamilyMember { get; set; } = null!;
    public ICollection<MediclaimPolicyMember>? CoveredMembers { get; set; }
}
