using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace FamilyVault.Tests.API.EndPoints.Document;

/// <summary>
/// Represents DocumentApiTests.
/// </summary>
public class DocumentApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IDocumentService> _documentServiceMock = new();

    /// <summary>
    /// Initializes a new instance of DocumentApiTests.
    /// </summary>
    public DocumentApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_documentServiceMock.Object);

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test", options => { });
            });
        });
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Test");
        return client;
    }

    #region GET /documents/{familyMemberId}

    [Fact]
    /// <summary>
    /// Performs the GetDocuments_ShouldReturnEmptyList_WhenNoDocuments operation.
    /// </summary>
    public async Task GetDocuments_ShouldReturnEmptyList_WhenNoDocuments()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();

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
    /// <summary>
    /// Performs the GetDocumentById_ShouldReturnNotFound_WhenDocumentDoesNotExist operation.
    /// </summary>
    public async Task GetDocumentById_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();

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
    /// <summary>
    /// Performs the CreateDocument_ShouldReturnCreated operation.
    /// </summary>
    public async Task CreateDocument_ShouldReturnCreated()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();

        var request = new CreateDocumentRequest
        {
            FamilyMemberId = familyMemberId,
            DocumentNumber = "1234"
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
    /// <summary>
    /// Performs the UpdateDocument_ShouldReturnOk operation.
    /// </summary>
    public async Task UpdateDocument_ShouldReturnOk()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        var request = new UpdateDocumentRequest
        {
            Id = docId,
            DocumentNumber = "updated"
        };

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
    /// <summary>
    /// Performs the DeleteDocument_ShouldReturnOk operation.
    /// </summary>
    public async Task DeleteDocument_ShouldReturnOk()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();

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
