using FamilyVault.Application.DTOs.MutualFunds;
using FamilyVault.Domain.Enums;
using FluentValidation;

namespace FamilyVault.Application.Validators.MutualFunds;

public class CreateMutualFundHoldingValidators : AbstractValidator<CreateMutualFundHoldingRequest>
{
    public CreateMutualFundHoldingValidators()
    {
        RuleFor(x => x.AMCName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.FolioNumber).NotEmpty().MaximumLength(60);
        RuleFor(x => x.SchemeName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SchemeType).NotEqual(MutualFundSchemeType.Unknown);
        RuleFor(x => x.PlanType).NotEqual(MutualFundPlanType.Unknown);
        RuleFor(x => x.OptionType).NotEqual(MutualFundOptionType.Unknown);
        RuleFor(x => x.InvestmentMode).NotEqual(InvestmentModeType.Unknown);
        RuleFor(x => x.FamilyMemberId).NotEmpty();
    }
}
