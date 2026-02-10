using FamilyVault.Application.DTOs.FamilyMembers;

namespace FamilyVault.Application.DTOs.Family;

/// <summary>
/// Represents FamilyDto.
/// </summary>
public class FamilyDto : BaseDto
{

    /// <summary>
    /// Gets or sets Name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets UserId.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets FamilyMembers.
    /// </summary>
    public ICollection<FamilyMemberDto>? FamilyMembers { get; set; }

}
