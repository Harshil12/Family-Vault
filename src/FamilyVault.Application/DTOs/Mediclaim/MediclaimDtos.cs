using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.Mediclaim;

public class MediclaimPolicyDetailsDto : BaseDto
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
}

public class CreateMediclaimPolicyRequest
{
    public string InsurerName { get; set; } = null!;
    public string PolicyNumber { get; set; } = null!;
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
}

public class UpdateMediclaimPolicyRequest : CreateMediclaimPolicyRequest
{
    public Guid Id { get; set; }
}
