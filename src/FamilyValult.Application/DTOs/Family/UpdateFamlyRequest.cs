using System.ComponentModel.DataAnnotations;

namespace FamilyVault.Application.DTOs.Family;

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
