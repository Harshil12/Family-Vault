using FamilyVault.Application.DTOs.Mediclaim;
using FamilyVault.Domain.Enums;
using FluentValidation;

namespace FamilyVault.Application.Validators.Mediclaim;

public class CreateMediclaimPolicyValidators : AbstractValidator<CreateMediclaimPolicyRequest>
{
    public CreateMediclaimPolicyValidators()
    {
        RuleFor(x => x.InsurerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PolicyNumber).NotEmpty().MaximumLength(60);
        RuleFor(x => x.PolicyType).NotEqual(MediclaimPolicyType.Unknown);
        RuleFor(x => x.Status).NotEqual(PolicyStatus.Unknown);
        RuleFor(x => x.SumInsured).GreaterThan(0);
        RuleFor(x => x.PremiumAmount).GreaterThan(0);
        RuleFor(x => x.FamilyMemberId).NotEmpty();
    }
}
