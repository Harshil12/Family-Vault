using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using FamilyVault.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Tests.Infrastructure.Repositories;

/// <summary>
/// Represents FamilyMemberRepositoryTests.
/// </summary>
public class FamilyMemberRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly FamilyMemberRepository _sut;

    /// <summary>
    /// Initializes a new instance of FamilyMemberRepositoryTests.
    /// </summary>
    public FamilyMemberRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new AppDbContext(options);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _sut = new FamilyMemberRepository(_dbContext, _memoryCache);
    }

    #region AddAsync

    [Fact]
    /// <summary>
    /// Performs the AddAsync_ShouldPersistFamilyMember_AndClearCaches operation.
    /// </summary>
    public async Task AddAsync_ShouldPersistFamilyMember_AndClearCaches()
    {
        // Arrange
        _memoryCache.Set("AllFamiliesWithDocuments", new List<FamilyMember>());
        _memoryCache.Set("AllFamilies", new List<FamilyMember>());

        var member = new FamilyMember
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            FamilyId = Guid.NewGuid(),
            CreatedBy = "test-user-add",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _sut.AddAsync(member, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var saved = await _dbContext.FamilyMembers.FindAsync(member.Id);
        saved.Should().NotBeNull();

        _memoryCache.TryGetValue("AllFamiliesWithDocuments", out _).Should().BeFalse();
        _memoryCache.TryGetValue("AllFamilies", out _).Should().BeFalse();
    }

    #endregion

    #region GetAllWithDocumentsAsync

    [Fact]
    /// <summary>
    /// Performs the GetAllWithDocumentsAsync_ShouldReturnMembers_WithDocuments_AndCache operation.
    /// </summary>
    public async Task GetAllWithDocumentsAsync_ShouldReturnMembers_WithDocuments_AndCache()
    {
        // Arrange
        var member = new FamilyMember
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user-add",
            FamilyId = Guid.NewGuid(),
        };

        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            DocumentType = Domain.Enums.DocumentTypes.Passport,
            DocumentNumber = "P123456",
            FamilyMemberId = member.Id,
            CreatedBy = "test-user-add",
            CreatedAt = DateTime.UtcNow
        };

        var document1 = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            DocumentType = Domain.Enums.DocumentTypes.Passport,
            DocumentNumber = "P123456",
            FamilyMemberId = member.Id,
            CreatedBy = "test-user-add",
            CreatedAt = DateTime.UtcNow
        };


        _dbContext.FamilyMembers.Add(member);
        _dbContext.Documents.Add(document);
        _dbContext.Documents.Add(document1);
        await _dbContext.SaveChangesAsync();

        // Act
        var first = await _sut.GetAllWithDocumentsAsync(CancellationToken.None);
        var second = await _sut.GetAllWithDocumentsAsync(CancellationToken.None);

        // Assert
        first.Should().HaveCount(1);
        first.First().DocumentDetails.Should().HaveCount(2);

        // Cached instance reused
        first.Should().BeSameAs(second);
    }

    [Fact]
    public async Task GetAllWithDocumentsAsync_ShouldReturnEmpty_WhenNoMembers()
    {
        // Act
        var result = await _sut.GetAllWithDocumentsAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    /// <summary>
    /// Performs the GetByIdAsync_ShouldReturnFamilyMember_WhenExists operation.
    /// </summary>
    public async Task GetByIdAsync_ShouldReturnFamilyMember_WhenExists()
    {
        // Arrange
        var member = new FamilyMember
        {
            Id = Guid.NewGuid(),
            FirstName = "Lookup",
            FamilyId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user-add"
        };

        _dbContext.FamilyMembers.Add(member);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(member.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(member.Id);
    }

    [Fact]
    /// <summary>
    /// Performs the GetByIdAsync_ShouldReturnNull_WhenNotFound operation.
    /// </summary>
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllByFamilyIdAsync

    [Fact]
    /// <summary>
    /// Performs the GetAllByFamilyIdAsync_ShouldReturnMembers_ForFamily_AndCache operation.
    /// </summary>
    public async Task GetAllByFamilyIdAsync_ShouldReturnMembers_ForFamily_AndCache()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        var members = new[]
        {
            new FamilyMember { Id = Guid.NewGuid(), FirstName = "John",  FamilyId = familyId, CreatedAt=DateTime.UtcNow ,CreatedBy="test-user-add"},
            new FamilyMember { Id = Guid.NewGuid(), FirstName = "Jane",  FamilyId = familyId, CreatedAt=DateTime.UtcNow ,CreatedBy="test-user-add"},
        };

        _dbContext.FamilyMembers.AddRange(members);
        await _dbContext.SaveChangesAsync();

        // Act
        var first = await _sut.GetAllByFamilyIdAsync(familyId, CancellationToken.None);
        var second = await _sut.GetAllByFamilyIdAsync(familyId, CancellationToken.None);

        // Assert
        first.Should().HaveCount(2);
        first.Should().BeSameAs(second);
    }

    [Fact]
    public async Task GetAllByFamilyIdAsync_ShouldReturnEmpty_WhenFamilyHasNoMembers()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        // Act
        var result = await _sut.GetAllByFamilyIdAsync(familyId, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    /// <summary>
    /// Performs the UpdateAsync_ShouldUpdateFamilyMember_AndClearCaches operation.
    /// </summary>
    public async Task UpdateAsync_ShouldUpdateFamilyMember_AndClearCaches()
    {
        // Arrange
        var member = new FamilyMember
        {
            Id = Guid.NewGuid(),
            FirstName = "Old",
            FamilyId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user-add"
        };

        _dbContext.FamilyMembers.Add(member);
        await _dbContext.SaveChangesAsync();

        _memoryCache.Set("AllFamiliesWithDocuments", new List<FamilyMember>());
        _memoryCache.Set("AllFamilies", new List<FamilyMember>());

        member.FirstName = "Updated";
        member.UpdatedAt = DateTime.UtcNow;
        member.UpdatedBy = "test-user-update";

        // Act
        var result = await _sut.UpdateAsync(member, CancellationToken.None);

        // Assert
        var updated = await _dbContext.FamilyMembers.FindAsync(member.Id);
        updated!.FirstName.Should().Be("Updated");

        _memoryCache.TryGetValue("AllFamiliesWithDocuments", out _).Should().BeFalse();
        _memoryCache.TryGetValue("AllFamilies", out _).Should().BeFalse();
    }

    [Fact]
    /// <summary>
    /// Performs the UpdateAsync_ShouldThrow_WhenFamilyMemberNotFound operation.
    /// </summary>
    public async Task UpdateAsync_ShouldThrow_WhenFamilyMemberNotFound()
    {
        // Arrange
        var member = new FamilyMember { Id = Guid.NewGuid() };

        // Act
        Func<Task> act = async () =>
            await _sut.UpdateAsync(member, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region DeleteByIdAsync

    [Fact]
    /// <summary>
    /// Performs the DeleteByIdAsync_ShouldSoftDeleteFamilyMember_AndDocuments operation.
    /// </summary>
    public async Task DeleteByIdAsync_ShouldSoftDeleteFamilyMember_AndDocuments()
    {
        // Arrange
        var memberId = Guid.NewGuid();

        var member = new FamilyMember { Id = memberId, FirstName = "John", CreatedAt = DateTime.UtcNow, CreatedBy = "test-user-add" };
        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            DocumentType = Domain.Enums.DocumentTypes.Passport,
            DocumentNumber = "P123456",
            FamilyMemberId = memberId,
            CreatedBy = "test-user-add",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.FamilyMembers.Add(member);
        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.DeleteByIdAsync(memberId, "test-user-add-delete", CancellationToken.None);

        // Assert
        (await _dbContext.FamilyMembers.FindAsync(memberId))!.IsDeleted.Should().BeTrue();
        (await _dbContext.Documents.FindAsync(document.Id))!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldClearCache()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var member = new FamilyMember { Id = memberId, FirstName = "John", FamilyId = familyId, CreatedAt = DateTime.UtcNow, CreatedBy = "test-user" };

        _dbContext.FamilyMembers.Add(member);
        await _dbContext.SaveChangesAsync();

        _memoryCache.Set("AllFamiliesWithDocuments", new List<FamilyMember>());
        _memoryCache.Set($"FamilyMembers:{familyId}", new List<FamilyMember>());

        // Act
        await _sut.DeleteByIdAsync(memberId, "test-user", CancellationToken.None);

        // Assert
        _memoryCache.TryGetValue("AllFamiliesWithDocuments", out _).Should().BeFalse();
        _memoryCache.TryGetValue($"FamilyMembers:{familyId}", out _).Should().BeFalse();
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