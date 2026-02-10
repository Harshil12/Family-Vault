using FamilyVault.Application.DTOs.Family;

namespace FamilyVault.Application.DTOs.User;

/// <summary>
/// Represents UserDto.
/// </summary>
public class UserDto : BaseDto
{
    
    /// <summary>
    /// Gets or sets Username.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Gets or sets Email.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets FirstName.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Gets or sets LastName.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets CountryCode.
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Gets or sets Mobile.
    /// </summary>
    public string? Mobile { get; set; }

    /// <summary>
    /// Gets or sets Famillies.
    /// </summary>
    public ICollection<FamilyDto>? Famillies { get; set; }


}
