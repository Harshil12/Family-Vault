using AutoMapper;
using FamilyVault.Application.DTOs.BankAccounts;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Application.Services;
using FamilyVault.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FamilyVault.Tests.Services;

public class BankAccountServiceTests
{
    private readonly Mock<IBankAccountRepository> _bankAccountRepoMock;
    private readonly Mock<ICryptoService> _cryptoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<BankAccountService>> _loggerMock;

    private readonly BankAccountService _sut;

    public BankAccountServiceTests()
    {
        _bankAccountRepoMock = new Mock<IBankAccountRepository>();
        _cryptoMock = new Mock<ICryptoService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<BankAccountService>>();

        _sut = new BankAccountService(
            _bankAccountRepoMock.Object,
            _cryptoMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    #region GetBankAccountByIdAsync

    [Fact]
    public async Task GetBankAccountByIdAsync_ShouldDecrypt_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var entity = new BankAccountDetails { Id = accountId, AccountNumber = "enc-1234" };
        var dto = new BankAccountDetailsDto { Id = accountId };

        _bankAccountRepoMock
            .Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _cryptoMock
            .Setup(c => c.DecryptData("enc-1234"))
            .Returns("1234");

        _mapperMock
            .Setup(m => m.Map<BankAccountDetailsDto>(entity))
            .Returns(dto);

        // Act
        var result = await _sut.GetBankAccountByIdAsync(accountId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _cryptoMock.Verify(c => c.DecryptData("enc-1234"), Times.Once);
    }

    [Fact]
    public async Task GetBankAccountByIdAsync_ShouldNotDecrypt_WhenAccountDoesNotExist()
    {
        // Arrange
        _bankAccountRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BankAccountDetails)null!);

        _mapperMock
            .Setup(m => m.Map<BankAccountDetailsDto>(null))
            .Returns((BankAccountDetailsDto)null!);

        // Act
        var result = await _sut.GetBankAccountByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _cryptoMock.Verify(c => c.DecryptData(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region GetBankAccountsByFamilyMemberIdAsync

    [Fact]
    public async Task GetBankAccountsByFamilyMemberIdAsync_ShouldDecryptAccountNumbers()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var entities = new List<BankAccountDetails>
        {
            new() { Id = Guid.NewGuid(), AccountNumber = "enc-1" },
            new() { Id = Guid.NewGuid(), AccountNumber = "enc-2" }
        };

        _bankAccountRepoMock
            .Setup(r => r.GetAllByFamilyMemberIdAsync(familyMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        _cryptoMock
            .Setup(c => c.DecryptData(It.IsAny<string>()))
            .Returns<string>(s => $"dec-{s}");

        _mapperMock
            .Setup(m => m.Map<List<BankAccountDetailsDto>>(entities))
            .Returns(new List<BankAccountDetailsDto>());

        // Act
        var result = await _sut.GetBankAccountsByFamilyMemberIdAsync(familyMemberId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _cryptoMock.Verify(c => c.DecryptData("enc-1"), Times.Once);
        _cryptoMock.Verify(c => c.DecryptData("enc-2"), Times.Once);
    }

    #endregion

    #region CreateBankAccountAsync

    [Fact]
    public async Task CreateBankAccountAsync_ShouldEncryptAndPersistAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateBankAccountRequest
        {
            FamilyMemberId = Guid.NewGuid(),
            AccountNumber = "1234567890"
        };

        var entity = new BankAccountDetails { AccountNumber = request.AccountNumber };
        BankAccountDetails? capturedEntity = null;
        var saved = new BankAccountDetails { Id = Guid.NewGuid(), AccountNumber = "enc-1234567890" };
        var dto = new BankAccountDetailsDto { Id = saved.Id };

        _mapperMock
            .Setup(m => m.Map<BankAccountDetails>(request))
            .Returns(entity);

        _cryptoMock
            .Setup(c => c.EncryptData("1234567890"))
            .Returns("enc-1234567890");

        _bankAccountRepoMock
            .Setup(r => r.AddAsync(It.IsAny<BankAccountDetails>(), It.IsAny<CancellationToken>()))
            .Callback<BankAccountDetails, CancellationToken>((arg, _) => capturedEntity = arg)
            .ReturnsAsync(saved);

        _mapperMock
            .Setup(m => m.Map<BankAccountDetailsDto>(saved))
            .Returns(dto);

        // Act
        var result = await _sut.CreateBankAccountAsync(request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        capturedEntity.Should().NotBeNull();
        capturedEntity!.CreatedBy.Should().Be(userId.ToString());
        capturedEntity.AccountNumberLast4.Should().Be("7890");
        _cryptoMock.Verify(c => c.EncryptData("1234567890"), Times.Once);
        _bankAccountRepoMock.Verify(r => r.AddAsync(It.IsAny<BankAccountDetails>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBankAccountAsync_ShouldKeepLast4AsWhole_WhenAccountNumberLengthIsFourOrLess()
    {
        // Arrange
        var request = new CreateBankAccountRequest
        {
            FamilyMemberId = Guid.NewGuid(),
            AccountNumber = "1234"
        };

        var entity = new BankAccountDetails { AccountNumber = request.AccountNumber };
        BankAccountDetails? capturedEntity = null;

        _mapperMock
            .Setup(m => m.Map<BankAccountDetails>(request))
            .Returns(entity);

        _cryptoMock
            .Setup(c => c.EncryptData("1234"))
            .Returns("enc-1234");

        _bankAccountRepoMock
            .Setup(r => r.AddAsync(It.IsAny<BankAccountDetails>(), It.IsAny<CancellationToken>()))
            .Callback<BankAccountDetails, CancellationToken>((arg, _) => capturedEntity = arg)
            .ReturnsAsync(new BankAccountDetails { Id = Guid.NewGuid(), AccountNumber = "enc-1234" });

        _mapperMock
            .Setup(m => m.Map<BankAccountDetailsDto>(It.IsAny<BankAccountDetails>()))
            .Returns(new BankAccountDetailsDto { Id = Guid.NewGuid() });

        // Act
        await _sut.CreateBankAccountAsync(request, Guid.NewGuid(), CancellationToken.None);

        // Assert
        capturedEntity.Should().NotBeNull();
        capturedEntity!.AccountNumberLast4.Should().Be("1234");
    }

    #endregion

    #region UpdateBankAccountAsync

    [Fact]
    public async Task UpdateBankAccountAsync_ShouldEncryptAndPersistAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var request = new UpdateBankAccountRequest
        {
            Id = accountId,
            FamilyMemberId = Guid.NewGuid(),
            AccountNumber = "999988887777"
        };

        var entity = new BankAccountDetails { Id = accountId, AccountNumber = request.AccountNumber };
        BankAccountDetails? capturedEntity = null;
        var updated = new BankAccountDetails { Id = accountId, AccountNumber = "enc-999988887777" };

        _mapperMock
            .Setup(m => m.Map<BankAccountDetails>(request))
            .Returns(entity);

        _cryptoMock
            .Setup(c => c.EncryptData("999988887777"))
            .Returns("enc-999988887777");

        _bankAccountRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<BankAccountDetails>(), It.IsAny<CancellationToken>()))
            .Callback<BankAccountDetails, CancellationToken>((arg, _) => capturedEntity = arg)
            .ReturnsAsync(updated);

        _mapperMock
            .Setup(m => m.Map<BankAccountDetailsDto>(updated))
            .Returns(new BankAccountDetailsDto { Id = accountId });

        // Act
        var result = await _sut.UpdateBankAccountAsync(request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        capturedEntity.Should().NotBeNull();
        capturedEntity!.UpdatedBy.Should().Be(userId.ToString());
        capturedEntity.AccountNumberLast4.Should().Be("7777");
        _cryptoMock.Verify(c => c.EncryptData("999988887777"), Times.Once);
    }

    #endregion

    #region DeleteBankAccountByIdAsync

    [Fact]
    public async Task DeleteBankAccountByIdAsync_ShouldCallRepository()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        await _sut.DeleteBankAccountByIdAsync(accountId, userId, CancellationToken.None);

        // Assert
        _bankAccountRepoMock.Verify(
            r => r.DeleteByIdAsync(accountId, userId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}

