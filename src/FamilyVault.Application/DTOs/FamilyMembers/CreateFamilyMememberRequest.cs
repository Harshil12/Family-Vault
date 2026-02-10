using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.FamilyMembers;

/// <summary>
/// Represents CreateFamilyMememberRequest.
/// </summary>
public class CreateFamilyMememberRequest
{
    /// <summary>
    /// Member's given name.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Member's family name / surname.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Family to which this member belongs.
    /// </summary>
    public Guid FamilyId { get; set; }

    /// <summary>
    /// Country calling code (e.g. +91).
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Mobile number without country code.
    /// </summary>
    public string? Mobile { get; set; }

    /// <summary>
    /// Relationship of the member with the family owner.
    /// </summary>
    public Relationships RelationshipType { get; set; }

    /// <summary>
    /// Date of birth (date only).
    /// </summary>
    public DateTimeOffset? DateOfBirth { get; set; }

    /// <summary>
    /// Blood group of the member.
    /// </summary>
    public BloodGroups? BloodGroup { get; set; }

    /// <summary>
    /// Email address for communication.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Permanent Account Number (PAN).
    /// </summary>
    public string? PAN { get; set; }

    /// <summary>
    /// Aadhaar number.
    /// </summary>
    public string? Aadhar { get; set; }
}
