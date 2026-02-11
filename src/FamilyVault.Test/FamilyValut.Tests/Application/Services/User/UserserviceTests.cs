using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Application.Services;
using FamilyVault.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FamilyVault.Tests.Services;

/// <summary>
/// Represents UserserviceTests.
/// </summary>
public class UserserviceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ICryptoService> _cryptoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<Userservice>> _loggerMock;

    private readonly Userservice _sut;

    /// <summary>
    /// Initializes a new instance of UserserviceTests.
    /// </summary>
    public UserserviceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _cryptoMock = new Mock<ICryptoService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<Userservice>>();

        _sut = new Userservice(
            _userRepoMock.Object,
            _cryptoMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    #region GetUserAsync

    [Fact]
    /// <summary>
    /// Performs the GetUserAsync_ShouldReturnMappedUsers operation.
    /// </summary>
    public async Task GetUserAsync_ShouldReturnMappedUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Username = "user1" },
            new() { Id = Guid.NewGuid(), Username = "user2" }
        };

        var userDtos = new List<UserDto>
        {
            new() { Id = users[0].Id },
            new() { Id = users[1].Id }
        };

        _userRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<UserDto>>(users))
            .Returns(userDtos);

        // Act
        var result = await _sut.GetUserAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetUserByIdAsync

    [Fact]
    /// <summary>
    /// Performs the GetUserByIdAsync_ShouldReturnMappedUser operation.
    /// </summary>
    public async Task GetUserByIdAsync_ShouldReturnMappedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var user = new User { Id = userId, Username = "test" };
        var userDto = new UserDto { Id = userId };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mapperMock
            .Setup(m => m.Map<UserDto>(user))
            .Returns(userDto);

        // Act
        var result = await _sut.GetUserByIdAsync(userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
    }

    #endregion

    #region CreateUserAsync

    [Fact]
    /// <summary>
    /// Performs the CreateUserAsync_ShouldHashPassword_AndPersistUser operation.
    /// </summary>
    public async Task CreateUserAsync_ShouldHashPassword_AndPersistUser()
    {
        // Arrange
        var creatorId = Guid.NewGuid();

        var request = new CreateUserRequest
        {
            Username = "newuser",
            Password = "plain-password"
        };

        var userEntity = new User
        {
            Username = request.Username,
            Password = request.Password
        };

        var savedUser = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Password = "hashed-password"
        };

        var userDto = new UserDto { Id = savedUser.Id };

        _mapperMock
            .Setup(m => m.Map<User>(request))
            .Returns(userEntity);

        _cryptoMock
            .Setup(c => c.HashPassword("plain-password"))
            .Returns("hashed-password");

        _userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedUser);

        _mapperMock
            .Setup(m => m.Map<UserDto>(savedUser))
            .Returns(userDto);

        // Act
        var result = await _sut.CreateUserAsync(request, creatorId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _cryptoMock.Verify(c => c.HashPassword("plain-password"), Times.Once);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region RegisterUserAsync

    [Fact]
    /// <summary>
    /// Performs the RegisterUserAsync_ShouldHashPassword_AndSetSelfRegisterCreator operation.
    /// </summary>
    public async Task RegisterUserAsync_ShouldHashPassword_AndSetSelfRegisterCreator()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "selfuser",
            Password = "plain-password"
        };

        var userEntity = new User
        {
            Username = request.Username,
            Password = request.Password
        };

        User? capturedUser = null;

        var savedUser = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Password = "hashed-password"
        };

        var userDto = new UserDto { Id = savedUser.Id };

        _mapperMock
            .Setup(m => m.Map<User>(request))
            .Returns(userEntity);

        _cryptoMock
            .Setup(c => c.HashPassword("plain-password"))
            .Returns("hashed-password");

        _userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .ReturnsAsync(savedUser);

        _mapperMock
            .Setup(m => m.Map<UserDto>(savedUser))
            .Returns(userDto);

        // Act
        var result = await _sut.RegisterUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        capturedUser.Should().NotBeNull();
        capturedUser!.CreatedBy.Should().Be("SELF_REGISTER");
        _cryptoMock.Verify(c => c.HashPassword("plain-password"), Times.Once);
    }

    #endregion

    #region DeleteUserByIdAsync

    [Fact]
    /// <summary>
    /// Performs the DeleteUserByIdAsync_ShouldCallRepository operation.
    /// </summary>
    public async Task DeleteUserByIdAsync_ShouldCallRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        // Act
        await _sut.DeleteUserByIdAsync(userId, creatorId, CancellationToken.None);

        // Assert
        _userRepoMock.Verify(
            r => r.DeleteByIdAsync(userId, creatorId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateUserAsync

    [Fact]
    /// <summary>
    /// Performs the UpdateUserAsync_ShouldHashPassword_AndUpdateUser operation.
    /// </summary>
    public async Task UpdateUserAsync_ShouldHashPassword_AndUpdateUser()
    {
        // Arrange
        var updaterId = Guid.NewGuid();

        var request = new UpdateUserRequest
        {
            Id = Guid.NewGuid(),
            Password = "new-password"
        };

        var userEntity = new User
        {
            Id = request.Id,
            Password = request.Password
        };

        var updatedUser = new User
        {
            Id = request.Id,
            Password = "hashed-new-password"
        };

        var userDto = new UserDto { Id = request.Id };

        _mapperMock
            .Setup(m => m.Map<User>(request))
            .Returns(userEntity);

        _cryptoMock
            .Setup(c => c.HashPassword("new-password"))
            .Returns("hashed-new-password");

        _userRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        _mapperMock
            .Setup(m => m.Map<UserDto>(updatedUser))
            .Returns(userDto);

        // Act
        var result = await _sut.UpdateUserAsync(request, updaterId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _cryptoMock.Verify(c => c.HashPassword("new-password"), Times.Once);
    }

    #endregion
}
