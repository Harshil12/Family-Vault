using FamilyVault.Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;

namespace FamilyValut.Tests.Application.Services.Others;

public class CryptoServiceTests
{
    private readonly CryptoService _sut;

    public CryptoServiceTests()
    {
        var provider = new EphemeralDataProtectionProvider();
        _sut = new CryptoService(provider);
    }

    #region Encrypt / Decrypt

    [Fact]
    public void EncryptData_ShouldReturnProtectedValue()
    {
        // Arrange
        var plainText = "hello world";

        // Act
        var result = _sut.EncryptData(plainText);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().NotBe(plainText);
    }

    [Fact]
    public void DecryptData_ShouldRoundTrip()
    {
        // Arrange
        var plainText = "hello world";
        var encryptedText = _sut.EncryptData(plainText);

        // Act
        var result = _sut.DecryptData(encryptedText);

        // Assert
        result.Should().Be(plainText);
    }

    [Fact]
    public void DecryptData_ShouldThrow_WhenProtectorThrows()
    {
        // Arrange
        var invalidEncryptedData = "not-protected-data";

        // Act
        Action act = () => _sut.DecryptData(invalidEncryptedData);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Failed to decrypt the provided data.");
    }

    [Fact]
    public void EncryptData_ShouldHandleEmptyString()
    {
        // Arrange
        var emptyString = string.Empty;

        // Act
        var encrypted = _sut.EncryptData(emptyString);
        var decrypted = _sut.DecryptData(encrypted);

        // Assert
        decrypted.Should().Be(emptyString);
    }

    [Fact]
    public void EncryptData_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var specialData = "Test!@#$%^&*()_+{}[]|\\:;\"'<>,.?/~`";

        // Act
        var encrypted = _sut.EncryptData(specialData);
        var decrypted = _sut.DecryptData(encrypted);

        // Assert
        decrypted.Should().Be(specialData);
    }

    [Fact]
    public void EncryptData_ShouldHandleUnicodeCharacters()
    {
        // Arrange
        var unicodeData = "Hello 世界 🌍";

        // Act
        var encrypted = _sut.EncryptData(unicodeData);
        var decrypted = _sut.DecryptData(encrypted);

        // Assert
        decrypted.Should().Be(unicodeData);
    }

    #endregion

    #region HashPassword

    [Fact]
    public void HashPassword_ShouldReturn_NonEmptyHash()
    {
        // Arrange
        var password = "MyStrongPassword@123";

        // Act
        var hash = _sut.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashes_ForSamePassword()
    {
        // Arrange
        var password = "SamePassword";

        // Act
        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void HashPassword_ShouldThrow_WhenPasswordIsNull()
    {
        // Act
        Action act = () => _sut.HashPassword(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region VerifyPassword

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        // Arrange
        var password = "CorrectPassword!";
        var hash = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(hash, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        var password = "CorrectPassword!";
        var wrongPassword = "WrongPassword!";
        var hash = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(hash, wrongPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashIsInvalid()
    {
        // Arrange
        var invalidHash = "invalid-hash";
        var password = "AnyPassword";

        // Act
        Action act = () => _sut.VerifyPassword(invalidHash, password);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Malformed password hash.");
    }

    [Fact]
    public void VerifyPassword_ShouldThrow_WhenPasswordIsNull()
    {
        // Arrange
        var hash = _sut.HashPassword("test");

        // Act
        Action act = () => _sut.VerifyPassword(hash, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_ForEmptyPassword()
    {
        // Arrange
        var emptyPassword = string.Empty;
        var hash = _sut.HashPassword(emptyPassword);

        // Act
        var result = _sut.VerifyPassword(hash, emptyPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_ForEmptyHashWithNonEmptyPassword()
    {
        // Arrange
        var emptyHash = string.Empty;
        var password = "SomePassword";

        // Act
        var result = _sut.VerifyPassword(emptyHash, password);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}

