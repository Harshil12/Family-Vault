using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace FamilyVault.Tests.API.EndPoints.FamilyMember;

/// <summary>
/// Represents FamilyMemberApiTest.
/// </summary>
public class FamilyMemberApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IFamilymemeberService> _familyMemberServiceMock = new();

    /// <summary>
    /// Initializes a new instance of FamilyMemberApiTest.
    /// </summary>
    public FamilyMemberApiTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_familyMemberServiceMock.Object);

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

    #region GET /familymember/{familyId}

    [Fact]
    /// <summary>
    /// Performs the GetFamilyMembers_ShouldReturnEmptyList_WhenNoneExist operation.
    /// </summary>
    public async Task GetFamilyMembers_ShouldReturnEmptyList_WhenNoneExist()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        _familyMemberServiceMock
            .Setup(s => s.GetFamilyMembersByFamilyIdAsync(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FamilyMemberDto>());

        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/familymember/{familyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /familymember/{familyId}/{id}

    [Fact]
    /// <summary>
    /// Performs the GetFamilyMemberById_ShouldReturnNotFound_WhenMemberDoesNotExist operation.
    /// </summary>
    public async Task GetFamilyMemberById_ShouldReturnNotFound_WhenMemberDoesNotExist()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        _familyMemberServiceMock
            .Setup(s => s.GetFamilyMemberByIdAsync(memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyMemberDto?)null);

        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/familymember/{familyId}/{memberId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /familymember/{familyId}/familymember

    [Fact]
    /// <summary>
    /// Performs the CreateFamilyMember_ShouldReturnCreated operation.
    /// </summary>
    public async Task CreateFamilyMember_ShouldReturnCreated()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        var request = new CreateFamilyMememberRequest
        {
            FamilyId = familyId,
            FirstName = "John",
            LastName = "Doe"
        };

        var created = new FamilyMemberDto
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName
        };

        _familyMemberServiceMock
            .Setup(s => s.CreateFamilyMemberAsync(
                It.IsAny<CreateFamilyMememberRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var client = CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            $"/familymember/{familyId}/familymember", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region PUT /familymember/{familyId}/familymember/{id}

    [Fact]
    /// <summary>
    /// Performs the UpdateFamilyMember_ShouldReturnOk operation.
    /// </summary>
    public async Task UpdateFamilyMember_ShouldReturnOk()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var request = new UpdateFamilyMememberRequest
        {
            Id = memberId,
            FirstName = "Updated"
        };

        var updated = new FamilyMemberDto
        {
            Id = memberId,
            FirstName = request.FirstName
        };

        _familyMemberServiceMock
            .Setup(s => s.UpdateFamilyMemberAsync(
                It.IsAny<UpdateFamilyMememberRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var client = CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/familymember/{familyId}/familymember/{memberId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region DELETE /familymember/{familyId}/{id}

    [Fact]
    /// <summary>
    /// Performs the DeleteFamilyMember_ShouldReturnOk operation.
    /// </summary>
    public async Task DeleteFamilyMember_ShouldReturnOk()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        _familyMemberServiceMock
            .Setup(s => s.DeleteFamilyMemberByIdAsync(
                memberId,
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var client = CreateClient();

        // Act
        var response = await client.DeleteAsync(
            $"/familymember/{familyId}/{memberId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
