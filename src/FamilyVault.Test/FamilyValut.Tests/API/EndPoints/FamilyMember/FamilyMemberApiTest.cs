using FamilyVault.API;
using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace FamilyVault.Tests.API.EndPoints.FamilyMember;

public class FamilyMemberApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IFamilyMemberService> _familyMemberServiceMock = new();
    private readonly Mock<IFamilyService> _familyServiceMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();

    public FamilyMemberApiTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_familyMemberServiceMock.Object);
                services.AddSingleton(_familyServiceMock.Object);
                services.AddSingleton(_auditServiceMock.Object);

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test", _ => { });
            });
        });

        _auditServiceMock
            .Setup(s => s.LogAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Test");
        return client;
    }

    private void SetupFamilyOwnership(Guid familyId)
    {
        _familyServiceMock
            .Setup(s => s.GetFamilyByIdAsync(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FamilyDto
            {
                Id = familyId,
                Name = "Test Family",
                UserId = TestAuthHandler.TestUserId
            });
    }

    #region GET /familymember/{familyId}

    [Fact]
    public async Task GetFamilyMembers_ShouldReturnEmptyList_WhenNoneExist()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        SetupFamilyOwnership(familyId);

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
    public async Task GetFamilyMemberById_ShouldReturnNotFound_WhenMemberDoesNotExist()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        SetupFamilyOwnership(familyId);

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
    public async Task CreateFamilyMember_ShouldReturnCreated()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        SetupFamilyOwnership(familyId);

        var request = new CreateFamilyMemberRequest
        {
            FamilyId = familyId,
            FirstName = "John",
            LastName = "Doe",
            RelationshipType = Relationships.Spouse,
            CountryCode = "+91",
            Mobile = "9876543210"
        };

        var created = new FamilyMemberDto
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            FirstName = request.FirstName
        };

        _familyMemberServiceMock
            .Setup(s => s.CreateFamilyMemberAsync(
                It.IsAny<CreateFamilyMemberRequest>(),
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
    public async Task UpdateFamilyMember_ShouldReturnOk()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        SetupFamilyOwnership(familyId);

        var request = new UpdateFamilyMemberRequest
        {
            Id = memberId,
            FamilyId = familyId,
            FirstName = "Updated",
            RelationshipType = Relationships.Spouse,
            CountryCode = "+91",
            Mobile = "9876543210"
        };

        _familyMemberServiceMock
            .Setup(s => s.GetFamilyMemberByIdAsync(memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FamilyMemberDto
            {
                Id = memberId,
                FamilyId = familyId,
                FirstName = "Existing"
            });

        var updated = new FamilyMemberDto
        {
            Id = memberId,
            FamilyId = familyId,
            FirstName = request.FirstName
        };

        _familyMemberServiceMock
            .Setup(s => s.UpdateFamilyMemberAsync(
                It.IsAny<UpdateFamilyMemberRequest>(),
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
    public async Task DeleteFamilyMember_ShouldReturnOk()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        SetupFamilyOwnership(familyId);

        _familyMemberServiceMock
            .Setup(s => s.GetFamilyMemberByIdAsync(memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FamilyMemberDto
            {
                Id = memberId,
                FamilyId = familyId,
                FirstName = "Existing"
            });

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


