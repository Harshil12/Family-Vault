using FamilyVault.Application.DTOs.DematAccounts;
using FluentValidation;

namespace FamilyVault.Application.Validators.DematAccounts;

public class UpdateDematAccountValidators : AbstractValidator<UpdateDematAccountRequest>
{
    public UpdateDematAccountValidators()
    {
        RuleFor(x => x.Id).NotEmpty();
        Include(new CreateDematAccountValidators());
    }
}
