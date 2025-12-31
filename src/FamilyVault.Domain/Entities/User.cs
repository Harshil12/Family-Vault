using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FamilyVault.Domain.Entities;


/// <summary>
/// Represents an application user with identity and profile information.
/// Inherits common entity properties from <see cref="BaseEntity"/>.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Unique username used for authentication and display.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// User's email address used for contact, notifications, and as an alternate identifier.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Hashed password used for authentication.
    /// Marked with <see cref="JsonIgnoreAttribute"/> so it is not included in JSON payloads.
    /// Store and compare only hashed values; never expose plain-text passwords.
    /// </summary>
    [JsonIgnore]
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
    /// Gets or sets the country calling code in international format.
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// User's phone number.
    /// </summary>
    [MaxLength(10)]
    public string? Mobile { get; set; }
}

