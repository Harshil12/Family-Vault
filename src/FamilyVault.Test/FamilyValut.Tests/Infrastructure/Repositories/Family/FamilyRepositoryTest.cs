using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using FamilyVault.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Tests.Infrastructure.Repositories;

public class FamilyRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly FamilyRepository _sut;

    public FamilyRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _sut = new FamilyRepository(_dbContext, _memoryCache);
    }

    private async Task<User> SeedUserAsync(Guid? userId = null)
    {
        var user = new User
        {
            Id = userId ?? Guid.NewGuid(),
            Username = $"user-{Guid.NewGuid():N}",
            FirstName = "Test",
            Email = $"{Guid.NewGuid():N}@example.com",
            Password = "password",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user"
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    #region GetAllWithFamilyMembersAsync

    [Fact]
    public async Task GetAllWithFamilyMembersAsync_ShouldReturnFamilies_WithMembers_AndCache()
    {
        // Arrange
        var user = await SeedUserAsync();
        var family = new Family
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
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

    [Fact]
    public async Task GetAllWithFamilyMembersAsync_ShouldReturnEmpty_WhenNoFamilies()
    {
        // Act
        var result = await _sut.GetAllWithFamilyMembersAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAllByUserIdAsync

    [Fact]
    public async Task GetAllByUserIdAsync_ShouldReturnFamilies_ForUser_AndCache()
    {
        // Arrange
        var user = await SeedUserAsync();
        var userId = user.Id;

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

    [Fact]
    public async Task GetAllByUserIdAsync_ShouldReturnEmpty_WhenUserHasNoFamilies()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _sut.GetAllByUserIdAsync(userId, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ShouldReturnOnlySpecificUsersFamilies()
    {
        // Arrange
        var user1 = await SeedUserAsync();
        var user2 = await SeedUserAsync();
        var userId1 = user1.Id;
        var userId2 = user2.Id;

        var families = new[]
        {
            new Family { Id = Guid.NewGuid(), UserId = userId1, Name = "User1Family", CreatedBy="test-user", CreatedAt = DateTime.UtcNow },
            new Family { Id = Guid.NewGuid(), UserId = userId2, Name = "User2Family", CreatedBy="test-user", CreatedAt = DateTime.UtcNow }
        };

        _dbContext.Families.AddRange(families);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllByUserIdAsync(userId1, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("User1Family");
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFamily_WhenExists()
    {
        // Arrange
        var user = await SeedUserAsync();
        var family = new Family
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
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
    public async Task AddAsync_ShouldPersistFamily_AndClearCaches()
    {
        // Arrange
        var user = await SeedUserAsync();
        var userId = user.Id;
        _memoryCache.Set("AllWithFamilyMembers", new List<Family>());
        _memoryCache.Set($"AllFamilyMembers:{userId}", new List<Family>());

        var family = new Family
        {
            Id = Guid.NewGuid(),
            UserId = userId,
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
        _memoryCache.TryGetValue($"AllFamilyMembers:{userId}", out _).Should().BeFalse();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFamily_AndClearCaches()
    {
        // Arrange
        var user = await SeedUserAsync();
        var family = new Family
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user-add"
        };

        _dbContext.Families.Add(family);
        await _dbContext.SaveChangesAsync();

        _memoryCache.Set("AllWithFamilyMembers", new List<Family>());
        _memoryCache.Set($"AllFamilyMembers:{family.UserId}", new List<Family>());

        family.Name = "Updated Name";
        family.UpdatedAt = DateTime.UtcNow;
        family.UpdatedBy = "test-user-update";

        // Act
        var result = await _sut.UpdateAsync(family, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Families.FindAsync(family.Id);
        updated!.Name.Should().Be("Updated Name");

        _memoryCache.TryGetValue("AllWithFamilyMembers", out _).Should().BeFalse();
        _memoryCache.TryGetValue($"AllFamilyMembers:{family.UserId}", out _).Should().BeFalse();
    }

    [Fact]
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
    public async Task DeleteByIdAsync_ShouldSoftDeleteFamily_Members_AndDocuments()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = "test-user";
        await SeedUserAsync(userId);

        var family = new Family { Id = familyId, UserId = userId, Name = "Test Family",  CreatedAt= DateTime.UtcNow, CreatedBy = user };
        var member = new FamilyMember { Id = Guid.NewGuid(), FirstName = "Member", FamilyId = familyId, CreatedAt = DateTime.UtcNow, CreatedBy = user };
        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            FamilyMemberId = member.Id,
            DocumentType = Domain.Enums.DocumentTypes.Passport,
            DocumentNumber = "DOC-12345",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user
        };

        _dbContext.Families.Add(family);
        _dbContext.FamilyMembers.Add(member);
        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.DeleteByIdAsync(familyId, user, CancellationToken.None);

        // Assert
        _dbContext.ChangeTracker.Clear();
        (await _dbContext.Families.IgnoreQueryFilters().FirstAsync(f => f.Id == familyId)).IsDeleted.Should().BeTrue();
        (await _dbContext.FamilyMembers.IgnoreQueryFilters().FirstAsync(fm => fm.Id == member.Id)).IsDeleted.Should().BeTrue();
        (await _dbContext.Documents.IgnoreQueryFilters().FirstAsync(d => d.Id == document.Id)).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldClearCache()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await SeedUserAsync(userId);
        var family = new Family { Id = familyId, Name = "Test Family", UserId = userId, CreatedAt = DateTime.UtcNow, CreatedBy = "test-user" };

        _dbContext.Families.Add(family);
        await _dbContext.SaveChangesAsync();

        _memoryCache.Set("AllWithFamilyMembers", new List<Family>());
        _memoryCache.Set($"AllFamilyMembers:{userId}", new List<Family>());

        // Act
        await _sut.DeleteByIdAsync(familyId, "test-user", CancellationToken.None);

        // Assert
        _memoryCache.TryGetValue("AllWithFamilyMembers", out _).Should().BeFalse();
        _memoryCache.TryGetValue($"AllFamilyMembers:{userId}", out _).Should().BeFalse();
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Dispose();
        _memoryCache.Dispose();
        _connection.Dispose();
    }
}
