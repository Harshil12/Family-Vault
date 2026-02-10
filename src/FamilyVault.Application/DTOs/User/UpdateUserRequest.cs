namespace FamilyVault.Application.DTOs.User;

/// <summary>
/// Represents UpdateUserRequest.
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// Gets or sets Id.
    /// </summary>
    public Guid Id { get; set; }    

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
    /// Gets or sets Email.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets Password.
    /// </summary>
    public string? Password { get; set; }
}
