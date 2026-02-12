using FamilyVault.API;
using FamilyVault.Application.DTOs.Documents;
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

namespace FamilyVault.Tests.API.EndPoints.Document;

public class DocumentApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IDocumentService> _documentServiceMock = new();
    private readonly Mock<IFamilyMemberService> _familyMemberServiceMock = new();
    private readonly Mock<IFamilyService> _familyServiceMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();

    public DocumentApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_documentServiceMock.Object);
                services.AddSingleton(_familyMemberServiceMock.Object);
                services.AddSingleton(_familyServiceMock.Object);
                services.AddSingleton(_auditServiceMock.Object);

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test", options => { });
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

    private void SetupOwnership(Guid familyMemberId)
    {
        var familyId = Guid.NewGuid();

        _familyMemberServiceMock
            .Setup(s => s.GetFamilyMemberByIdAsync(familyMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FamilyMemberDto
            {
                Id = familyMemberId,
                FamilyId = familyId,
                FirstName = "John"
            });

        _familyServiceMock
            .Setup(s => s.GetFamilyByIdAsync(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FamilyDto
            {
                Id = familyId,
                Name = "Test Family",
                UserId = TestAuthHandler.TestUserId
            });
    }

    #region GET /documents/{familyMemberId}

    [Fact]
    public async Task GetDocuments_ShouldReturnEmptyList_WhenNoDocuments()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        _documentServiceMock
            .Setup(s => s.GetDocumentsDetailsByFamilyMemberIdAsync(
                familyMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DocumentDetailsDto>());

        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/documents/{familyMemberId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /documents/{familyMemberId}/{id}

    [Fact]
    public async Task GetDocumentById_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        _documentServiceMock
            .Setup(s => s.GetDocumentDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentDetailsDto?)null);

        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/documents/{familyMemberId}/{docId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /documents

    [Fact]
    public async Task CreateDocument_ShouldReturnCreated()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        var request = new CreateDocumentRequest
        {
            FamilyMemberId = familyMemberId,
            DocumentNumber = "P1234567",
            DocumentType = DocumentTypes.Passport
        };

        var created = new DocumentDetailsDto
        {
            Id = Guid.NewGuid()
        };

        _documentServiceMock
            .Setup(s => s.CreateDocumentDetailsAsync(
                It.IsAny<CreateDocumentRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var client = CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            $"/documents/{familyMemberId}/documents", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region PUT /documents/{id}

    [Fact]
    public async Task UpdateDocument_ShouldReturnOk()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        var request = new UpdateDocumentRequest
        {
            Id = docId,
            FamilyMemberId = familyMemberId,
            DocumentNumber = "P1234567",
            DocumentType = DocumentTypes.Passport
        };

        _documentServiceMock
            .Setup(s => s.GetDocumentDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDetailsDto
            {
                Id = docId,
                FamilyMemberId = familyMemberId,
                DocumentNumber = "P1234567",
                DocumentType = DocumentTypes.Passport
            });

        var updated = new DocumentDetailsDto { Id = docId };

        _documentServiceMock
            .Setup(s => s.UpdateDocumentDetailsAsync(
                It.IsAny<UpdateDocumentRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var client = CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/documents/{familyMemberId}/documents/{docId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region DELETE /documents/{id}

    [Fact]
    public async Task DeleteDocument_ShouldReturnOk()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        _documentServiceMock
            .Setup(s => s.GetDocumentDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDetailsDto
            {
                Id = docId,
                FamilyMemberId = familyMemberId,
                DocumentNumber = "P1234567",
                DocumentType = DocumentTypes.Passport
            });

        _documentServiceMock
            .Setup(s => s.DeleteDocumentDetailsByIdAsync(
                docId,
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var client = CreateClient();

        // Act
        var response = await client.DeleteAsync(
            $"/documents/{familyMemberId}/{docId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}

