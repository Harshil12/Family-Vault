using FamilyVault.Application.DTOs.LifeInsurance;
using FluentValidation;

namespace FamilyVault.Application.Validators.LifeInsurance;

public class UpdateLifeInsurancePolicyValidators : AbstractValidator<UpdateLifeInsurancePolicyRequest>
{
    public UpdateLifeInsurancePolicyValidators()
    {
        RuleFor(x => x.Id).NotEmpty();
        Include(new CreateLifeInsurancePolicyValidators());
    }
}
