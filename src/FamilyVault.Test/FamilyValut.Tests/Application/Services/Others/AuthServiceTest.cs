using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Application.Services;
using FamilyVault.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FamilyVault.Tests.Services;

/// <summary>
/// Represents AuthServiceTests.
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ICryptoService> _cryptoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IConfiguration _configuration;

    private readonly AuthService _sut;

    /// <summary>
    /// Initializes a new instance of AuthServiceTests.
    /// </summary>
    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _cryptoMock = new Mock<ICryptoService>();
        _mapperMock = new Mock<IMapper>();

        var jwtSettings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "THIS_IS_A_TEST_SECRET_KEY_123456789",
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(jwtSettings)
            .Build();

        _sut = new AuthService(
            _configuration,
            _userRepoMock.Object,
            _mapperMock.Object,
            _cryptoMock.Object);
    }

    #region GetTokenAsync Tests

    /// <summary>
    /// Performs the GetTokenAsync_ShouldReturnNull_WhenUserDoesNotExist operation.
    /// </summary>
    [Fact]
    public async Task GetTokenAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _sut.GetTokenAsync("test@test.com", "password", CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _cryptoMock.Verify(c => c.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Performs the GetTokenAsync_ShouldReturnNull_WhenPasswordIsInvalid operation.
    /// </summary>
    [Fact]
    public async Task GetTokenAsync_ShouldReturnNull_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            Password = "hashed-password"
        };

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cryptoMock
            .Setup(c => c.VerifyPassword(user.Password, "wrong-password"))
            .Returns(false);

        // Act
        var result = await _sut.GetTokenAsync(user.Email, "wrong-password", CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _mapperMock.Verify(m => m.Map<UserDto>(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Performs the GetTokenAsync_ShouldReturnToken_WhenCredentialsAreValid operation.
    /// </summary>
    [Fact]
    public async Task GetTokenAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@doe.com",
            Password = "hashed-password"
        };

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _cryptoMock
            .Setup(c => c.VerifyPassword(user.Password, "correct-password"))
            .Returns(true);

        _mapperMock
            .Setup(m => m.Map<UserDto>(user))
            .Returns(userDto);

        // Act
        var token = await _sut.GetTokenAsync(user.Email, "correct-password", CancellationToken.None);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region GenerateToken Tests

    /// <summary>
    /// Performs the GenerateToken_ShouldCreateValidJwt_WithExpectedClaims operation.
    /// </summary>
    [Fact]
    public void GenerateToken_ShouldCreateValidJwt_WithExpectedClaims()
    {
        // Arrange
        var user = new UserDto
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@smith.com"
        };

        // Act
        var tokenString = _sut.GenerateToken(user);

        // Assert
        tokenString.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Email && c.Value == user.Email);

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    #endregion
}
