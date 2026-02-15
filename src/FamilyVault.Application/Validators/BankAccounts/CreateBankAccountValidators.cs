using FamilyVault.Application.DTOs.BankAccounts;
using FamilyVault.Domain.Enums;
using FluentValidation;

namespace FamilyVault.Application.Validators.BankAccounts;

/// <summary>
/// Represents CreateBankAccountValidators.
/// </summary>
public class CreateBankAccountValidators : AbstractValidator<CreateBankAccountRequest>
{
    /// <summary>
    /// Initializes a new instance of CreateBankAccountValidators.
    /// </summary>
    public CreateBankAccountValidators()
    {
        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required.")
            .MaximumLength(150).WithMessage("Bank name must be at most 150 characters.");

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required.")
            .Matches(@"^\d{6,20}$").WithMessage("Account number must contain 6 to 20 digits.");

        RuleFor(x => x.AccountType)
            .NotEqual(BankAccountType.Unknown).WithMessage("Account type is required.");

        RuleFor(x => x.FamilyMemberId)
            .NotEmpty().WithMessage("Family member is required.");

        RuleFor(x => x.AccountHolderName)
            .MaximumLength(150).WithMessage("Account holder name must be at most 150 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.AccountHolderName));

        RuleFor(x => x.IFSC)
            .Matches(@"^[A-Z]{4}0[A-Z0-9]{6}$").WithMessage("IFSC must be in valid format.")
            .When(x => !string.IsNullOrWhiteSpace(x.IFSC));

        RuleFor(x => x.Branch)
            .MaximumLength(150).WithMessage("Branch must be at most 150 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Branch));

        RuleFor(x => x.NomineeName)
            .MaximumLength(150).WithMessage("Nominee name must be at most 150 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.NomineeName));
    }
}
