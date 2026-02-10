using AutoMapper;
using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Services;
using FamilyVault.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FamilyVault.Tests.Services;

/// <summary>
/// Represents FamilyServiceTests.
/// </summary>
public class FamilyServiceTests
{
    private readonly Mock<IFamilyRepository> _familyRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<FamilyService>> _loggerMock;

    private readonly FamilyService _sut;

    /// <summary>
    /// Initializes a new instance of FamilyServiceTests.
    /// </summary>
    public FamilyServiceTests()
    {
        _familyRepoMock = new Mock<IFamilyRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<FamilyService>>();

        _sut = new FamilyService(
            _familyRepoMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    #region GetFamilyByUserIdAsync

    [Fact]
    /// <summary>
    /// Performs the GetFamilyByUserIdAsync_ShouldReturnMappedFamilies operation.
    /// </summary>
    public async Task GetFamilyByUserIdAsync_ShouldReturnMappedFamilies()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var families = new List<Family>
        {
            new() { Id = Guid.NewGuid(), Name = "Family One" },
            new() { Id = Guid.NewGuid(), Name = "Family Two" }
        };

        var familyDtos = new List<FamilyDto>
        {
            new() { Id = families[0].Id },
            new() { Id = families[1].Id }
        };

        _familyRepoMock
            .Setup(r => r.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(families);

        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<FamilyDto>>(families))
            .Returns(familyDtos);

        // Act
        var result = await _sut.GetFamilyByUserIdAsync(userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetFamilyByIdAsync

    [Fact]
    /// <summary>
    /// Performs the GetFamilyByIdAsync_ShouldReturnMappedFamily operation.
    /// </summary>
    public async Task GetFamilyByIdAsync_ShouldReturnMappedFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        var family = new Family
        {
            Id = familyId,
            Name = "My Family"
        };

        var familyDto = new FamilyDto
        {
            Id = familyId
        };

        _familyRepoMock
            .Setup(r => r.GetByIdAsync(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        _mapperMock
            .Setup(m => m.Map<FamilyDto>(family))
            .Returns(familyDto);

        // Act
        var result = await _sut.GetFamilyByIdAsync(familyId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(familyId);
    }

    #endregion

    #region CreateFamilyAsync

    [Fact]
    /// <summary>
    /// Performs the CreateFamilyAsync_ShouldCreateAndReturnFamily operation.
    /// </summary>
    public async Task CreateFamilyAsync_ShouldCreateAndReturnFamily()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = new CreateFamilyRequest
        {
            FamilyName = "New Family"
        };

        var familyEntity = new Family
        {
            Name = request.FamilyName
        };

        var savedFamily = new Family
        {
            Id = Guid.NewGuid(),
            Name = request.FamilyName
        };

        var familyDto = new FamilyDto
        {
            Id = savedFamily.Id
        };

        _mapperMock
            .Setup(m => m.Map<Family>(request))
            .Returns(familyEntity);

        _familyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Family>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedFamily);

        _mapperMock
            .Setup(m => m.Map<FamilyDto>(savedFamily))
            .Returns(familyDto);

        // Act
        var result = await _sut.CreateFamilyAsync(request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _familyRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<Family>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteFamilyByIdAsync

    [Fact]
    /// <summary>
    /// Performs the DeleteFamilyByIdAsync_ShouldCallRepository operation.
    /// </summary>
    public async Task DeleteFamilyByIdAsync_ShouldCallRepository()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        await _sut.DeleteFamilyByIdAsync(familyId, userId, CancellationToken.None);

        // Assert
        _familyRepoMock.Verify(r =>
            r.DeleteByIdAsync(familyId, userId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateFamilyAsync

    [Fact]
    /// <summary>
    /// Performs the UpdateFamilyAsync_ShouldUpdateAndReturnFamily operation.
    /// </summary>
    public async Task UpdateFamilyAsync_ShouldUpdateAndReturnFamily()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = new UpdateFamlyRequest
        {
            Id = Guid.NewGuid(),
            FamilyName = "Updated Family"
        };

        var familyEntity = new Family
        {
            Id = request.Id,
            Name = request.FamilyName
        };

        var updatedFamily = new Family
        {
            Id = request.Id,
            Name = request.FamilyName
        };

        var familyDto = new FamilyDto
        {
            Id = request.Id
        };

        _mapperMock
            .Setup(m => m.Map<Family>(request))
            .Returns(familyEntity);

        _familyRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Family>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedFamily);

        _mapperMock
            .Setup(m => m.Map<FamilyDto>(updatedFamily))
            .Returns(familyDto);

        // Act
        var result = await _sut.UpdateFamilyAsync(request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(request.Id);
    }

    #endregion
}
