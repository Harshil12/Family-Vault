using FamilyVault.Application.DTOs.DematAccounts;
using FamilyVault.Domain.Enums;
using FluentValidation;

namespace FamilyVault.Application.Validators.DematAccounts;

public class CreateDematAccountValidators : AbstractValidator<CreateDematAccountRequest>
{
    public CreateDematAccountValidators()
    {
        RuleFor(x => x.BrokerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Depository).NotEqual(DepositoryType.Unknown);
        RuleFor(x => x.DPId).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.HoldingPattern).NotEqual(HoldingPatternType.Unknown);
        RuleFor(x => x.FamilyMemberId).NotEmpty();
    }
}
