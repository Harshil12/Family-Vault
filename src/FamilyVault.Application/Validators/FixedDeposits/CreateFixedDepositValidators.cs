using FamilyVault.Application.DTOs.FixedDeposits;
using FamilyVault.Domain.Enums;
using FluentValidation;

namespace FamilyVault.Application.Validators.FixedDeposits;

public class CreateFixedDepositValidators : AbstractValidator<CreateFixedDepositRequest>
{
    public CreateFixedDepositValidators()
    {
        RuleFor(x => x.InstitutionName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DepositNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DepositType).NotEqual(FixedDepositType.Unknown);
        RuleFor(x => x.PrincipalAmount).GreaterThan(0);
        RuleFor(x => x.InterestRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FamilyMemberId).NotEmpty();
        RuleFor(x => x.MaturityDate).GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.NomineeName).MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.NomineeName));
    }
}
