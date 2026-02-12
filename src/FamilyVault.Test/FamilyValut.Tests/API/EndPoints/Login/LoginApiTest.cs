using FamilyVault.API;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FamilyVault.Tests.API.EndPoints.Login;

public class LoginApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();

    public LoginApiTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_authServiceMock.Object);
                services.AddSingleton(_userServiceMock.Object);
            });
        });
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.GetTokenAsync("test@test.com", "wrong-password", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var client = CreateClient();
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "wrong-password"
        };

        // Act
        var response = await client.PostAsJsonAsync("/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnOkWithToken_WhenCredentialsAreValid()
    {
        // Arrange
        const string token = "mock-jwt-token";
        _authServiceMock
            .Setup(s => s.GetTokenAsync("test@test.com", "Password@123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var client = CreateClient();
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "Password@123"
        };

        // Act
        var response = await client.PostAsJsonAsync("/login", request);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        json.TryGetProperty("token", out var tokenElement).Should().BeTrue();
        tokenElement.GetString().Should().Be(token);
    }

    [Fact]
    public async Task Register_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Password = "Password@123",
            Email = "newuser@test.com",
            FirstName = "New",
            LastName = "User",
            CountryCode = "+91",
            Mobile = "9876543210"
        };

        _userServiceMock
            .Setup(s => s.RegisterUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDto
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            });

        var client = CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}


