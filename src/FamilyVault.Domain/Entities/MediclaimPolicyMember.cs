namespace FamilyVault.Domain.Entities;

/// <summary>
/// Represents a family member covered under a mediclaim policy.
/// </summary>
public class MediclaimPolicyMember : BaseEntity
{
    public Guid MediclaimPolicyId { get; set; }
    public MediclaimPolicyDetails MediclaimPolicy { get; set; } = null!;
    public Guid FamilyMemberId { get; set; }
    public FamilyMember FamilyMember { get; set; } = null!;
    public string? RelationshipLabel { get; set; }
}
