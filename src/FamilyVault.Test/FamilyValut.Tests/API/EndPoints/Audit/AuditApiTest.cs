using FamilyVault.API;
using FamilyVault.Application.DTOs.Audit;
using FamilyVault.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;

namespace FamilyVault.Tests.API.EndPoints.Audit;

/// <summary>
/// Represents AuditApiTest.
/// </summary>
public class AuditApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IAuditService> _auditServiceMock = new();

    /// <summary>
    /// Initializes a new instance of AuditApiTest.
    /// </summary>
    public AuditApiTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_auditServiceMock.Object);

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
            });
        });
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Test");
        return client;
    }

    [Fact]
    /// <summary>
    /// Performs the GetActivity_ShouldReturnOk operation.
    /// </summary>
    public async Task GetActivity_ShouldReturnOk()
    {
        // Arrange
        _auditServiceMock
            .Setup(s => s.GetActivityAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AuditEventDto>());

        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/audit/activity");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    /// <summary>
    /// Performs the GetDownloads_ShouldReturnOk operation.
    /// </summary>
    public async Task GetDownloads_ShouldReturnOk()
    {
        // Arrange
        _auditServiceMock
            .Setup(s => s.GetDownloadHistoryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AuditEventDto>());

        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/audit/downloads");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    /// <summary>
    /// Performs the ExportAuditReport_ShouldReturnCsvFile operation.
    /// </summary>
    public async Task ExportAuditReport_ShouldReturnCsvFile()
    {
        // Arrange
        _auditServiceMock
            .Setup(s => s.BuildCsvReportAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("a,b,c");

        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/audit/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    }
}

