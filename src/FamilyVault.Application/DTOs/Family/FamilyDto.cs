using FamilyVault.Application.DTOs.FamilyMembers;

namespace FamilyVault.Application.DTOs.Family;

public class FamilyDto : BaseDto
{

    public string Name { get; set; } = null!;

    public Guid UserId { get; set; }

    public ICollection<FamilyMemberDto>? FamilyMembers { get; set; }

}
