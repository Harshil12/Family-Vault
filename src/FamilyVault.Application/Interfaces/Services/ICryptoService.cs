namespace FamilyVault.Application.Interfaces.Services;

public interface ICryptoService
{
    public string HashPassword(string password);

    public bool VerifyPassword(string hashPassword, string password);

    public string EncryptData(string data);

    public string DecryptData(string encryptedData);

}
