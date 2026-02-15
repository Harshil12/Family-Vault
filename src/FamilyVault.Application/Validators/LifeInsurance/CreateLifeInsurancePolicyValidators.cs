using FamilyVault.Application.DTOs.LifeInsurance;
using FamilyVault.Domain.Enums;
using FluentValidation;

namespace FamilyVault.Application.Validators.LifeInsurance;

public class CreateLifeInsurancePolicyValidators : AbstractValidator<CreateLifeInsurancePolicyRequest>
{
    public CreateLifeInsurancePolicyValidators()
    {
        RuleFor(x => x.InsurerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PolicyNumber).NotEmpty().MaximumLength(60);
        RuleFor(x => x.PolicyType).NotEqual(LifeInsurancePolicyType.Unknown);
        RuleFor(x => x.PremiumFrequency).NotEqual(PremiumFrequency.Unknown);
        RuleFor(x => x.Status).NotEqual(PolicyStatus.Unknown);
        RuleFor(x => x.CoverAmount).GreaterThan(0);
        RuleFor(x => x.PremiumAmount).GreaterThan(0);
        RuleFor(x => x.FamilyMemberId).NotEmpty();
    }
}
