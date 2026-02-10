using FamilyVault.Domain.Entities;
using FamilyVault.Domain.Enums;
using FamilyVault.Infrastructure.Data;
using FamilyVault.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Tests.Infrastructure.Repositories;

/// <summary>
/// Represents DocumentRepositoryTests.
/// </summary>
public class DocumentRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly DocumentRepository _sut;

    /// <summary>
    /// Initializes a new instance of DocumentRepositoryTests.
    /// </summary>
    public DocumentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _sut = new DocumentRepository(_dbContext, _memoryCache);
    }

    #region GetAllByFamilymemberIdAsync

    [Fact]
    /// <summary>
    /// Performs the GetAllByFamilymemberIdAsync_ShouldReturnDocuments_AndCacheResult operation.
    /// </summary>
    public async Task GetAllByFamilymemberIdAsync_ShouldReturnDocuments_AndCacheResult()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();

        var documents = new List<DocumentDetails>
        {
            new() {
                Id = Guid.NewGuid(), FamilyMemberId = familyMemberId,
                CreatedAt = DateTime.UtcNow , CreatedBy="test-user",
                DocumentType = DocumentTypes.Passport,
                DocumentNumber = "123"
            },
             new() {
                Id = Guid.NewGuid(), FamilyMemberId = familyMemberId,
                CreatedAt = DateTime.UtcNow , CreatedBy="test-user",
                DocumentType = DocumentTypes.Passport,
                DocumentNumber = "123"
            }
        };

        _dbContext.Documents.AddRange(documents);
        await _dbContext.SaveChangesAsync();

        // Act
        var firstCall = await _sut.GetAllByFamilymemberIdAsync(familyMemberId, CancellationToken.None);
        var secondCall = await _sut.GetAllByFamilymemberIdAsync(familyMemberId, CancellationToken.None);

        // Assert
        firstCall.Should().HaveCount(2);
        secondCall.Should().HaveCount(2);

        // Cached result reused
        firstCall.Should().BeSameAs(secondCall);
    }

    #endregion

    #region GetAsyncbyId

    [Fact]
    /// <summary>
    /// Performs the GetAsyncbyId_ShouldReturnDocument_WhenExists operation.
    /// </summary>
    public async Task GetAsyncbyId_ShouldReturnDocument_WhenExists()
    {
        // Arrange
        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "123",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user",
            DocumentType = DocumentTypes.Passport
        };

        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAsyncbyId(document.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(document.Id);
    }

    [Fact]
    /// <summary>
    /// Performs the GetAsyncbyId_ShouldReturnNull_WhenNotFound operation.
    /// </summary>
    public async Task GetAsyncbyId_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _sut.GetAsyncbyId(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddAsync

    [Fact]
    /// <summary>
    /// Performs the AddAsync_ShouldPersistDocument_AndClearCache operation.
    /// </summary>
    public async Task AddAsync_ShouldPersistDocument_AndClearCache()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();

        _memoryCache.Set("AllDocument", new List<DocumentDetails>());

        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            FamilyMemberId = familyMemberId,
            DocumentNumber = "DOC123",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user-create"
        };

        // Act
        var result = await _sut.AddAsync(document, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var saved = await _dbContext.Documents.FindAsync(document.Id);
        saved.Should().NotBeNull();

        _memoryCache.TryGetValue("AllDocument", out _).Should().BeFalse();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    /// <summary>
    /// Performs the UpdateAsync_ShouldUpdateDocument_AndClearCache operation.
    /// </summary>
    public async Task UpdateAsync_ShouldUpdateDocument_AndClearCache()
    {
        // Arrange
        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "OLD",
            CreatedBy = "test-user-create",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();

        _memoryCache.Set("AllDocument", new List<DocumentDetails>());

        document.DocumentNumber = "NEW";
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = "test-user-Update";

        // Act
        var result = await _sut.UpdateAsync(document, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Documents.FindAsync(document.Id);
        updated!.DocumentNumber.Should().Be("NEW");

        _memoryCache.TryGetValue("AllDocument", out _).Should().BeFalse();
    }

    [Fact]
    /// <summary>
    /// Performs the UpdateAsync_ShouldThrow_WhenDocumentNotFound operation.
    /// </summary>
    public async Task UpdateAsync_ShouldThrow_WhenDocumentNotFound()
    {
        // Arrange
        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "NOT_FOUND"
        };

        // Act
        Func<Task> act = async () =>
            await _sut.UpdateAsync(document, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region DeleteByIdAsync

    [Fact]
    /// <summary>
    /// Performs the DeleteByIdAsync_ShouldSoftDeleteDocument_AndClearCache operation.
    /// </summary>
    public async Task DeleteByIdAsync_ShouldSoftDeleteDocument_AndClearCache()
    {
        // Arrange
        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "DELETE_ME",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user-add"
        };

        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();

        _memoryCache.Set("AllDocument", new List<DocumentDetails>());

        // Act
        await _sut.DeleteByIdAsync(document.Id, "test-user-delete", CancellationToken.None);

        // Assert
        var deleted = await _dbContext.Documents.FindAsync(document.Id);

        deleted!.IsDeleted.Should().BeTrue();
        deleted.UpdatedBy.Should().Be("test-user-delete");
        deleted.UpdatedAt.Should().NotBeNull();

        _memoryCache.TryGetValue("AllDocument", out _).Should().BeFalse();
    }

    [Fact]
    /// <summary>
    /// Performs the DeleteByIdAsync_ShouldThrow_WhenDocumentNotFound operation.
    /// </summary>
    public async Task DeleteByIdAsync_ShouldThrow_WhenDocumentNotFound()
    {
        // Act
        Func<Task> act = async () =>
            await _sut.DeleteByIdAsync(Guid.NewGuid(), "user", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    /// <summary>
    /// Performs the Dispose operation.
    /// </summary>
    public void Dispose()
    {
        _dbContext.Dispose();
        _memoryCache.Dispose();
    }
}
