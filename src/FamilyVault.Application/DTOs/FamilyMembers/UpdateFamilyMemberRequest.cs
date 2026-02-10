using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.FamilyMembers;

/// <summary>
/// Represents UpdateFamilyMemberRequest.
/// </summary>
public class UpdateFamilyMemberRequest
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
    /// Gets or sets RelationshipType.
    /// </summary>
    public Relationships RelationshipType { get; set; }

    /// <summary>
    /// Gets or sets DateOfBirth.
    /// </summary>
    public DateTimeOffset? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets BloodGroup.
    /// </summary>
    public BloodGroups? BloodGroup { get; set; }

    /// <summary>
    /// Gets or sets Email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets PAN.
    /// </summary>
    public string? PAN { get; set; }

    /// <summary>
    /// Gets or sets Aadhar.
    /// </summary>
    public string ? Aadhar { get; set; }

    /// <summary>
    /// Gets or sets FamilyId.
    /// </summary>
    public Guid FamilyId { get; set; }
}
