namespace FamilyVault.Application.Interfaces.Services;

/// <summary>
/// Represents ICryptoService.
/// </summary>
public interface ICryptoService
{
    /// <summary>
    /// Performs the HashPassword operation.
    /// </summary>
    public string HashPassword(string password);

    /// <summary>
    /// Performs the VerifyPassword operation.
    /// </summary>
    public bool VerifyPassword(string hashPassword, string password);

    /// <summary>
    /// Performs the EncryptData operation.
    /// </summary>
    public string EncryptData(string data);

    /// <summary>
    /// Performs the DecryptData operation.
    /// </summary>
    public string DecryptData(string encryptedData);

}
