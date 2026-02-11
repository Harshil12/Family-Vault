using FamilyVault.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FamilyVault.Domain.Entities;

/// <summary>
/// Represents FamilyMember.
/// </summary>
public class FamilyMember : BaseEntity  
{
    /// <summary>
    /// User's given name.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// User's family name / surname.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Identifier of the owning <see cref="Family"/>.
    /// This acts as the foreign key linking the family to its owner.
    /// </summary>
    public Guid FamilyId { get; set; }

    /// <summary>
    /// Gets or sets the country calling code in international format.
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// User's phone number.
    /// </summary>
    [MaxLength(10)]
    public long? Mobile { get; set; }

    /// <summary>
    /// The relationship of this member to the family's primary user/owner.
    /// Uses the <see cref="Relationships"/> enum to represent common relationships
    /// (for example <see cref="Relationships.Self"/>, <see cref="Relationships.Spouse"/>,
    /// <see cref="Relationships.Parent"/>, <see cref="Relationships.Child"/>).
    /// When the relationship is unknown use <see cref="Relationships.Other"/>.
    /// </summary>
    public Relationships RelationshipType { get; set; }

    /// <summary>
    /// Date of birth of the person.
    /// Stored as a date-only value; time and timezone are not applicable.
    /// </summary>
    public DateTimeOffset? DateOfBirth { get; set; }

    /// <summary>
    /// Blood group of the person.
    /// Optional value represented using the <see cref="BloodGroups"/> enumeration.
    /// </summary>
    public BloodGroups? BloodGroup { get; set; }

    /// <summary>
    /// User's email address used for contact, notifications, and as an alternate identifier.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Permanent Account Number (PAN) issued by the Income Tax Department of India.
    /// This is a 10-character alphanumeric identifier (e.g., ABCDE1234F).
    /// </summary>
    [MaxLength(10)]
    public string? PAN { get; set; }

    /// <summary>
    ///Unique identifier for individuals issued by the Government of India.
    /// This is a 12-digit numeric identifier (e.g., 1234 5678 9012).
    /// </summary>
    [MaxLength(12)]
    public long? Aadhar { get; set; }

    /// <summary>
    /// Navigation property to the owning <see cref="Family"/>.
    /// Should be populated when the owner information is required.
    /// </summary>
    public Family Family { get; set; } = null!;

    /// <summary>
    /// List of related <see cref="DocumentDetails"/> entities that belong to this member.
    /// </summary>
    public ICollection<DocumentDetails>? DocumentDetails { get; set; }

    /// <summary>
    /// List of related <see cref="BankAccountDetails"/> entities that belong to this member.
    /// </summary>
    public ICollection<BankAccountDetails>? BankAccountDetails { get; set; }

}
