namespace FamilyVault.Application.DTOs.Family;

public class FamilyDto : BaseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid UserId { get; set; }

}
