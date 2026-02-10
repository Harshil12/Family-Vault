namespace FamilyVault.Application.Interfaces.Services;

/// <summary>
/// Represents IAuthService.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Performs the GetTokenAsync operation.
    /// </summary>
    public Task<string?> GetTokenAsync(string email, string password, CancellationToken cancellationToken);
}
