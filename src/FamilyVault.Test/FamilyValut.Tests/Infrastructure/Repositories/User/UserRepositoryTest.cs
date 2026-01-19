using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using FamilyVault.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Tests.Infrastructure.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly UserRepository _sut;

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

    #endregion

    #region GetAllAsync

    [Fact]
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

    #endregion

    #region AddAsync

    [Fact]
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

    public void Dispose()
    {
        _dbContext.Dispose();
        _memoryCache.Dispose();
    }
}
