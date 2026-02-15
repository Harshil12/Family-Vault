using FamilyVault.Application.DTOs.Mediclaim;
using FluentValidation;

namespace FamilyVault.Application.Validators.Mediclaim;

public class UpdateMediclaimPolicyValidators : AbstractValidator<UpdateMediclaimPolicyRequest>
{
    public UpdateMediclaimPolicyValidators()
    {
        RuleFor(x => x.Id).NotEmpty();
        Include(new CreateMediclaimPolicyValidators());
    }
}
