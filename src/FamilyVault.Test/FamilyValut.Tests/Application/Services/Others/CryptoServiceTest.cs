using FamilyVault.Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using System.Text;

namespace FamilyValut.Tests.Application.Services.Others;

public class CryptoServiceTests
{
    private readonly CryptoService _sut;
    private readonly Mock<IDataProtectionProvider> _dataProtectorMock;

    public CryptoServiceTests()
    {
        _dataProtectorMock = new Mock<IDataProtectionProvider>();
        _sut = new CryptoService(_dataProtectorMock.Object);
    }

    #region Encrypt / Decrypt

    [Fact]
    public void EncryptData_ShouldThrow_WhenKeyIsNotValidBase64()
    {
        // Arrange
        var plainText = "hello world";

        // Act
        Action act = () => _sut.EncryptData(plainText);

        // Assert
        act.Should().Throw<FormatException>()
           .WithMessage("*base-64*");
    }

    [Fact]
    public void DecryptData_ShouldThrow_WhenKeyIsNotValidBase64()
    {
        // Arrange
        var encryptedText = Convert.ToBase64String(Encoding.UTF8.GetBytes("dummy"));

        // Act
        Action act = () => _sut.DecryptData(encryptedText);

        // Assert
        act.Should().Throw<FormatException>()
           .WithMessage("*base-64*");
    }

    [Fact]
    public void DecryptData_ShouldThrow_WhenEncryptedDataIsNotBase64()
    {
        // Arrange
        var invalidEncryptedData = "not-base64-data";

        // Act
        Action act = () => _sut.DecryptData(invalidEncryptedData);

        // Assert
        act.Should().Throw<FormatException>();
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
        var result = _sut.VerifyPassword(invalidHash, password);

        // Assert
        result.Should().BeFalse();
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

    #endregion
}
