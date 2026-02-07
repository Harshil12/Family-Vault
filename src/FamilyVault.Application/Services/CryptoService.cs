using FamilyVault.Application.Interfaces.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace FamilyVault.Application.Services;

public sealed class CryptoService : ICryptoService
{
    private readonly IDataProtector _protector;
    private readonly PasswordHasher<object?> _passwordHasher = new();

    public CryptoService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("FamilyVault.SensitiveData.v1");
    }

    // 🔐 For application data (email, phone, notes, etc.)
    public string EncryptData(string data)
        => _protector.Protect(data);

    public string DecryptData(string encryptedData)
        => _protector.Unprotect(encryptedData);

    // 🔑 For passwords
    public string HashPassword(string password)
        => _passwordHasher.HashPassword(null, password);

    public bool VerifyPassword(string hashPassword, string password)
        => _passwordHasher.VerifyHashedPassword(
            null,
            hashPassword,
            password) == PasswordVerificationResult.Success;
}
