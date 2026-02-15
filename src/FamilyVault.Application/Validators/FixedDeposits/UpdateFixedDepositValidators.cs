using FamilyVault.Application.DTOs.FixedDeposits;
using FluentValidation;

namespace FamilyVault.Application.Validators.FixedDeposits;

public class UpdateFixedDepositValidators : AbstractValidator<UpdateFixedDepositRequest>
{
    public UpdateFixedDepositValidators()
    {
        RuleFor(x => x.Id).NotEmpty();
        Include(new CreateFixedDepositValidators());
    }
}
