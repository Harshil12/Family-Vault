using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace FamilyVault.Tests.API.EndPoints.Family;

/// <summary>
/// Represents FamilymemberEventsTests.
/// </summary>
public class FamilymemberEventsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IFamilyService> _familyServiceMock = new();

    /// <summary>
    /// Initializes a new instance of FamilymemberEventsTests.
    /// </summary>
    public FamilymemberEventsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_familyServiceMock.Object);

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

    #region GET /family/{userId}

    [Fact]
    /// <summary>
    /// Performs the GetFamilies_ShouldReturnEmptyList_WhenNoFamilies operation.
    /// </summary>
    public async Task GetFamilies_ShouldReturnEmptyList_WhenNoFamilies()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _familyServiceMock
            .Setup(s => s.GetFamilyByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FamilyDto>());

        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/family/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /family/{userId}/{id}

    [Fact]
    /// <summary>
    /// Performs the GetFamilyById_ShouldReturnNotFound_WhenFamilyDoesNotExist operation.
    /// </summary>
    public async Task GetFamilyById_ShouldReturnNotFound_WhenFamilyDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        _familyServiceMock
            .Setup(s => s.GetFamilyByIdAsync(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyDto?)null);

        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/family/{userId}/{familyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /family/{userId}/family

    [Fact]
    /// <summary>
    /// Performs the CreateFamily_ShouldReturnCreated operation.
    /// </summary>
    public async Task CreateFamily_ShouldReturnCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = new CreateFamilyRequest
        {
            FamilyName = "Test Family"
        };

        var created = new FamilyDto
        {
            Id = Guid.NewGuid(),
            Name = request.FamilyName
        };

        _familyServiceMock
            .Setup(s => s.CreateFamilyAsync(
                It.IsAny<CreateFamilyRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var client = CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            $"/family/{userId}/family", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region PUT /family/{userId}/family/{id}

    [Fact]
    /// <summary>
    /// Performs the UpdateFamily_ShouldReturnOk operation.
    /// </summary>
    public async Task UpdateFamily_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        var request = new UpdateFamlyRequest
        {
            Id = familyId,
            FamilyName = "Updated Family"
        };

        var updated = new FamilyDto
        {
            Id = familyId,
            Name = request.FamilyName
        };

        _familyServiceMock
            .Setup(s => s.UpdateFamilyAsync(
                It.IsAny<UpdateFamlyRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var client = CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/family/{userId}/family/{familyId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region DELETE /family/{userId}/{id}

    [Fact]
    /// <summary>
    /// Performs the DeleteFamily_ShouldReturnOk operation.
    /// </summary>
    public async Task DeleteFamily_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        _familyServiceMock
            .Setup(s => s.DeleteFamilyByIdAsync(
                familyId,
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var client = CreateClient();

        // Act
        var response = await client.DeleteAsync(
            $"/family/{userId}/{familyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
