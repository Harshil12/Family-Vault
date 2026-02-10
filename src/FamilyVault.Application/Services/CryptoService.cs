using FamilyVault.Application.Interfaces.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace FamilyVault.Application.Services;

/// <summary>
/// Represents CryptoService.
/// </summary>
public sealed class CryptoService : ICryptoService
{
    private readonly IDataProtector _protector;
    private readonly PasswordHasher<object?> _passwordHasher = new();

    /// <summary>
    /// Initializes a new instance of CryptoService.
    /// </summary>
    public CryptoService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("FamilyVault.SensitiveData.v1");
    }

    // 🔐 For application data (email, phone, notes, etc.)
    /// <summary>
    /// Performs the EncryptData operation.
    /// </summary>
    public string EncryptData(string data)
        => _protector.Protect(data);

    /// <summary>
    /// Performs the DecryptData operation.
    /// </summary>
    public string DecryptData(string encryptedData)
        => _protector.Unprotect(encryptedData);

    // 🔑 For passwords
    /// <summary>
    /// Performs the HashPassword operation.
    /// </summary>
    public string HashPassword(string password)
        => _passwordHasher.HashPassword(null, password);

    /// <summary>
    /// Performs the VerifyPassword operation.
    /// </summary>
    public bool VerifyPassword(string hashPassword, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(
             null,
             hashPassword,
             password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
