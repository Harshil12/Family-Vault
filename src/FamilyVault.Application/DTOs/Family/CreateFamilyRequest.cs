namespace FamilyVault.Application.DTOs.Family;

public class CreateFamilyRequest
{
    /// <summary>
    /// Human-readable name of the family (e.g. "Smith Family").
    /// </summary>
    public string FamilyName { get; set; } = null!;
}
