using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using FamilyVault.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Tests.Infrastructure.Repositories;

/// <summary>
/// Represents FamilyRepositoryTests.
/// </summary>
public class FamilyRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly FamilyRepository _sut;

    /// <summary>
    /// Initializes a new instance of FamilyRepositoryTests.
    /// </summary>
    public FamilyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new AppDbContext(options);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _sut = new FamilyRepository(_dbContext, _memoryCache);
    }

    #region GetAllWithFamilyMembersAsync

    [Fact]
    /// <summary>
    /// Performs the GetAllWithFamilyMembersAsync_ShouldReturnFamilies_WithMembers_AndCache operation.
    /// </summary>
    public async Task GetAllWithFamilyMembersAsync_ShouldReturnFamilies_WithMembers_AndCache()
    {
        // Arrange
        var family = new Family
        {
            Id = Guid.NewGuid(),
            Name = "Test Family",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user"
        };
        var familyMember = new FamilyMember 
        { 
            Id = Guid.NewGuid(), 
            FirstName = "John", 
            FamilyId = family.Id, 
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow 
        };

        _dbContext.Families.Add(family);
        _dbContext.FamilyMembers.Add(familyMember);
        await _dbContext.SaveChangesAsync();

        // Act
        var first = await _sut.GetAllWithFamilyMembersAsync(CancellationToken.None);
        var second = await _sut.GetAllWithFamilyMembersAsync(CancellationToken.None);

        // Assert
        first.Should().HaveCount(1);
        first.First().FamilyMembers.Should().HaveCount(1);

        // Cached instance reused
        first.Should().BeSameAs(second);
    }

    #endregion

    #region GetAllByUserIdAsync

    [Fact]
    /// <summary>
    /// Performs the GetAllByUserIdAsync_ShouldReturnFamilies_ForUser_AndCache operation.
    /// </summary>
    public async Task GetAllByUserIdAsync_ShouldReturnFamilies_ForUser_AndCache()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var families = new[]
        {
            new Family { Id = Guid.NewGuid(), UserId = userId, Name = "F1", CreatedBy="test-user" , CreatedAt = DateTime.UtcNow },
            new Family { Id = Guid.NewGuid(), UserId = userId, Name = "F2", CreatedBy="test-user" , CreatedAt = DateTime.UtcNow }
        };

        _dbContext.Families.AddRange(families);
        await _dbContext.SaveChangesAsync();

        // Act
        var first = await _sut.GetAllByUserIdAsync(userId, CancellationToken.None);
        var second = await _sut.GetAllByUserIdAsync(userId, CancellationToken.None);

        // Assert
        first.Should().HaveCount(2);
        first.Should().BeSameAs(second);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    /// <summary>
    /// Performs the GetByIdAsync_ShouldReturnFamily_WhenExists operation.
    /// </summary>
    public async Task GetByIdAsync_ShouldReturnFamily_WhenExists()
    {
        // Arrange
        var family = new Family
        {
            Id = Guid.NewGuid(),
            Name = "Lookup Family",
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Families.Add(family);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(family.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(family.Id);
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

    #region AddAsync

    [Fact]
    /// <summary>
    /// Performs the AddAsync_ShouldPersistFamily_AndClearCaches operation.
    /// </summary>
    public async Task AddAsync_ShouldPersistFamily_AndClearCaches()
    {
        // Arrange
        _memoryCache.Set("AllWithFamilyMembers", new List<Family>());
        _memoryCache.Set("AllFamilyMembers", new List<Family>());

        var family = new Family
        {
            Id = Guid.NewGuid(),
            Name = "New Family",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user-add"
        };

        // Act
        var result = await _sut.AddAsync(family, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        (await _dbContext.Families.FindAsync(family.Id)).Should().NotBeNull();

        _memoryCache.TryGetValue("AllWithFamilyMembers", out _).Should().BeFalse();
        _memoryCache.TryGetValue("AllFamilyMembers", out _).Should().BeFalse();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    /// <summary>
    /// Performs the UpdateAsync_ShouldUpdateFamily_AndClearCaches operation.
    /// </summary>
    public async Task UpdateAsync_ShouldUpdateFamily_AndClearCaches()
    {
        // Arrange
        var family = new Family
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            UserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user-add"
        };

        _dbContext.Families.Add(family);
        await _dbContext.SaveChangesAsync();

        _memoryCache.Set("AllWithFamilyMembers", new List<Family>());
        _memoryCache.Set("AllFamilyMembers", new List<Family>());

        family.Name = "Updated Name";
        family.UpdatedAt = DateTime.UtcNow;
        family.UpdatedBy = "test-user-update";

        // Act
        var result = await _sut.UpdateAsync(family, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Families.FindAsync(family.Id);
        updated!.Name.Should().Be("Updated Name");

        _memoryCache.TryGetValue("AllWithFamilyMembers", out _).Should().BeFalse();
        _memoryCache.TryGetValue("AllFamilyMembers", out _).Should().BeFalse();
    }

    [Fact]
    /// <summary>
    /// Performs the UpdateAsync_ShouldThrow_WhenFamilyNotFound operation.
    /// </summary>
    public async Task UpdateAsync_ShouldThrow_WhenFamilyNotFound()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid() };

        // Act
        Func<Task> act = async () =>
            await _sut.UpdateAsync(family, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region DeleteByIdAsync

    [Fact]
    /// <summary>
    /// Performs the DeleteByIdAsync_ShouldSoftDeleteFamily_Members_AndDocuments operation.
    /// </summary>
    public async Task DeleteByIdAsync_ShouldSoftDeleteFamily_Members_AndDocuments()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var user = "test-user";

        var family = new Family { Id = familyId, Name = "Test Family",  CreatedAt= DateTime.UtcNow, CreatedBy = user };
        var member = new FamilyMember { Id = Guid.NewGuid(), FamilyId = familyId, CreatedAt = DateTime.UtcNow, CreatedBy = user };
        var document = new DocumentDetails { Id = Guid.NewGuid(), FamilyMemberId = member.Id, CreatedAt = DateTime.UtcNow, CreatedBy = user };

        _dbContext.Families.Add(family);
        _dbContext.FamilyMembers.Add(member);
        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.DeleteByIdAsync(familyId, user, CancellationToken.None);

        // Assert
        (await _dbContext.Families.FindAsync(familyId))!.IsDeleted.Should().BeTrue();
        (await _dbContext.FamilyMembers.FindAsync(member.Id))!.IsDeleted.Should().BeTrue();
        (await _dbContext.Documents.FindAsync(document.Id))!.IsDeleted.Should().BeTrue();
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
