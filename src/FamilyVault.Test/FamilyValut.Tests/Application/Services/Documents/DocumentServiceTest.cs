using AutoMapper;
using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Application.Services;
using FamilyVault.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FamilyVault.Tests.Services;

/// <summary>
/// Represents DocumentServiceTests.
/// </summary>
public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepoMock;
    private readonly Mock<ICryptoService> _cryptoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<DocumentService>> _loggerMock;

    private readonly DocumentService _sut;

    /// <summary>
    /// Initializes a new instance of DocumentServiceTests.
    /// </summary>
    public DocumentServiceTests()
    {
        _documentRepoMock = new Mock<IDocumentRepository>();
        _cryptoMock = new Mock<ICryptoService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<DocumentService>>();

        _sut = new DocumentService(
            _documentRepoMock.Object,
            _cryptoMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    #region GetDocumentsDetailsByFamilyMemberIdAsync

    [Fact]
    /// <summary>
    /// Performs the GetDocumentsDetailsByFamilyMemberIdAsync_ShouldDecryptDocumentNumbers operation.
    /// </summary>
    public async Task GetDocumentsDetailsByFamilyMemberIdAsync_ShouldDecryptDocumentNumbers()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();

        var documents = new List<DocumentDetails>
        {
            new() { Id = Guid.NewGuid(), DocumentNumber = "encrypted-1" },
            new() { Id = Guid.NewGuid(), DocumentNumber = "encrypted-2" }
        };

        _documentRepoMock
            .Setup(r => r.GetAllByFamilymemberIdAsync(familyMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        _cryptoMock
            .Setup(c => c.DecryptData(It.IsAny<string>()))
            .Returns<string>(s => $"decrypted-{s}");

        _mapperMock
            .Setup(m => m.Map<List<DocumentDetailsDto>>(documents))
            .Returns(new List<DocumentDetailsDto>());

        // Act
        var result = await _sut.GetDocumentsDetailsByFamilyMemberIdAsync(
            familyMemberId,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _cryptoMock.Verify(c => c.DecryptData("encrypted-1"), Times.Once);
        _cryptoMock.Verify(c => c.DecryptData("encrypted-2"), Times.Once);
    }

    #endregion

    #region GetDocumentDetailsByIdAsync

    [Fact]
    /// <summary>
    /// Performs the GetDocumentDetailsByIdAsync_ShouldDecrypt_WhenDocumentExists operation.
    /// </summary>
    public async Task GetDocumentDetailsByIdAsync_ShouldDecrypt_WhenDocumentExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        var document = new DocumentDetails
        {
            Id = documentId,
            DocumentNumber = "encrypted"
        };

        var dto = new DocumentDetailsDto { Id = documentId };

        _documentRepoMock
            .Setup(r => r.GetAsyncbyId(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _cryptoMock
            .Setup(c => c.DecryptData("encrypted"))
            .Returns("decrypted");

        _mapperMock
            .Setup(m => m.Map<DocumentDetailsDto>(document))
            .Returns(dto);

        // Act
        var result = await _sut.GetDocumentDetailsByIdAsync(documentId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _cryptoMock.Verify(c => c.DecryptData("encrypted"), Times.Once);
    }

    [Fact]
    /// <summary>
    /// Performs the GetDocumentDetailsByIdAsync_ShouldReturnNullDto_WhenDocumentDoesNotExist operation.
    /// </summary>
    public async Task GetDocumentDetailsByIdAsync_ShouldReturnNullDto_WhenDocumentDoesNotExist()
    {
        // Arrange
        _documentRepoMock
            .Setup(r => r.GetAsyncbyId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentDetails)null!);

        _mapperMock
            .Setup(m => m.Map<DocumentDetailsDto>(null))
            .Returns((DocumentDetailsDto)null!);

        // Act
        var result = await _sut.GetDocumentDetailsByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _cryptoMock.Verify(c => c.DecryptData(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region CreateDocumentDetailsAsync

    [Fact]
    /// <summary>
    /// Performs the CreateDocumentDetailsAsync_ShouldEncryptAndPersistDocument operation.
    /// </summary>
    public async Task CreateDocumentDetailsAsync_ShouldEncryptAndPersistDocument()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = new CreateDocumentRequest
        {
            FamilyMemberId = Guid.NewGuid(),
            DocumentNumber = "plain-number"
        };

        var entity = new DocumentDetails
        {
            DocumentNumber = "plain-number"
        };

        var savedEntity = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "encrypted-number"
        };

        var dto = new DocumentDetailsDto { Id = savedEntity.Id };

        _mapperMock
            .Setup(m => m.Map<DocumentDetails>(request))
            .Returns(entity);

        _cryptoMock
            .Setup(c => c.EncryptData("plain-number"))
            .Returns("encrypted-number");

        _documentRepoMock
            .Setup(r => r.AddAsync(It.IsAny<DocumentDetails>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        _mapperMock
            .Setup(m => m.Map<DocumentDetailsDto>(savedEntity))
            .Returns(dto);

        // Act
        var result = await _sut.CreateDocumentDetailsAsync(request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _cryptoMock.Verify(c => c.EncryptData("plain-number"), Times.Once);
        _documentRepoMock.Verify(r => r.AddAsync(It.IsAny<DocumentDetails>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UpdateDocumentDetailsAsync

    [Fact]
    /// <summary>
    /// Performs the UpdateDocumentDetailsAsync_ShouldEncryptAndUpdateDocument operation.
    /// </summary>
    public async Task UpdateDocumentDetailsAsync_ShouldEncryptAndUpdateDocument()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = new UpdateDocumentRequest
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "plain-number"
        };

        var entity = new DocumentDetails
        {
            Id = request.Id,
            DocumentNumber = "plain-number"
        };

        var updatedEntity = new DocumentDetails
        {
            Id = request.Id,
            DocumentNumber = "encrypted-number"
        };

        var dto = new DocumentDetailsDto { Id = request.Id };

        _mapperMock
            .Setup(m => m.Map<DocumentDetails>(request))
            .Returns(entity);

        _cryptoMock
            .Setup(c => c.EncryptData("plain-number"))
            .Returns("encrypted-number");

        _documentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<DocumentDetails>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEntity);

        _mapperMock
            .Setup(m => m.Map<DocumentDetailsDto>(updatedEntity))
            .Returns(dto);

        // Act
        var result = await _sut.UpdateDocumentDetailsAsync(request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _cryptoMock.Verify(c => c.EncryptData("plain-number"), Times.Once);
    }

    #endregion

    #region DeleteDocumentDetailsByIdAsync

    [Fact]
    /// <summary>
    /// Performs the DeleteDocumentDetailsByIdAsync_ShouldCallRepository operation.
    /// </summary>
    public async Task DeleteDocumentDetailsByIdAsync_ShouldCallRepository()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        await _sut.DeleteDocumentDetailsByIdAsync(documentId, userId, CancellationToken.None);

        // Assert
        _documentRepoMock.Verify(
            r => r.DeleteByIdAsync(documentId, userId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
