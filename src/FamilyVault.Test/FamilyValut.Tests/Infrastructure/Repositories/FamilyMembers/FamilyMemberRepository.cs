using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using FamilyVault.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Tests.Infrastructure.Repositories;

public class FamilyMemberRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly FamilyMemberRepository _sut;

    public FamilyMemberRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _sut = new FamilyMemberRepository(_dbContext, _memoryCache);
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_ShouldPersistFamilyMember_AndClearCaches()
    {
        // Arrange
        _memoryCache.Set("AllFamiliesWithDocuments", new List<FamilyMember>());
        _memoryCache.Set("AllFamilies", new List<FamilyMember>());

        var member = new FamilyMember
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            FamilyId = Guid.NewGuid()
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
    public async Task GetAllWithDocumentsAsync_ShouldReturnMembers_WithDocuments_AndCache()
    {
        // Arrange
        var member = new FamilyMember
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            DocumentDetails =
            {
                new DocumentDetails { Id = Guid.NewGuid(), DocumentNumber = "DOC1" },
                new DocumentDetails { Id = Guid.NewGuid(), DocumentNumber = "DOC2" }
            }
        };

        _dbContext.FamilyMembers.Add(member);
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

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFamilyMember_WhenExists()
    {
        // Arrange
        var member = new FamilyMember
        {
            Id = Guid.NewGuid(),
            FirstName = "Lookup"
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
    public async Task GetAllByFamilyIdAsync_ShouldReturnMembers_ForFamily_AndCache()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        var members = new[]
        {
            new FamilyMember { Id = Guid.NewGuid(), FamilyId = familyId },
            new FamilyMember { Id = Guid.NewGuid(), FamilyId = familyId }
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

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFamilyMember_AndClearCaches()
    {
        // Arrange
        var member = new FamilyMember
        {
            Id = Guid.NewGuid(),
            FirstName = "Old",
            FamilyId = Guid.NewGuid()
        };

        _dbContext.FamilyMembers.Add(member);
        await _dbContext.SaveChangesAsync();

        _memoryCache.Set("AllFamiliesWithDocuments", new List<FamilyMember>());
        _memoryCache.Set("AllFamilies", new List<FamilyMember>());

        member.FirstName = "Updated";

        // Act
        var result = await _sut.UpdateAsync(member, CancellationToken.None);

        // Assert
        var updated = await _dbContext.FamilyMembers.FindAsync(member.Id);
        updated!.FirstName.Should().Be("Updated");

        _memoryCache.TryGetValue("AllFamiliesWithDocuments", out _).Should().BeFalse();
        _memoryCache.TryGetValue("AllFamilies", out _).Should().BeFalse();
    }

    [Fact]
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
    public async Task DeleteByIdAsync_ShouldSoftDeleteFamilyMember_AndDocuments()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var user = "test-user";

        var member = new FamilyMember { Id = memberId };
        var document = new DocumentDetails
        {
            Id = Guid.NewGuid(),
            FamilyMemberId = memberId
        };

        _dbContext.FamilyMembers.Add(member);
        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.DeleteByIdAsync(memberId, user, CancellationToken.None);

        // Assert
        (await _dbContext.FamilyMembers.FindAsync(memberId))!.IsDeleted.Should().BeTrue();
        (await _dbContext.Documents.FindAsync(document.Id))!.IsDeleted.Should().BeTrue();
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Dispose();
        _memoryCache.Dispose();
    }
}
