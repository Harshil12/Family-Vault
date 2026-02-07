using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace FamilyValut.Tests.Application.Services.Others;

public class CryptoServiceTests
{
    private readonly CryptoService _sut;
    private readonly Mock<IDataProtectionProvider> _dataProtectorMock;
    private readonly Mock<IDataProtector> _protectorMock;

    public CryptoServiceTests()
    {
        _dataProtectorMock = new Mock<IDataProtectionProvider>();
        _protectorMock = new Mock<IDataProtector>();

        // Setup provider to return a protector that does round-trip Protect/Unprotect.
        _dataProtectorMock
            .Setup(p => p.CreateProtector(It.IsAny<string>()))
            .Returns(_protectorMock.Object);

        _protectorMock.Setup(p => p.Protect(It.IsAny<byte[]>()))
            .Returns((byte[] input) => input); // identity for bytes
        _protectorMock.Setup(p => p.Unprotect(It.IsAny<byte[]>()))
            .Returns((byte[] input) => input);

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
        var password = "MyStrongPassword@123";
        var hash = _sut.HashPassword(password);
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashes_ForSamePassword()
    {
        var password = "SamePassword";
        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void HashPassword_ShouldThrow_WhenPasswordIsNull()
    {
        Action act = () => _sut.HashPassword(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region VerifyPassword

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        var password = "CorrectPassword!";
        var hash = _sut.HashPassword(password);
        var result = _sut.VerifyPassword(hash, password);
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        var password = "CorrectPassword!";
        var wrongPassword = "WrongPassword!";
        var hash = _sut.HashPassword(password);
        var result = _sut.VerifyPassword(hash, wrongPassword);
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashIsInvalid()
    {
        var invalidHash = "invalid-hash";
        var password = "AnyPassword";
        var result = _sut.VerifyPassword(invalidHash, password);
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldThrow_WhenPasswordIsNull()
    {
        var hash = _sut.HashPassword("test");
        Action act = () => _sut.VerifyPassword(hash, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
