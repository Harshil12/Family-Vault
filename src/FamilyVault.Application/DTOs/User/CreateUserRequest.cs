namespace FamilyVault.Application.DTOs.User;

public class CreateUserRequest
{
    /// <summary>
    /// Unique username for login and display.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// User email address.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Plain text password (will be hashed server-side).
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// User's given name.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// User's family name / surname.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Country calling code (e.g. +91).
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Mobile number without country code.
    /// </summary>
    public string? Mobile { get; set; }
}
