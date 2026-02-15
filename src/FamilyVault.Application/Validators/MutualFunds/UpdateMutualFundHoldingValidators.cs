using FamilyVault.Application.DTOs.MutualFunds;
using FluentValidation;

namespace FamilyVault.Application.Validators.MutualFunds;

public class UpdateMutualFundHoldingValidators : AbstractValidator<UpdateMutualFundHoldingRequest>
{
    public UpdateMutualFundHoldingValidators()
    {
        RuleFor(x => x.Id).NotEmpty();
        Include(new CreateMutualFundHoldingValidators());
    }
}
