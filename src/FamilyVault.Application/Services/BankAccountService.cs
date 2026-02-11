using AutoMapper;
using FamilyVault.Application.DTOs.BankAccounts;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

/// <summary>
/// Represents BankAccountService.
/// </summary>
public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly ICryptoService _cryptoService;
    private readonly IMapper _mapper;
    private readonly ILogger<BankAccountService> _logger;

    /// <summary>
    /// Initializes a new instance of BankAccountService.
    /// </summary>
    public BankAccountService(
        IBankAccountRepository bankAccountRepository,
        ICryptoService cryptoService,
        IMapper mapper,
        ILogger<BankAccountService> logger)
    {
        _bankAccountRepository = bankAccountRepository;
        _cryptoService = cryptoService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Performs the GetBankAccountByIdAsync operation.
    /// </summary>
    public async Task<BankAccountDetailsDto> GetBankAccountByIdAsync(Guid bankAccountId, CancellationToken cancellationToken)
    {
        var result = await _bankAccountRepository.GetByIdAsync(bankAccountId, cancellationToken);

        if (result != null)
        {
            result.AccountNumber = _cryptoService.DecryptData(result.AccountNumber);
        }

        return _mapper.Map<BankAccountDetailsDto>(result);
    }

    /// <summary>
    /// Performs the GetBankAccountsByFamilyMemberIdAsync operation.
    /// </summary>
    public async Task<IReadOnlyList<BankAccountDetailsDto>> GetBankAccountsByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var result = await _bankAccountRepository.GetAllByFamilyMemberIdAsync(familyMemberId, cancellationToken);

        foreach (var account in result)
        {
            account.AccountNumber = _cryptoService.DecryptData(account.AccountNumber);
        }

        return _mapper.Map<List<BankAccountDetailsDto>>(result);
    }

    /// <summary>
    /// Performs the CreateBankAccountAsync operation.
    /// </summary>
    public async Task<BankAccountDetailsDto> CreateBankAccountAsync(CreateBankAccountRequest createBankAccountRequest, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating bank account for FamilyMemberId: {FamilyMemberId}", createBankAccountRequest.FamilyMemberId);

        var accountToCreate = _mapper.Map<BankAccountDetails>(createBankAccountRequest);
        accountToCreate.CreatedAt = DateTimeOffset.UtcNow;
        accountToCreate.CreatedBy = userId.ToString();
        accountToCreate.AccountNumberLast4 = GetLastFourDigits(accountToCreate.AccountNumber);
        accountToCreate.AccountNumber = _cryptoService.EncryptData(accountToCreate.AccountNumber);

        var result = await _bankAccountRepository.AddAsync(accountToCreate, cancellationToken);
        result.AccountNumber = createBankAccountRequest.AccountNumber;

        _logger.LogInformation("Created bank account with Id: {BankAccountId}", result.Id);

        return _mapper.Map<BankAccountDetailsDto>(result);
    }

    /// <summary>
    /// Performs the UpdateBankAccountAsync operation.
    /// </summary>
    public async Task<BankAccountDetailsDto> UpdateBankAccountAsync(UpdateBankAccountRequest updateBankAccountRequest, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating bank account with Id: {BankAccountId}", updateBankAccountRequest.Id);

        var accountToUpdate = _mapper.Map<BankAccountDetails>(updateBankAccountRequest);
        accountToUpdate.UpdatedAt = DateTimeOffset.UtcNow;
        accountToUpdate.UpdatedBy = userId.ToString();
        accountToUpdate.AccountNumberLast4 = GetLastFourDigits(accountToUpdate.AccountNumber);
        accountToUpdate.AccountNumber = _cryptoService.EncryptData(accountToUpdate.AccountNumber);

        var result = await _bankAccountRepository.UpdateAsync(accountToUpdate, cancellationToken);
        result.AccountNumber = updateBankAccountRequest.AccountNumber;

        _logger.LogInformation("Updated bank account with Id: {BankAccountId}", result.Id);

        return _mapper.Map<BankAccountDetailsDto>(result);
    }

    /// <summary>
    /// Performs the DeleteBankAccountByIdAsync operation.
    /// </summary>
    public async Task DeleteBankAccountByIdAsync(Guid bankAccountId, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting bank account with Id: {BankAccountId}", bankAccountId);

        await _bankAccountRepository.DeleteByIdAsync(bankAccountId, userId.ToString(), cancellationToken);

        _logger.LogInformation("Deleted bank account with Id: {BankAccountId}", bankAccountId);
    }

    private static string? GetLastFourDigits(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            return null;
        }

        return accountNumber.Length <= 4 ? accountNumber : accountNumber[^4..];
    }
}
