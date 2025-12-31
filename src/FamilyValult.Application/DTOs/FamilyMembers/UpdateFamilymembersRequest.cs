using FamilyVault.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FamilyVault.Application.DTOs.FamilyMembers;

public class UpdateFamilyMememberRequest
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? CountryCode { get; set; }

    public string? Mobile { get; set; }

    public Relationships RelationshipType { get; set; }

    public DateTimeOffset? DateOfBirth { get; set; }

    public BloodGroups? BloodGroup { get; set; }

    public string? Email { get; set; }

    public string? PAN { get; set; }

    public string ? Aadhar { get; set; }

    public Guid FamilyId { get; set; }
}
