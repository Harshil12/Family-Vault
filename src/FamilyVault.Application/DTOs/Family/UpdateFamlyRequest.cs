namespace FamilyVault.Application.DTOs.Family;

/// <summary>
/// Represents UpdateFamlyRequest.
/// </summary>
public class UpdateFamlyRequest
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
