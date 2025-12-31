using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.FamilyMembers;

public class FamilyMemberDto
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? CountryCode { get; set; }

    public long? Mobile { get; set; }

    public Relationships RelationshipType { get; set; }

    public DateTimeOffset? DateOfBirth { get; set; }

    public BloodGroups? BloodGroup { get; set; }

    public string? Email { get; set; }

    public long? PAN { get; set; }

    public long? Aadhar { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string? UpdatedBy { get; set; }
}
