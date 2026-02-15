using FamilyVault.Domain.Enums;

namespace FamilyVault.Domain.Entities;

/// <summary>
/// Represents demat account details for a family member.
/// </summary>
public class DematAccountDetails : BaseEntity
{
    public string BrokerName { get; set; } = null!;
    public DepositoryType Depository { get; set; }
    public string DPId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string? ClientIdLast4 { get; set; }
    public string? BOId { get; set; }
    public string? BOIdLast4 { get; set; }
    public HoldingPatternType HoldingPattern { get; set; }
    public string? NomineeName { get; set; }
    public Guid FamilyMemberId { get; set; }
    public FamilyMember FamilyMember { get; set; } = null!;
}
