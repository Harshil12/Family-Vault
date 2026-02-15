using FamilyVault.API;
using FamilyVault.Application.DTOs.DematAccounts;
using FamilyVault.Application.DTOs.FixedDeposits;
using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.DTOs.LifeInsurance;
using FamilyVault.Application.DTOs.Mediclaim;
using FamilyVault.Application.DTOs.MutualFunds;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace FamilyVault.Tests.API.EndPoints.FinancialDetails;

public class FinancialDetailsApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IFixedDepositService> _fixedDepositServiceMock = new();
    private readonly Mock<ILifeInsuranceService> _lifeInsuranceServiceMock = new();
    private readonly Mock<IMediclaimService> _mediclaimServiceMock = new();
    private readonly Mock<IDematAccountService> _dematAccountServiceMock = new();
    private readonly Mock<IMutualFundService> _mutualFundServiceMock = new();
    private readonly Mock<IFamilyMemberService> _familyMemberServiceMock = new();
    private readonly Mock<IFamilyService> _familyServiceMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();

    public FinancialDetailsApiTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_fixedDepositServiceMock.Object);
                services.AddSingleton(_lifeInsuranceServiceMock.Object);
                services.AddSingleton(_mediclaimServiceMock.Object);
                services.AddSingleton(_dematAccountServiceMock.Object);
                services.AddSingleton(_mutualFundServiceMock.Object);
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
            .ReturnsAsync(new FamilyMemberDto { Id = familyMemberId, FamilyId = familyId, FirstName = "John" });

        _familyServiceMock
            .Setup(s => s.GetFamilyByIdAsync(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FamilyDto { Id = familyId, Name = "Test Family", UserId = TestAuthHandler.TestUserId });
    }

    [Fact]
    public async Task CreateFixedDeposit_ShouldReturnCreated()
    {
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);
        var request = new CreateFixedDepositRequest
        {
            InstitutionName = "SBI",
            DepositNumber = "12345678",
            DepositType = FixedDepositType.Cumulative,
            PrincipalAmount = 100000,
            InterestRate = 7.25m,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(1)),
            FamilyMemberId = familyMemberId
        };

        _fixedDepositServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateFixedDepositRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FixedDepositDetailsDto { Id = Guid.NewGuid(), FamilyMemberId = familyMemberId, InstitutionName = "SBI", DepositNumber = "12345678" });

        var response = await CreateClient().PostAsJsonAsync($"/financial-details/{familyMemberId}/fd", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateLifeInsurance_ShouldReturnCreated()
    {
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);
        var request = new CreateLifeInsurancePolicyRequest
        {
            InsurerName = "LIC",
            PolicyNumber = "LIFE12345",
            PolicyType = LifeInsurancePolicyType.Term,
            CoverAmount = 1000000,
            PremiumAmount = 15000,
            PremiumFrequency = PremiumFrequency.Yearly,
            PolicyStartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = PolicyStatus.Active,
            FamilyMemberId = familyMemberId
        };

        _lifeInsuranceServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateLifeInsurancePolicyRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LifeInsurancePolicyDetailsDto { Id = Guid.NewGuid(), FamilyMemberId = familyMemberId, InsurerName = "LIC", PolicyNumber = "LIFE12345" });

        var response = await CreateClient().PostAsJsonAsync($"/financial-details/{familyMemberId}/life-insurance", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateMediclaim_ShouldReturnCreated()
    {
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);
        var request = new CreateMediclaimPolicyRequest
        {
            InsurerName = "Star",
            PolicyNumber = "MED12345",
            PolicyType = MediclaimPolicyType.Individual,
            SumInsured = 500000,
            PremiumAmount = 12000,
            PolicyStartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            PolicyEndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(1)),
            Status = PolicyStatus.Active,
            FamilyMemberId = familyMemberId
        };

        _mediclaimServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateMediclaimPolicyRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MediclaimPolicyDetailsDto { Id = Guid.NewGuid(), FamilyMemberId = familyMemberId, InsurerName = "Star", PolicyNumber = "MED12345" });

        var response = await CreateClient().PostAsJsonAsync($"/financial-details/{familyMemberId}/mediclaim", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateDemat_ShouldReturnCreated()
    {
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);
        var request = new CreateDematAccountRequest
        {
            BrokerName = "Zerodha",
            Depository = DepositoryType.CDSL,
            DPId = "12012300",
            ClientId = "12345678",
            HoldingPattern = HoldingPatternType.Single,
            FamilyMemberId = familyMemberId
        };

        _dematAccountServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateDematAccountRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DematAccountDetailsDto { Id = Guid.NewGuid(), FamilyMemberId = familyMemberId, BrokerName = "Zerodha", ClientId = "12345678", DPId = "12012300" });

        var response = await CreateClient().PostAsJsonAsync($"/financial-details/{familyMemberId}/demat-accounts", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateMutualFund_ShouldReturnCreated()
    {
        var familyMemberId = Guid.NewGuid();
        SetupOwnership(familyMemberId);
        var request = new CreateMutualFundHoldingRequest
        {
            AMCName = "HDFC",
            FolioNumber = "FOL12345",
            SchemeName = "Balanced Fund",
            SchemeType = MutualFundSchemeType.Hybrid,
            PlanType = MutualFundPlanType.Direct,
            OptionType = MutualFundOptionType.Growth,
            InvestmentMode = InvestmentModeType.SIP,
            FamilyMemberId = familyMemberId
        };

        _mutualFundServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateMutualFundHoldingRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MutualFundHoldingDetailsDto { Id = Guid.NewGuid(), FamilyMemberId = familyMemberId, AMCName = "HDFC", FolioNumber = "FOL12345", SchemeName = "Balanced Fund" });

        var response = await CreateClient().PostAsJsonAsync($"/financial-details/{familyMemberId}/mutual-funds", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
