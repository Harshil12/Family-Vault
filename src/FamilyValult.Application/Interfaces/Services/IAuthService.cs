namespace FamilyVault.Application.Interfaces.Services;

public interface IAuthService
{
    public Task<string?> GetTokenAsync(string email, string password, CancellationToken cancellationToken);
}
