namespace FamilyVault.Application.DTOs.User;

public class UpdateUserRequest
{
    public Guid Id { get; set; }    

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? CountryCode { get; set; }

    public string? Mobile { get; set; }

    public string Email { get; set; } = null!;

    public string? Password { get; set; }
}
