using FamilyVault.API;
using FamilyVault.Application.DTOs.BankAccounts;
using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace FamilyVault.Tests.API.EndPoints.BankAccount;

public class BankAccountApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IBankAccountService> _bankAccountServiceMock = new();
    private readonly Mock<IFamilyMemberService> _familyMemberServiceMock = new();
    private readonly Mock<IFamilyService> _familyServiceMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();

    public BankAccountApiTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_bankAccountServiceMock.Object);
                services.AddSingleton(_familyMemberServiceMock.Object);
                services.AddSingleton(_familyServiceMock.Object);
                services.AddSingleton(_auditServiceMock.Object);

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
            });
        });

        _auditServiceMock
            .Setup(s => s.LogAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Test");
        return client;
    }

    private void SetupOwnership(Guid familyMemberId)
    {
        var familyId = Guid.NewGuid();

        _familyMemberServiceMock
            .Setup(s => s.GetFamilyMemberByIdAsync(familyMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FamilyMemberDto
            {
                Id = familyMemberId,
                FamilyId = familyId,
                FirstName = "John"
            });

        _familyServiceMock
            .Setup(s => s.GetFamilyByIdAsync(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FamilyDto
            {
                Id = familyId,
                Name = "Test Family",
                UserId = TestAuthHandler.TestUserId
            });
    }

    [Fact]
    public async Task GetBankAccounts_ShouldReturnEmptyList_WhenNoBankAccounts()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        _bankAccountServiceMock
            .Setup(s => s.GetBankAccountsByFamilyMemberIdAsync(familyMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<BankAccountDetailsDto>());

        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/financial-details/{familyMemberId}/bank-accounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBankAccountById_ShouldReturnNotFound_WhenBankAccountDoesNotExist()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        _bankAccountServiceMock
            .Setup(s => s.GetBankAccountByIdAsync(bankAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BankAccountDetailsDto?)null);

        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/financial-details/{familyMemberId}/bank-accounts/{bankAccountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBankAccount_ShouldReturnCreated()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        var request = new CreateBankAccountRequest
        {
            FamilyMemberId = familyMemberId,
            BankName = "SBI",
            AccountNumber = "1234567890",
            AccountType = BankAccountType.Savings
        };

        _bankAccountServiceMock
            .Setup(s => s.CreateBankAccountAsync(
                It.IsAny<CreateBankAccountRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BankAccountDetailsDto
            {
                Id = Guid.NewGuid(),
                FamilyMemberId = familyMemberId,
                BankName = "SBI",
                AccountNumber = "1234567890"
            });

        var client = CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/financial-details/{familyMemberId}/bank-accounts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task UpdateBankAccount_ShouldReturnOk()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        var request = new UpdateBankAccountRequest
        {
            Id = bankAccountId,
            FamilyMemberId = familyMemberId,
            BankName = "SBI",
            AccountNumber = "1234567890",
            AccountType = BankAccountType.Savings
        };

        _bankAccountServiceMock
            .Setup(s => s.UpdateBankAccountAsync(
                It.IsAny<UpdateBankAccountRequest>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BankAccountDetailsDto
            {
                Id = bankAccountId,
                FamilyMemberId = familyMemberId,
                BankName = "SBI",
                AccountNumber = "1234567890"
            });

        var client = CreateClient();

        // Act
        var response = await client.PutAsJsonAsync($"/financial-details/{familyMemberId}/bank-accounts/{bankAccountId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteBankAccount_ShouldReturnOk()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        SetupOwnership(familyMemberId);

        _bankAccountServiceMock
            .Setup(s => s.GetBankAccountByIdAsync(bankAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BankAccountDetailsDto
            {
                Id = bankAccountId,
                FamilyMemberId = familyMemberId,
                BankName = "SBI",
                AccountNumber = "1234567890"
            });

        _bankAccountServiceMock
            .Setup(s => s.DeleteBankAccountByIdAsync(bankAccountId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var client = CreateClient();

        // Act
        var response = await client.DeleteAsync($"/financial-details/{familyMemberId}/bank-accounts/{bankAccountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFinancialDetails_ShouldReturnBadRequest_WhenCategoryIsUnsupported()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/financial-details/{familyMemberId}/unsupported-category");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}


