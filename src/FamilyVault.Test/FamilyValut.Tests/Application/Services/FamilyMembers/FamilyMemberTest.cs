using AutoMapper;
using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Services;
using FamilyVault.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FamilyVault.Tests.Services;

public class FamilyMemberServiceTests
{
    private readonly Mock<IFamilyMemberRepository> _familyMemberRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<FamilyMemberService>> _loggerMock;

    private readonly FamilyMemberService _sut;

    public FamilyMemberServiceTests()
    {
        _familyMemberRepoMock = new Mock<IFamilyMemberRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<FamilyMemberService>>();

        _sut = new FamilyMemberService(
            _familyMemberRepoMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    #region GetFamilyMemberByIdAsync

    [Fact]
    public async Task GetFamilyMemberByIdAsync_ShouldReturnMappedDto()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();

        var entity = new FamilyMember
        {
            Id = familyMemberId,
            FirstName = "John",
            LastName = "Doe"
        };

        var dto = new FamilyMemberDto
        {
            Id = familyMemberId
        };

        _familyMemberRepoMock
            .Setup(r => r.GetByIdAsync(familyMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mapperMock
            .Setup(m => m.Map<FamilyMemberDto>(entity))
            .Returns(dto);

        // Act
        var result = await _sut.GetFamilyMemberByIdAsync(familyMemberId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(familyMemberId);
    }

    #endregion

    #region GetFamilyMembersByFamilyIdAsync

    [Fact]
    public async Task GetFamilyMembersByFamilyIdAsync_ShouldReturnMappedList()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        var entities = new List<FamilyMember>
        {
            new() { Id = Guid.NewGuid(), FamilyId = familyId },
            new() { Id = Guid.NewGuid(), FamilyId = familyId }
        };

        var dtos = new List<FamilyMemberDto>
        {
            new() { Id = entities[0].Id },
            new() { Id = entities[1].Id }
        };

        _familyMemberRepoMock
            .Setup(r => r.GetAllByFamilyIdAsync(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<FamilyMemberDto>>(entities))
            .Returns(dtos);

        // Act
        var result = await _sut.GetFamilyMembersByFamilyIdAsync(familyId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    #endregion

    #region CreateFamilyMemberAsync

    [Fact]
    public async Task CreateFamilyMemberAsync_ShouldCreateAndReturnFamilyMember()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = new CreateFamilyMememberRequest
        {
            FirstName = "Jane",
            LastName = "Doe"
        };

        var entity = new FamilyMember
        {
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var savedEntity = new FamilyMember
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var dto = new FamilyMemberDto
        {
            Id = savedEntity.Id
        };

        _mapperMock
            .Setup(m => m.Map<FamilyMember>(request))
            .Returns(entity);

        _familyMemberRepoMock
            .Setup(r => r.AddAsync(It.IsAny<FamilyMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        _mapperMock
            .Setup(m => m.Map<FamilyMemberDto>(savedEntity))
            .Returns(dto);

        // Act
        var result = await _sut.CreateFamilyMemberAsync(request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _familyMemberRepoMock.Verify(r =>
            r.AddAsync(It.IsAny<FamilyMember>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteFamilyMemberByIdAsync

    [Fact]
    public async Task DeleteFamilyMemberByIdAsync_ShouldCallRepository()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        await _sut.DeleteFamilyMemberByIdAsync(familyMemberId, userId, CancellationToken.None);

        // Assert
        _familyMemberRepoMock.Verify(r =>
            r.DeleteByIdAsync(familyMemberId, userId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateFamilyMemberAsync

    [Fact]
    public async Task UpdateFamilyMemberAsync_ShouldUpdateAndReturnFamilyMember()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = new UpdateFamilyMememberRequest
        {
            Id = Guid.NewGuid(),
            FirstName = "Updated",
            LastName = "Name"
        };

        var entity = new FamilyMember
        {
            Id = request.Id,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var updatedEntity = new FamilyMember
        {
            Id = request.Id,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var dto = new FamilyMemberDto
        {
            Id = request.Id
        };

        _mapperMock
            .Setup(m => m.Map<FamilyMember>(request))
            .Returns(entity);

        _familyMemberRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<FamilyMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEntity);

        _mapperMock
            .Setup(m => m.Map<FamilyMemberDto>(updatedEntity))
            .Returns(dto);

        // Act
        var result = await _sut.UpdateFamilyMemberAsync(request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(request.Id);
    }

    #endregion
}
