using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.DTOs.FamilyMembers;

namespace FamilyVault.Application.DTOs.User;

public class UserDto : BaseDto
{
    
    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? CountryCode { get; set; }

    public string? Mobile { get; set; }

    public ICollection<FamilyDto>? Famillies { get; set; }


}
