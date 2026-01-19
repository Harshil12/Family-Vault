using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using FamilyVault.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Tests.Infrastructure.Repositories;

public class DocumentRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly DocumentRepository _sut;

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
    public async Task GetAllByFamilymemberIdAsync_ShouldReturnDocuments_AndCacheResult()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();

        var documents = new List<DocumentDetails>
        {
            new() { Id = Guid.NewGuid(), FamilyMemberId = familyMemberId },
            new() { Id = Guid.NewGuid(), FamilyMemberId = familyMemberId }
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
    public async Task GetAsyncbyId_ShouldReturnDocument_WhenExists()
    {
        // Arrange
        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "123"
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
    public async Task AddAsync_ShouldPersistDocument_AndClearCache()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();

        _memoryCache.Set("AllDocument", new List<DocumentDetails>());

        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            FamilyMemberId = familyMemberId,
            DocumentNumber = "DOC123"
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
    public async Task UpdateAsync_ShouldUpdateDocument_AndClearCache()
    {
        // Arrange
        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "OLD"
        };

        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();

        _memoryCache.Set("AllDocument", new List<DocumentDetails>());

        document.DocumentNumber = "NEW";

        // Act
        var result = await _sut.UpdateAsync(document, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Documents.FindAsync(document.Id);
        updated!.DocumentNumber.Should().Be("NEW");

        _memoryCache.TryGetValue("AllDocument", out _).Should().BeFalse();
    }

    [Fact]
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
    public async Task DeleteByIdAsync_ShouldSoftDeleteDocument_AndClearCache()
    {
        // Arrange
        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "DELETE_ME",
            IsDeleted = false
        };

        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();

        _memoryCache.Set("AllDocument", new List<DocumentDetails>());

        // Act
        await _sut.DeleteByIdAsync(document.Id, "test-user", CancellationToken.None);

        // Assert
        var deleted = await _dbContext.Documents.FindAsync(document.Id);

        deleted!.IsDeleted.Should().BeTrue();
        deleted.UpdatedBy.Should().Be("test-user");
        deleted.UpdatedAt.Should().NotBeNull();

        _memoryCache.TryGetValue("AllDocument", out _).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldThrow_WhenDocumentNotFound()
    {
        // Act
        Func<Task> act = async () =>
            await _sut.DeleteByIdAsync(Guid.NewGuid(), "user", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Dispose();
        _memoryCache.Dispose();
    }
}
