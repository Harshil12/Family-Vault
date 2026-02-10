using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace FamilyVault.Tests.API.EndPoints.User;

/// <summary>
/// Represents UserEventsTests.
/// </summary>
public class UserEventsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IUserService> _userServiceMock = new();

    /// <summary>
    /// Initializes a new instance of UserEventsTests.
    /// </summary>
    public UserEventsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_userServiceMock.Object);

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test", _ => { });
            });
        });
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Test");
        return client;
    }

    #region GET /User

    [Fact]
    /// <summary>
    /// Performs the GetUsers_ShouldReturnEmptyList_WhenNoUsers operation.
    /// </summary>
    public async Task GetUsers_ShouldReturnEmptyList_WhenNoUsers()
    {
        // Arrange
        _userServiceMock
            .Setup(s => s.GetUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<UserDto>());

        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/User");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /User/{id}

    [Fact]
    /// <summary>
    /// Performs the GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist operation.
    /// </summary>
    public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userServiceMock
            .Setup(s => s.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/User/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /User/user

    [Fact]
    /// <summary>
    /// Performs the CreateUser_ShouldReturnCreated operation.
    /// </summary>
    public async Task CreateUser_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Password = "Password@123",
            Email   = "Iz9Ou@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var createdUser = new UserDto
        {
            Id = Guid.NewGuid(),
            Username = request.Username
        };

        _userServiceMock
            .Setup(s => s.CreateUserAsync(
                It.IsAny<CreateUserRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        var client = CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/User/user", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region PUT /User/user/{id}

    [Fact]
    /// <summary>
    /// Performs the UpdateUser_ShouldReturnOk operation.
    /// </summary>
    public async Task UpdateUser_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = new UpdateUserRequest
        {
            Id = userId,
            FirstName = "updatedUser",
            Email = "Iz9Ou@example.com",
            Password = "NewPassword@123"
        };

        var updatedUser = new UserDto
        {
            Id = userId,
            FirstName = request.FirstName
        };

        _userServiceMock
            .Setup(s => s.UpdateuUerAsync(
                It.IsAny<UpdateUserRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        var client = CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/User/user/{userId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region DELETE /User/{id}

    [Fact]
    /// <summary>
    /// Performs the DeleteUser_ShouldReturnOk operation.
    /// </summary>
    public async Task DeleteUser_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userServiceMock
            .Setup(s => s.DeleteUserByIdAsync(
                userId,
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var client = CreateClient();

        // Act
        var response = await client.DeleteAsync($"/User/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
