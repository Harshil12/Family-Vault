using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.FamilyMembers;

/// <summary>
/// Represents FamilyMemberDto.
/// </summary>
public class FamilyMemberDto: BaseDto 
{

    /// <summary>
    /// Gets or sets FamilyId.
    /// </summary>
    public Guid FamilyId { get; set; }

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
    public long? Mobile { get; set; }

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
    public long? Aadhar { get; set; }

    /// <summary>
    /// Gets or sets DocumentDetails.
    /// </summary>
    public ICollection<DocumentDetailsDto>? DocumentDetails { get; set; }

}
