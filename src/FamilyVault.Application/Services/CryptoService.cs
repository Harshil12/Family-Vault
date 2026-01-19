using FamilyVault.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace FamilyVault.Application.Services;

public class CryptoService : ICryptoService
{
    private const string key = "VERY_SECRET_32_CHAR_KEY!!";
    private readonly PasswordHasher<object?> _passwordHasher = new();
    
    public string DecryptData(string encryptedData)
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = Convert.FromBase64String(key);
        aes.IV = new byte[16];

        var decryptor = aes.CreateDecryptor();
        var bytes = Convert.FromBase64String(encryptedData);

        return Encoding.UTF8.GetString(decryptor.TransformFinalBlock(bytes, 0, bytes.Length));
    }

    public string EncryptData(string data)
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = Convert.FromBase64String(key);
        aes.IV = new byte[16];

        var encryptor = aes.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(data);

        return Encoding.UTF8.GetString(encryptor.TransformFinalBlock(bytes, 0, bytes.Length));
    }

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null, password);
    }

    public bool VerifyPassword(string hashPassword, string password)
    {
        try
        {
            return _passwordHasher.VerifyHashedPassword(
                null,
                hashPassword,
                password) == PasswordVerificationResult.Success;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
