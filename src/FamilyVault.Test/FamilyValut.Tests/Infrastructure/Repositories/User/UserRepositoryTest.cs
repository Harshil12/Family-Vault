using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using FamilyVault.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Tests.Infrastructure.Repositories;

/// <summary>
/// Represents UserRepositoryTests.
/// </summary>
public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly UserRepository _sut;

    /// <summary>
    /// Initializes a new instance of UserRepositoryTests.
    /// </summary>
    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _sut = new UserRepository(_dbContext, _memoryCache);
    }

    #region GetAllWithFamilyDetailsAsync

    [Fact]
    /// <summary>
    /// Performs the GetAllWithFamilyDetailsAsync_ShouldReturnUsers_WithFamilies_AndCache operation.
    /// </summary>
    public async Task GetAllWithFamilyDetailsAsync_ShouldReturnUsers_WithFamilies_AndCache()
    {
        // Arrange
        var user = new User
        {
            Username = "u1",
            Id = Guid.NewGuid(),
            FirstName = "John",
            Email = "john@doe",
            Password = "password",
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
        };

        var family = new Family
        {
            Id = Guid.NewGuid(),
            Name = "Family",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user",
            UserId = user.Id
        };
        var family1 = new Family
        {
            Id = Guid.NewGuid(),
            Name = "Family1",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user",
            UserId = user.Id
        };


        _dbContext.Users.Add(user);
        _dbContext.Families.Add(family);
        _dbContext.Families.Add(family1);
        await _dbContext.SaveChangesAsync();

        // Act
        var first = await _sut.GetAllWithFamilyDetailsAsync(CancellationToken.None);
        var second = await _sut.GetAllWithFamilyDetailsAsync(CancellationToken.None);

        // Assert
        first.Should().HaveCount(1);
        first.First().Families.Should().HaveCount(2);

        // Cached instance reused
        first.Should().BeSameAs(second);
    }

    [Fact]
    public async Task GetAllWithFamilyDetailsAsync_ShouldExcludeSoftDeletedUsers()
    {
        // Arrange
        var activeUser = new User
        {
            Username = "active",
            Id = Guid.NewGuid(),
            FirstName = "Active",
            Email = "active@test.com",
            Password = "password",
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var deletedUser = new User
        {
            Username = "deleted",
            Id = Guid.NewGuid(),
            FirstName = "Deleted",
            Email = "deleted@test.com",
            Password = "password",
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = true
        };

        _dbContext.Users.AddRange(activeUser, deletedUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllWithFamilyDetailsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Username.Should().Be("active");
    }

    [Fact]
    public async Task GetAllWithFamilyDetailsAsync_ShouldExcludeSoftDeletedFamilies()
    {
        // Arrange
        var user = new User
        {
            Username = "u1",
            Id = Guid.NewGuid(),
            FirstName = "John",
            Email = "john@doe.com",
            Password = "password",
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
        };

        var activeFamily = new Family
        {
            Id = Guid.NewGuid(),
            Name = "ActiveFamily",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user",
            UserId = user.Id,
            IsDeleted = false
        };

        var deletedFamily = new Family
        {
            Id = Guid.NewGuid(),
            Name = "DeletedFamily",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user",
            UserId = user.Id,
            IsDeleted = true
        };

        _dbContext.Users.Add(user);
        _dbContext.Families.AddRange(activeFamily, deletedFamily);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllWithFamilyDetailsAsync(CancellationToken.None);

        // Assert
        result.First().Families.Should().HaveCount(1);
        result.First().Families.First().Name.Should().Be("ActiveFamily");
    }

    #endregion

    #region GetAllAsync

    [Fact]
    /// <summary>
    /// Performs the GetAllAsync_ShouldReturnUsers_AndCache operation.
    /// </summary>
    public async Task GetAllAsync_ShouldReturnUsers_AndCache()
    {
        // Arrange
        _dbContext.Users.AddRange(
            new User { Id = Guid.NewGuid(), Username = "u1", FirstName = "John", Email = "john@doe", Password = "password",CreatedBy = "test-user", CreatedAt = DateTime.UtcNow },
            new User { Id = Guid.NewGuid(), Username = "u2", FirstName = "John", Email = "john@doe", Password = "password",CreatedBy = "test-user", CreatedAt = DateTime.UtcNow }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var first = await _sut.GetAllAsync(CancellationToken.None);
        var second = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        first.Should().HaveCount(2);
        first.Should().BeSameAs(second);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    /// <summary>
    /// Performs the GetByIdAsync_ShouldReturnUser_WhenExists operation.
    /// </summary>
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var user = new User { Username = "u1", Id = Guid.NewGuid(), Email = "test@test.com", FirstName = "John" ,Password = "password",CreatedBy = "test-user", CreatedAt = DateTime.UtcNow };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(user.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
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

    #region GetByEmailAsync

    [Fact]
    /// <summary>
    /// Performs the GetByEmailAsync_ShouldReturnUser_WhenEmailExists operation.
    /// </summary>
    public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
    {
        // Arrange
        var email = "john@doe.com";
        var user = new User { Id = Guid.NewGuid(), Email = email, Username = "u1", FirstName = "John", Password = "password", CreatedBy = "test-user", CreatedAt = DateTime.UtcNow };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetByEmailAsync(email, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailNotFound()
    {
        // Act
        var result = await _sut.GetByEmailAsync("nonexistent@test.com", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenUserIsDeleted()
    {
        // Arrange
        var email = "deleted@test.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Username = "deleted",
            FirstName = "Deleted",
            Password = "password",
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetByEmailAsync(email, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddAsync

    [Fact]
    /// <summary>
    /// Performs the AddAsync_ShouldPersistUser_AndClearCaches operation.
    /// </summary>
    public async Task AddAsync_ShouldPersistUser_AndClearCaches()
    {
        // Arrange
        _memoryCache.Set("UsersWithFamilies", new List<User>());
        _memoryCache.Set("UsersFamilies", new List<User>());

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "newuser",
            FirstName = "New",
            Password = "password",
            Email = "new@user.com",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user-add"
        };

        // Act
        var result = await _sut.AddAsync(user, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        (await _dbContext.Users.FindAsync(user.Id)).Should().NotBeNull();

        _memoryCache.TryGetValue("UsersWithFamilies", out _).Should().BeFalse();
        _memoryCache.TryGetValue("UsersFamilies", out _).Should().BeFalse();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    /// <summary>
    /// Performs the UpdateAsync_ShouldUpdateUser_AndClearCaches operation.
    /// </summary>
    public async Task UpdateAsync_ShouldUpdateUser_AndClearCaches()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "newuser",
            Password = "password",
            FirstName = "Old",
            Email = "old@test.com",
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _memoryCache.Set("UsersWithFamilies", new List<User>());
        _memoryCache.Set("UsersFamilies", new List<User>());

        user.FirstName = "Updated";
        user.Email = "updated@test.com";
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = "test-user-update";

        // Act
        var result = await _sut.UpdateAsync(user, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Users.FindAsync(user.Id);
        updated!.FirstName.Should().Be("Updated");
        updated.Email.Should().Be("updated@test.com");

        _memoryCache.TryGetValue("UsersWithFamilies", out _).Should().BeFalse();
        _memoryCache.TryGetValue("UsersFamilies", out _).Should().BeFalse();
    }

    [Fact]
    /// <summary>
    /// Performs the UpdateAsync_ShouldThrow_WhenUserNotFound operation.
    /// </summary>
    public async Task UpdateAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };

        // Act
        Func<Task> act = async () =>
            await _sut.UpdateAsync(user, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region DeleteByIdAsync

    [Fact]
    /// <summary>
    /// Performs the DeleteByIdAsync_ShouldSoftDeleteUser_Families_Members_AndDocuments operation.
    /// </summary>
    public async Task DeleteByIdAsync_ShouldSoftDeleteUser_Families_Members_AndDocuments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "admin";

        var user = new User { Id = userId, Username = "u1", CreatedAt = DateTime.UtcNow, CreatedBy = username, FirstName = "John", Email = "john@doe", Password = "password" };
        var family = new Family { Id = Guid.NewGuid(), Name = "Family", UserId = userId, CreatedAt = DateTime.UtcNow, CreatedBy = username };
        var member = new FamilyMember { Id = Guid.NewGuid(), FirstName = "John", FamilyId = family.Id, CreatedAt = DateTime.UtcNow, CreatedBy = username };
        var document = new DocumentDetails { Id = Guid.NewGuid(), FamilyMemberId = member.Id, CreatedAt = DateTime.UtcNow, CreatedBy = username, DocumentType= Domain.Enums.DocumentTypes.PAN , DocumentNumber = "1234567890" };

        _dbContext.Users.Add(user);
        _dbContext.Families.Add(family);
        _dbContext.FamilyMembers.Add(member);
        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.DeleteByIdAsync(userId, username, CancellationToken.None);

        // Assert
        (await _dbContext.Users.FindAsync(userId))!.IsDeleted.Should().BeTrue();
        (await _dbContext.Families.FindAsync(family.Id))!.IsDeleted.Should().BeTrue();
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