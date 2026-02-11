using AutoMapper;
using FamilyVault.Application.DTOs.Audit;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Services;
using FamilyVault.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FamilyVault.Tests.Services;

/// <summary>
/// Represents AuditServiceTests.
/// </summary>
public class AuditServiceTests
{
    private readonly Mock<IAuditRepository> _auditRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AuditService _sut;

    /// <summary>
    /// Initializes a new instance of AuditServiceTests.
    /// </summary>
    public AuditServiceTests()
    {
        _auditRepoMock = new Mock<IAuditRepository>();
        _mapperMock = new Mock<IMapper>();
        _sut = new AuditService(_auditRepoMock.Object, _mapperMock.Object);
    }

    #region LogAsync

    [Fact]
    /// <summary>
    /// Performs the LogAsync_ShouldCreateAndPersistAuditEvent operation.
    /// </summary>
    public async Task LogAsync_ShouldCreateAndPersistAuditEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AuditEvent? capturedEvent = null;

        _auditRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEvent, CancellationToken>((auditEvent, _) => capturedEvent = auditEvent)
            .ReturnsAsync((AuditEvent a, CancellationToken _) => a);

        // Act
        await _sut.LogAsync(
            userId,
            "Create",
            "Document",
            Guid.NewGuid(),
            "Created a document",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "127.0.0.1",
            "{\"ip\":\"127.0.0.1\"}",
            CancellationToken.None);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.UserId.Should().Be(userId);
        capturedEvent.Action.Should().Be("Create");
        capturedEvent.EntityType.Should().Be("Document");
        capturedEvent.CreatedBy.Should().Be(userId.ToString());
        capturedEvent.IsDeleted.Should().BeFalse();

        _auditRepoMock.Verify(r => r.AddAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetActivityAsync

    [Fact]
    /// <summary>
    /// Performs the GetActivityAsync_ShouldNormalizeInvalidDaysAndTake operation.
    /// </summary>
    public async Task GetActivityAsync_ShouldNormalizeInvalidDaysAndTake()
    {
        // Arrange
        var userId = Guid.NewGuid();
        DateTimeOffset capturedFromUtc = default;
        var capturedTake = 0;
        var events = new List<AuditEvent>();
        var mapped = new List<AuditEventDto>();

        _auditRepoMock
            .Setup(r => r.GetActivityByUserAsync(userId, It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, DateTimeOffset, int, CancellationToken>((_, fromUtc, take, _) =>
            {
                capturedFromUtc = fromUtc;
                capturedTake = take;
            })
            .ReturnsAsync(events);

        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<AuditEventDto>>(events))
            .Returns(mapped);

        var now = DateTimeOffset.UtcNow;

        // Act
        var result = await _sut.GetActivityAsync(userId, 0, 0, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        capturedTake.Should().Be(100);
        capturedFromUtc.Should().BeOnOrBefore(now.AddDays(-29));
        capturedFromUtc.Should().BeOnOrAfter(now.AddDays(-31));
    }

    [Fact]
    /// <summary>
    /// Performs the GetActivityAsync_ShouldCapDaysAndTakeAtMaxLimits operation.
    /// </summary>
    public async Task GetActivityAsync_ShouldCapDaysAndTakeAtMaxLimits()
    {
        // Arrange
        var userId = Guid.NewGuid();
        DateTimeOffset capturedFromUtc = default;
        var capturedTake = 0;

        _auditRepoMock
            .Setup(r => r.GetActivityByUserAsync(userId, It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, DateTimeOffset, int, CancellationToken>((_, fromUtc, take, _) =>
            {
                capturedFromUtc = fromUtc;
                capturedTake = take;
            })
            .ReturnsAsync(new List<AuditEvent>());

        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<AuditEventDto>>(It.IsAny<IReadOnlyList<AuditEvent>>()))
            .Returns(new List<AuditEventDto>());

        var now = DateTimeOffset.UtcNow;

        // Act
        await _sut.GetActivityAsync(userId, 9999, 9999, CancellationToken.None);

        // Assert
        capturedTake.Should().Be(1000);
        capturedFromUtc.Should().BeOnOrBefore(now.AddDays(-364));
        capturedFromUtc.Should().BeOnOrAfter(now.AddDays(-366));
    }

    #endregion

    #region GetDownloadHistoryAsync

    [Fact]
    /// <summary>
    /// Performs the GetDownloadHistoryAsync_ShouldNormalizeInvalidDaysAndTake operation.
    /// </summary>
    public async Task GetDownloadHistoryAsync_ShouldNormalizeInvalidDaysAndTake()
    {
        // Arrange
        var userId = Guid.NewGuid();
        DateTimeOffset capturedFromUtc = default;
        var capturedTake = 0;
        var events = new List<AuditEvent>();

        _auditRepoMock
            .Setup(r => r.GetDownloadHistoryByUserAsync(userId, It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, DateTimeOffset, int, CancellationToken>((_, fromUtc, take, _) =>
            {
                capturedFromUtc = fromUtc;
                capturedTake = take;
            })
            .ReturnsAsync(events);

        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<AuditEventDto>>(events))
            .Returns(new List<AuditEventDto>());

        var now = DateTimeOffset.UtcNow;

        // Act
        await _sut.GetDownloadHistoryAsync(userId, -1, -1, CancellationToken.None);

        // Assert
        capturedTake.Should().Be(100);
        capturedFromUtc.Should().BeOnOrBefore(now.AddDays(-29));
        capturedFromUtc.Should().BeOnOrAfter(now.AddDays(-31));
    }

    #endregion

    #region BuildCsvReportAsync

    [Fact]
    /// <summary>
    /// Performs the BuildCsvReportAsync_ShouldReturnCsvWithHeaderAndEscapedValues operation.
    /// </summary>
    public async Task BuildCsvReportAsync_ShouldReturnCsvWithHeaderAndEscapedValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var auditEvents = new List<AuditEvent>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "Download",
                EntityType = "Document",
                EntityId = Guid.NewGuid(),
                Description = "Downloaded, with comma",
                IpAddress = "127.0.0.1",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        var mapped = new List<AuditEventDto>
        {
            new()
            {
                Id = auditEvents[0].Id,
                UserId = userId,
                Action = "Download",
                EntityType = "Document",
                EntityId = auditEvents[0].EntityId,
                Description = "Downloaded, with comma",
                IpAddress = "127.0.0.1",
                CreatedAt = auditEvents[0].CreatedAt
            }
        };

        _auditRepoMock
            .Setup(r => r.GetActivityByUserAsync(userId, It.IsAny<DateTimeOffset>(), 1000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(auditEvents);

        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<AuditEventDto>>(auditEvents))
            .Returns(mapped);

        // Act
        var csv = await _sut.BuildCsvReportAsync(userId, 30, CancellationToken.None);

        // Assert
        csv.Should().Contain("TimestampUtc,Action,EntityType,EntityId,FamilyId,FamilyMemberId,DocumentId,Description,IpAddress,ActorUserId");
        csv.Should().Contain("\"Downloaded, with comma\"");
        csv.Should().Contain("Download");
    }

    #endregion
}
