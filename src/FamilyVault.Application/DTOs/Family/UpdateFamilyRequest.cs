namespace FamilyVault.Application.DTOs.Family;

/// <summary>
/// Represents UpdateFamilyRequest.
/// </summary>
public class UpdateFamilyRequest
{

    /// <summary>
    /// Unique identifier of the family.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Updated family name.
    /// </summary>
    public string FamilyName { get; set; } = null!;
}
