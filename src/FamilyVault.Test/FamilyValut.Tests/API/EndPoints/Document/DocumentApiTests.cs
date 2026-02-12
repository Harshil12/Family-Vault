using FamilyVault.API;
using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
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

    private void SetupOwnership(Guid familyMemberId)
    {
        SetupOwnership(familyMemberId, Guid.NewGuid());
    }

    private void SetupOwnership(Guid familyMemberId, Guid familyId)
    {
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

    private void SetupDifferentOwner(Guid familyMemberId)
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
                Name = "Other Family",
                UserId = Guid.NewGuid()
            });
    }

    #region GET /documents/{familyMemberId}

    [Fact]
    public async Task GetDocuments_ShouldReturnEmptyList_WhenNoDocuments()
    {
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        _documentServiceMock
            .Setup(s => s.GetDocumentsDetailsByFamilyMemberIdAsync(
                familyMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DocumentDetailsDto>());

        var client = CreateClient();
        var response = await client.GetAsync($"/documents/{familyMemberId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDocuments_ShouldReturnForbidden_WhenUserDoesNotOwnFamilyMember()
    {
        var familyMemberId = Guid.NewGuid();
        SetupDifferentOwner(familyMemberId);
        var client = CreateClient();

        var response = await client.GetAsync($"/documents/{familyMemberId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDocuments_ShouldReturnInternalServerError_WhenServiceThrows()
    {
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);
        _documentServiceMock
            .Setup(s => s.GetDocumentsDetailsByFamilyMemberIdAsync(familyMemberId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("unexpected"));
        var client = CreateClient();

        var response = await client.GetAsync($"/documents/{familyMemberId}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region GET /documents/{familyMemberId}/{id}

    [Fact]
    public async Task GetDocumentById_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        _documentServiceMock
            .Setup(s => s.GetDocumentDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentDetailsDto?)null);

        var client = CreateClient();
        var response = await client.GetAsync($"/documents/{familyMemberId}/{docId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDocumentById_ShouldReturnNotFound_WhenDocumentBelongsToDifferentFamilyMember()
    {
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        _documentServiceMock
            .Setup(s => s.GetDocumentDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDetailsDto
            {
                Id = docId,
                FamilyMemberId = Guid.NewGuid(),
                DocumentNumber = "P1234567",
                DocumentType = DocumentTypes.Passport
            });

        var client = CreateClient();
        var response = await client.GetAsync($"/documents/{familyMemberId}/{docId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDocumentById_ShouldReturnForbidden_WhenUserDoesNotOwnFamilyMember()
    {
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        SetupDifferentOwner(familyMemberId);
        var client = CreateClient();

        var response = await client.GetAsync($"/documents/{familyMemberId}/{docId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region GET /documents/{familyMemberId}/{id}/file

    [Fact]
    public async Task GetDocumentFile_ShouldReturnForbidden_WhenUserDoesNotOwnFamilyMember()
    {
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        SetupDifferentOwner(familyMemberId);
        var client = CreateClient();

        var response = await client.GetAsync($"/documents/{familyMemberId}/{docId}/file");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDocumentFile_ShouldReturnNotFound_WhenSavedLocationIsMissing()
    {
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
                DocumentType = DocumentTypes.Passport,
                SavedLocation = null
            });

        var client = CreateClient();
        var response = await client.GetAsync($"/documents/{familyMemberId}/{docId}/file");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDocumentFile_ShouldReturnNotFound_WhenFilePathEscapesUploadsRoot()
    {
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
                DocumentType = DocumentTypes.Passport,
                SavedLocation = "../outside.pdf"
            });

        var client = CreateClient();
        var response = await client.GetAsync($"/documents/{familyMemberId}/{docId}/file");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDocumentFile_ShouldReturnFile_WhenFileExists()
    {
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        var hostEnvironment = _factory.Services.GetRequiredService<IWebHostEnvironment>();
        var relativePath = Path.Combine("uploads", "test-user", $"{Guid.NewGuid():N}.pdf");
        var fullPath = Path.Combine(hostEnvironment.ContentRootPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, new byte[] { 1, 2, 3, 4, 5 });

        _documentServiceMock
            .Setup(s => s.GetDocumentDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDetailsDto
            {
                Id = docId,
                FamilyMemberId = familyMemberId,
                DocumentNumber = "P1234567",
                DocumentType = DocumentTypes.Passport,
                SavedLocation = relativePath.Replace("\\", "/")
            });

        var client = CreateClient();
        try
        {
            var response = await client.GetAsync($"/documents/{familyMemberId}/{docId}/file");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
            var bytes = await response.Content.ReadAsByteArrayAsync();
            bytes.Should().HaveCount(5);
        }
        finally
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }

    [Fact]
    public async Task GetDocumentFile_ShouldLogAudit_WhenDownloadIsTrue()
    {
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        var hostEnvironment = _factory.Services.GetRequiredService<IWebHostEnvironment>();
        var relativePath = Path.Combine("uploads", "test-user", $"{Guid.NewGuid():N}.pdf");
        var fullPath = Path.Combine(hostEnvironment.ContentRootPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, new byte[] { 1, 2, 3 });

        _documentServiceMock
            .Setup(s => s.GetDocumentDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDetailsDto
            {
                Id = docId,
                FamilyMemberId = familyMemberId,
                DocumentNumber = "P1234567",
                DocumentType = DocumentTypes.Passport,
                SavedLocation = relativePath.Replace("\\", "/")
            });

        var client = CreateClient();
        try
        {
            var response = await client.GetAsync($"/documents/{familyMemberId}/{docId}/file?download=true");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _auditServiceMock.Verify(s => s.LogAsync(
                TestAuthHandler.TestUserId,
                "Download",
                "Document",
                docId,
                It.IsAny<string>(),
                null,
                familyMemberId,
                docId,
                It.IsAny<string?>(),
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }

    #endregion

    #region POST /documents

    [Fact]
    public async Task CreateDocument_ShouldReturnCreated()
    {
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        var request = new CreateDocumentRequest
        {
            FamilyMemberId = familyMemberId,
            DocumentNumber = "P1234567",
            DocumentType = DocumentTypes.Passport
        };

        _documentServiceMock
            .Setup(s => s.CreateDocumentDetailsAsync(
                It.IsAny<CreateDocumentRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDetailsDto { Id = Guid.NewGuid() });

        var client = CreateClient();
        var response = await client.PostAsJsonAsync($"/documents/{familyMemberId}/documents", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateDocument_ShouldReturnBadRequest_WhenValidationExceptionIsThrown()
    {
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        var request = new CreateDocumentRequest
        {
            FamilyMemberId = familyMemberId,
            DocumentNumber = "P1234567",
            DocumentType = DocumentTypes.Passport
        };

        _documentServiceMock
            .Setup(s => s.CreateDocumentDetailsAsync(
                It.IsAny<CreateDocumentRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new[]
            {
                new ValidationFailure("DocumentNumber", "Invalid document number.")
            }));

        var client = CreateClient();
        var response = await client.PostAsJsonAsync($"/documents/{familyMemberId}/documents", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDocument_ShouldReturnForbidden_WhenUserDoesNotOwnFamilyMember()
    {
        var familyMemberId = Guid.NewGuid();
        SetupDifferentOwner(familyMemberId);

        var request = new CreateDocumentRequest
        {
            FamilyMemberId = familyMemberId,
            DocumentNumber = "P1234567",
            DocumentType = DocumentTypes.Passport
        };

        var client = CreateClient();
        var response = await client.PostAsJsonAsync($"/documents/{familyMemberId}/documents", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region PUT /documents/{id}

    [Fact]
    public async Task UpdateDocument_ShouldReturnOk()
    {
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

        _documentServiceMock
            .Setup(s => s.UpdateDocumentDetailsAsync(
                It.IsAny<UpdateDocumentRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDetailsDto { Id = docId });

        var client = CreateClient();
        var response = await client.PutAsJsonAsync($"/documents/{familyMemberId}/documents/{docId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateDocument_ShouldReturnForbidden_WhenDocumentDoesNotBelongToFamilyMember()
    {
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
                FamilyMemberId = Guid.NewGuid(),
                DocumentNumber = "P1234567",
                DocumentType = DocumentTypes.Passport
            });

        var client = CreateClient();
        var response = await client.PutAsJsonAsync($"/documents/{familyMemberId}/documents/{docId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region DELETE /documents/{id}

    [Fact]
    public async Task DeleteDocument_ShouldReturnOk()
    {
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
        var response = await client.DeleteAsync($"/documents/{familyMemberId}/{docId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteDocument_ShouldReturnForbidden_WhenDocumentDoesNotBelongToFamilyMember()
    {
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        _documentServiceMock
            .Setup(s => s.GetDocumentDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDetailsDto
            {
                Id = docId,
                FamilyMemberId = Guid.NewGuid(),
                DocumentNumber = "P1234567",
                DocumentType = DocumentTypes.Passport
            });

        var client = CreateClient();
        var response = await client.DeleteAsync($"/documents/{familyMemberId}/{docId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region POST /documents/upload

    [Fact]
    public async Task UploadDocument_ShouldReturnBadRequest_WhenFileIsMissing()
    {
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);
        var content = new MultipartFormDataContent
        {
            { new StringContent(((int)DocumentTypes.Passport).ToString()), "DocumentType" },
            { new StringContent("P1234567"), "DocumentNumber" }
        };
        var client = CreateClient();

        var response = await client.PostAsync($"/documents/{familyMemberId}/documents/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadDocument_ShouldReturnBadRequest_WhenFileTypeIsNotAllowed()
    {
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        content.Add(fileContent, "File", "blocked.txt");
        content.Add(new StringContent(((int)DocumentTypes.Passport).ToString()), "DocumentType");
        content.Add(new StringContent("P1234567"), "DocumentNumber");
        var client = CreateClient();

        var response = await client.PostAsync($"/documents/{familyMemberId}/documents/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadDocument_ShouldReturnBadRequest_WhenFileIsTooLarge()
    {
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[(10 * 1024 * 1024) + 1]);
        content.Add(fileContent, "File", "large.pdf");
        content.Add(new StringContent(((int)DocumentTypes.Passport).ToString()), "DocumentType");
        content.Add(new StringContent("P1234567"), "DocumentNumber");

        var client = CreateClient();

        var response = await client.PostAsync($"/documents/{familyMemberId}/documents/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadDocument_ShouldReturnNotFound_WhenFamilyMemberIsMissing_AfterOwnershipCheck()
    {
        var familyMemberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        SetupOwnership(familyMemberId, familyId);

        _familyMemberServiceMock
            .SetupSequence(s => s.GetFamilyMemberByIdAsync(familyMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FamilyMemberDto
            {
                Id = familyMemberId,
                FamilyId = familyId,
                FirstName = "John"
            })
            .ReturnsAsync((FamilyMemberDto?)null);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        content.Add(fileContent, "File", "ok.pdf");
        content.Add(new StringContent(((int)DocumentTypes.Passport).ToString()), "DocumentType");
        content.Add(new StringContent("P1234567"), "DocumentNumber");
        var client = CreateClient();

        var response = await client.PostAsync($"/documents/{familyMemberId}/documents/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadDocument_ShouldReturnNotFound_WhenFamilyIsMissing_AfterOwnershipCheck()
    {
        var familyMemberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        SetupOwnership(familyMemberId, familyId);

        _familyServiceMock
            .SetupSequence(s => s.GetFamilyByIdAsync(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FamilyDto
            {
                Id = familyId,
                Name = "Test Family",
                UserId = TestAuthHandler.TestUserId
            })
            .ReturnsAsync((FamilyDto?)null);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        content.Add(fileContent, "File", "ok.pdf");
        content.Add(new StringContent(((int)DocumentTypes.Passport).ToString()), "DocumentType");
        content.Add(new StringContent("P1234567"), "DocumentNumber");
        var client = CreateClient();

        var response = await client.PostAsync($"/documents/{familyMemberId}/documents/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region PUT /documents/{id}/file

    [Fact]
    public async Task ReplaceDocumentFile_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        SetupOwnership(familyMemberId);
        _documentServiceMock
            .Setup(s => s.GetDocumentDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentDetailsDto?)null);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        content.Add(fileContent, "File", "replace.pdf");
        content.Add(new StringContent(((int)DocumentTypes.Passport).ToString()), "DocumentType");
        content.Add(new StringContent("P1234567"), "DocumentNumber");

        var client = CreateClient();
        var response = await client.PutAsync($"/documents/{familyMemberId}/documents/{docId}/file", content);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReplaceDocumentFile_ShouldReturnBadRequest_WhenFileIsMissing()
    {
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        var content = new MultipartFormDataContent
        {
            { new StringContent(((int)DocumentTypes.Passport).ToString()), "DocumentType" },
            { new StringContent("P1234567"), "DocumentNumber" }
        };

        var client = CreateClient();
        var response = await client.PutAsync($"/documents/{familyMemberId}/documents/{docId}/file", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReplaceDocumentFile_ShouldReturnBadRequest_WhenFileTypeIsNotAllowed()
    {
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        content.Add(fileContent, "File", "bad.exe");
        content.Add(new StringContent(((int)DocumentTypes.Passport).ToString()), "DocumentType");
        content.Add(new StringContent("P1234567"), "DocumentNumber");

        var client = CreateClient();
        var response = await client.PutAsync($"/documents/{familyMemberId}/documents/{docId}/file", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReplaceDocumentFile_ShouldReturnNotFound_WhenFamilyMemberIsMissing()
    {
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        _documentServiceMock
            .Setup(s => s.GetDocumentDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDetailsDto
            {
                Id = docId,
                FamilyMemberId = familyMemberId,
                DocumentNumber = "P1234567",
                DocumentType = DocumentTypes.Passport
            });

        _familyMemberServiceMock
            .Setup(s => s.GetFamilyMemberByIdAsync(familyMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyMemberDto?)null);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        content.Add(fileContent, "File", "replace.pdf");
        content.Add(new StringContent(((int)DocumentTypes.Passport).ToString()), "DocumentType");
        content.Add(new StringContent("P1234567"), "DocumentNumber");

        var client = CreateClient();
        var response = await client.PutAsync($"/documents/{familyMemberId}/documents/{docId}/file", content);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReplaceDocumentFile_ShouldReturnNotFound_WhenFamilyIsMissing()
    {
        var familyMemberId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        _documentServiceMock
            .Setup(s => s.GetDocumentDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDetailsDto
            {
                Id = docId,
                FamilyMemberId = familyMemberId,
                DocumentNumber = "P1234567",
                DocumentType = DocumentTypes.Passport
            });

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
            .ReturnsAsync((FamilyDto?)null);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        content.Add(fileContent, "File", "replace.pdf");
        content.Add(new StringContent(((int)DocumentTypes.Passport).ToString()), "DocumentType");
        content.Add(new StringContent("P1234567"), "DocumentNumber");

        var client = CreateClient();
        var response = await client.PutAsync($"/documents/{familyMemberId}/documents/{docId}/file", content);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
